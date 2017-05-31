using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Websdepot
{

    class Program
    {
        static public string logUrl = "./log/Log.txt";
        static public string confUrl = "./Conf.cfg";
        static public string todayDate = DateTime.Now.ToString();
        static public string postUrl = "./post/post.csv";
        static List<Chunk> chunks = new List<Chunk>();        


        //======================================================
        //This function will write logMessage to log file.
        //Will create a new one if it doesn't exist.
        //Will also automatically add a timestamp in ISO format.
        //======================================================
        public static void writeLog(string logMessage)
        {
            //This will create a log if it doesn't exist
            StreamWriter sw = new StreamWriter(logUrl, true);

            logMessage = todayDate + "| " + logMessage;

            sw.WriteLine(logMessage);
            sw.Close();

        }


        //=======================================================
        //This function creates an MD5 hash of the Conf.cfg file.
        //Then passes it into createHashFile.
        //=======================================================
        private static string createHash()
        {


            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(confUrl))
                {

                    string fileHash = Encoding.Default.GetString(md5.ComputeHash(stream));
                    createHashFile(fileHash);
                    return fileHash;
                }
            }
        }

        //================================================
        //Creates hash file of the Conf.cfg file.
        //Should be repurposed to store hash in resgistry.
        //================================================
        private static void createHashFile(string hash)
        {
            /*
            string root = "HKEY_LOCAL_MACHINE\\Software\\Websdepot Reboot\\";
            string valueName = "ConfigHash";
            string value = hash;
            
            Registry.SetValue(root, valueName, hash);
            */

            StreamWriter sw = new StreamWriter("./md5", false);
            sw.WriteLine(hash);
            sw.Close();
        }

        //====================================================
        //This function causes the program to wait min minutes
        //====================================================
        private static void delayWait(int min)
        {
            int minToSec = 1000 * 60 * min;
            System.Threading.Thread.Sleep(minToSec);
        }

        static void Main(string[] args)
        {
            Directory.CreateDirectory("./log");
            Directory.CreateDirectory("./post");

            writeLog("Starting program");
            createHashFile(createHash());
            Toolbox magicBox = new Toolbox();
            ConfStore cStore = new ConfStore();
            magicBox.clearCsv();
            readConf(cStore, magicBox);

            
            magicBox.connectSql();
            magicBox.updateConfInSql(cStore);
            magicBox.runStart();
            magicBox.checkCsv();


            magicBox.uploadCsv();


            List<string> strTest = new List<string>();
            strTest.Add("C:\\Program Files (x86)\\Notepad++\notepad++.exe");

            Chunk cTest = new Chunk(strTest);

            cStore.changeChunk(cTest, 3);

            exit(0);
            TimeSpan sqlInterval = TimeSpan.FromMilliseconds(magicBox.getSqlInterval());
            TimeSpan rebootInterval = TimeSpan.FromMilliseconds(magicBox.getSqlInterval());
            //magicBox.runReb();
            var sqlTimer = new System.Threading.Timer((e) =>
            {
                //magicBox.updateLastCheckin();
                //TODO: Compare [Configured Reboot Times] with the SQL server, if they are different update the config file and write to the log file.
                //string hash = createHash();
                //magicBox.checkPostQueue();
                System.Console.WriteLine("2 Minutes has passed");
                if (magicBox.checkConnection())
                {

                }
            }, null, TimeSpan.Zero, sqlInterval);



            var rebootTimer = new System.Threading.Timer((f) =>
            {
                System.Console.WriteLine("5 minutes has passed");
                if (magicBox.checkRebootTime())
                {
                    magicBox.runReb();
                }
            }, null, TimeSpan.Zero, rebootInterval);

            while (true) //Prevents timer thread death
            {

            }
            
        }

        //=========================================================
        //Exits the program.
        //This should only occur if a problem has been encountered.
        //
        // Exit codes:
        //              0: Exit without problems
        //              1. Exit with error in conf file
        //              2. Exit with file open error
        //              3. Exit with CSV error
        //              4. Exit with SQL query error
        //=========================================================
        public static void exit(int code)
        {
            if (code > 0)
            {
                writeLog("Exitting program with error " + code);
            }
            else
            {
                writeLog("Exitting program");
            }

            
            System.Environment.Exit(code);
        }

        //=========================================
        //Read the Conf.cfg file
        //Chunks refers to each field in the file.
        //=========================================
        private static void readConf(ConfStore i_cs, Toolbox tIn)
        {
            if (!File.Exists(confUrl))
            {
                writeLog("Conf file is missing");
                exit(1);
            }
            StreamReader sr = new StreamReader(confUrl, System.Text.Encoding.Default);

            string line = "";
            List<string> currentList = new List<string>();
            int currentChunk = 0;
            while ((line = sr.ReadLine()) != null) //until EOF
            {

                if (currentChunk < 6)
                //If still working on a chunk, added to the list
                {
                    if (line != "")
                    {
                        currentList.Add(line);
                        //System.Console.WriteLine(line);
                    }
                    else
                    {
                        //System.Console.WriteLine("This is a blank line!");


                        switch (currentChunk) //Check which chunk it is on
                        {
                            case 0:
                                //sqlChunk = currentList;
                                chunks.Add(new Chunk(currentList));
                                break;
                            case 1:
                                //rebootConfigChunk = currentList;
                                chunks.Add(new Chunk(currentList));
                                break;
                            case 2:
                                //startupChunk = currentList;
                                chunks.Add(new Chunk(currentList));
                                break;
                            case 3:
                                //rebootChunk = currentList;
                                chunks.Add(new Chunk(currentList));
                                break;
                            case 4:
                                //configuredRebootChunk = currentList;
                                chunks.Add(new Chunk(currentList));
                                break;
                            case 5:
                                chunks.Add(new Chunk(currentList));
                                break;
                            default:
                                currentList = null;
                                break;
                                /*
                                case 4:
                                    //lastRebootChunk = currentList;
                                    chunks.Add(new Chunk(currentList));
                                    break;
                                */
                        }
                        currentList = new List<string>();
                        currentChunk++;
                    }
                }

            }

            sr.Close();
            chunks.Add(new Chunk(currentList));
            List<string> external = new List<string>();
            if (!File.Exists("./lastreboottime.txt"))
            {
                StreamWriter writer = new StreamWriter("./lastreboottime.txt", false);
                writer.WriteLine("[Last Reboot Time]");

                PerformanceCounter uptime = new PerformanceCounter("System", "System Up Time");
                uptime.NextValue();
                TimeSpan t = TimeSpan.FromSeconds(uptime.NextValue());
                DateTime final = DateTime.Now.Subtract(t);
                System.Console.WriteLine(final);
                //tools.setLastReboot(final);
                writer.WriteLine(final);
                writer.Close();

                
            }
            StreamReader sr_b = new StreamReader("./lastreboottime.txt", System.Text.Encoding.Default);
            external.Add(sr_b.ReadLine());
            external.Add(sr_b.ReadLine());

            sr_b.Close();
            //Send chunks to functions here

            Toolbox tb = tIn;
            ConfStore cStore = i_cs;

            foreach (Chunk c in chunks)
            {
                ParserChain ts = new ParserChain(c.getChunk(), tb, cStore);
            }

            ParserChain tc = new ParserChain(external, tb, cStore);
            //string strTest = tb.ToString();
            //System.Console.WriteLine(tb.checkSql());
            //System.Console.WriteLine(startupChunk[1]);
            //Process.Start(startupChunk[1]);

            //ParserChain ts = new ParserChain(chunks[0].getChunk(), new Toolbox());

        }

    }

    /* =======================================================================================================================================================================================
     * ParserChain
     *  - Does a full, rule-less execution of the settings file
     *  - ParserChain parses and handles the settings tags in the config file
     *      - This initial chain handles the [startup] tag
     *  - ParserChain is the first in a chain of tag parsers which allows the script to handle settings tags regardless of order
     *  
     *  - Parser chain workflow:
     *     - Upon creation preprocess settings tag
     *     - Check if tag is handled by this particular parser
     *          -If yes:
     *              - execute code based on settings
     *          -If no:
     *              - Pass the settings chunk onto the next parser in the chain
     * =======================================================================================================================================================================================
     */
    class ParserChain
    {
        /* =======================================================================================================================================================================================
         * ParserChain.ParserChain(List<string>, Toolbox)
         *   - The "default" constructor for ParserChain as Parsers always need a list of strings for commands and a toolbox for utilities
         *      - Stores input and sends it to preprocessing
         * =======================================================================================================================================================================================
         */
        public ParserChain(List<string> inChunk, Toolbox tIn, ConfStore cIn)
        {
            //Execute startup parser to initiate the chain
            StartupParser sP = new StartupParser(inChunk, tIn, true, cIn);
        }
    }


    /* =======================================================================================================================================================================================
     * Template method for 
     *  - name scheme for all the concrete parsers will follow the Tag[tag-name] naming convention
     *
     * Method operation chain:
     * 1) Create parser object
     *      - Object initiates tag keyword
     *      - Passes tag chunk to input sanitation function
     * 2) Input sanitation 
     *      - Sanitizes and stores the tag
     *      - Removes the tag from the chunk and stores it
     * 3) Process the chunk
     *      - Follow specific rules on a chunk specific basis
     *
     *      -By default, the ParserChain object parses the entire configuration file and executes it's functionality.
     *          -Mostly exists for testing purposes
     *  ***NOTE TO PEOPLE ADDING LINKS***
     *      - New end links should run the end of chain function which provides feedback if the code runs through the chain without finding the specific tag
     *        - Last chain in parser chain always calls chainEnd()
     *      
     * =======================================================================================================================================================================================
     */

    /*
     * 2 types of parsers will need to be made:
     *   1) settings tags parsers
     *         - these will deal with main settings tags and redirect action requests ex. [startup]\
     *   2) settings info parsers
     *         - these grab information from the config file and store it in the Toolbox object
     */

    abstract class Parser
    {
        //toolbox variable
        protected Toolbox tools;

        //variable which stores the parse word
        protected string strParse;

        //Sanitized Tag Input
        protected string strIn;

        //Raw chunk storage for when the tag isn't found
        protected List<string> rChunk;

        //Chunk storage
        protected List<string> lChunk;

        //Store configuration file data
        protected ConfStore cStore;


        /* =======================================================================================================================================================================================
         * cleanIn(List<string> strInput)
         *  Function:
         *   -Cleans the chunk data and prepares it for processing
         *   -Calls stringSwitch to decide whether this chain link performs the required actions for the settings
         * 
         * =======================================================================================================================================================================================
         */
        public void cleanIn(List<string> inChunk, bool blnChain)
        {
            //preprocess the settings tag
            string strUnIn;
            strUnIn = inChunk[0];
            strUnIn = strUnIn.Trim();
            strIn = strUnIn.ToLower();

            //store chunk
            lChunk = inChunk;


            if (blnChain)
            {
                //call switch function
                stringSwitch();
            }
            else
            {
                //pop off the options tag from the rest of the chunk
                lChunk.RemoveAt(0);

                //spawn specific subprocess parser
                spawnSub();
            }

        }

        //abstract function for performing settings actions
        public abstract void spawnSub();

        //abstract base class for going to next link in chain if this link isn't responsible for the settings tag
        public abstract void nextLink();

        /* =======================================================================================================================================================================================
         * TagParse.stringSwitch()
         *  - determines if the link handles this settings tag
         *      - if yes then pop off the settings tag and redirect it to the subprocess handler
         *      - if no then redirect it to the next link in the chain
         * =======================================================================================================================================================================================
         */
        public virtual void stringSwitch()
        {
            if (strIn.Equals(strParse))
            {
                //if the chunk belongs to this chain link then remove the tag and process it
                System.Console.WriteLine(strIn + " found option handler link");

                //pop off the options tag from the rest of the chunk
                lChunk.RemoveAt(0);

                //Store settings tag into ConfStore
                cStore.addTag(strParse);

                //Store Chunk into ConfStore
                cStore.addChunk(new Chunk(lChunk));

                //spawn specific subprocess parser
                spawnSub();
            }
            else
            {
                //Call next in chain
                //Chain tail should ouput a log
                System.Console.WriteLine(strIn + " link does not handle this option, going to next");
                nextLink();
            }
        }

        /* =======================================================================================================================================================================================
         * TagParse.execute()
         *  - executes/launches programs based on input stored in the chunk
         * =======================================================================================================================================================================================
         */
        public virtual void execute()
        {
            foreach (string strChunk in lChunk)
            {
                /*
                 * TEST CODE
                //read takes in string literals, no need for extra processing
                //chunks are super volitile right now needs further testing
                strProcessed = strChunk.Replace("\\", "\\\\");
                Process.Start(strProcessed);
                */

                //try to launch programs and catch if it fails
                try
                {
                    string strPath, strArgs;
                    string[] strRegEx = new string[] { ".exe " };
                    string[] strSplit;
                    strArgs = " ";
                    
                    //split the line twice using ".exe" as the split point
                    strSplit = strChunk.Split(strRegEx, 2, StringSplitOptions.None);

                    //if the first half of the program does not have ".exe" because of the split process append it to the back of the first half
                    //this is done so the integrity of the executable path is maintained
                    if (!strSplit[0].Contains(".exe"))
                    {
                        strSplit[0] = strSplit[0] + ".exe";
                    }

                    //store the path in a variable for easy accessibility and to improve 
                    strPath = strSplit[0];

                    //Grabs the command line arguments
                    // - try/catch block used because command line arguments might not be there and the strSplit[1] index might not exist
                    // - it is normal for the code to continue even after the exception is caught (cmd args not found)
                    try
                    {
                        strArgs = strSplit[1];
                    }
                    catch (Exception e) { }


                    System.Console.WriteLine("Executing: " + strPath + " | " + strArgs);

                    //launch process with path and command line arguments
                    var process = Process.Start(strPath, strArgs);

                    //wait for process to end
                    process.WaitForExit();
                }
                catch (Win32Exception e)
                {
                    Program.writeLog("Failed to open " + strChunk + ": " + e.Message);
                    Program.exit(2);

                }

            }
        }



        /* =======================================================================================================================================================================================
         * Parser.chainEnd()
         *   - Run by the last parser in the chain to log that an unknown setting has not been processed by the script as it has not been implemented or incorrectly named
         * =======================================================================================================================================================================================
         */
        public virtual void chainEnd()
        {
            Program.writeLog(strIn + "Command not found, check configuration file formatting or check if specific command has been implemented.");
        }
    }

    //********************************************************************************************************************************************************************************************
    //settings tag parser chain begins

    class StartupParser : Parser
    {
        /* =======================================================================================================================================================================================
         * StartupParser.StartupParser(List<string>, Toolbox)
         *   - The "default" constructor for StartupParser as Parsers always need a list of strings for commands and a toolbox for utilities
         *      - Stores input and sends it to preprocessing
         * =======================================================================================================================================================================================
         */
        public StartupParser(List<string> inChunk, Toolbox tIn, ConfStore cIn)
        {
            //Configuration store (prep for SQL upload)
            cStore = cIn;

            //Toolbox to use if the parser needs utilites implemented within it
            tools = tIn;

            //System.Console.WriteLine("StartupParser entered");

            //set parser keyword/tag
            strParse = "[startup]";

            //start tag preprocessing
            cleanIn(inChunk, false);
        }
        /* =======================================================================================================================================================================================
         * StartupParser.StartupParser(List<string>, Toolbox, bool)
         *   - Constructor for chain link
         * =======================================================================================================================================================================================
         */
        public StartupParser(List<string> inChunk, Toolbox tIn, bool blnChain, ConfStore cIn)
        {
            //Configuration store (prep for SQL upload)
            cStore = cIn;

            //Toolbox to use if the parser needs utilites implemented within it
            tools = tIn;

            //System.Console.WriteLine("StartupParser entered");

            //set parser keyword/tag
            strParse = "[startup]";

            //start tag preprocessing
            cleanIn(inChunk, blnChain);
        }

        /* =======================================================================================================================================================================================
         * StartupParser.spawnSub()
         *   - Store startup parser in Toolbox
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            //store Startup parser in Toolbox
            tools.setStartupParser(this);
        }

        /* =======================================================================================================================================================================================
         * RebootParser.run()
         *   - Run [reboot] tag options
         *     - execute programs on reboot (run execute())
         * =======================================================================================================================================================================================
         */
        public void run()
        {
            execute();
        }

        /* =======================================================================================================================================================================================
         * StartupParser.nextLink()
         *   - Pass chunk onto the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            System.Console.WriteLine("In [startup] chain link, going to next link");
            RebootParser rlLink = new RebootParser(lChunk, tools, true, cStore);
        }
    }
    /* =======================================================================================================================================================================================
     * RebootLink
     *  - Concrete implementation of Parser abstract class
     *  - RebootLink parses and handles the [reboot] in the config file
     *  - Is a Parser in the settings tag parser chain
     * =======================================================================================================================================================================================
     */
    class RebootParser : Parser
    {
        /* =======================================================================================================================================================================================
         * RebootParser.RebootParser(List<string>, Toolbox)
         *   - The "default" constructor for RebootParser as Parsers always need a list of strings for commands and a toolbox for utilities
         *      - Stores input and sends it to preprocessing
         * =======================================================================================================================================================================================
         */
        public RebootParser(List<string> inChunk, Toolbox tIn, ConfStore cIn)
        {
            //Configuration store (prep for SQL upload)
            cStore = cIn;

            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("RebootParser entered");

            //set parser keyword/tag
            strParse = "[reboot]";
            cleanIn(inChunk, false);
        }

        /* =======================================================================================================================================================================================
         * RebootParser.RebootParser(List<string>, Toolbox)
         *   - Constructor for chain link
         * =======================================================================================================================================================================================
         */
        public RebootParser(List<string> inChunk, Toolbox tIn, bool blnChain, ConfStore cIn)
        {
            //Configuration store (prep for SQL upload)
            cStore = cIn;

            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("RebootParser entered");

            //set parser keyword/tag
            strParse = "[reboot]";
            cleanIn(inChunk, blnChain);
        }

        /* =======================================================================================================================================================================================
         * RebootParser.spawnSub()
         *   - Run [reboot] tag options
         *     - execute programs on reboot (run execute())
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            tools.setRebootParser(this);
        }

        /* =======================================================================================================================================================================================
         * RebootParser.run()
         *   - Run [reboot] tag options
         *     - execute programs on reboot (run execute())
         * =======================================================================================================================================================================================
         */
        public void run()
        {
            foreach (string strChunk in lChunk)
            {
                if (strChunk.Contains("shutdown"))
                {
                    if (strChunk.Contains("-r") || strChunk.Contains("/r"))
                    {
                        System.Console.WriteLine("Now rebooting...");
                        StreamWriter sw = new StreamWriter("./lastreboottime.txt", false);
                        sw.WriteLine("[Last Reboot Time]");

                        PerformanceCounter uptime = new PerformanceCounter("System", "System Up Time");
                        uptime.NextValue();
                        TimeSpan t = TimeSpan.FromSeconds(uptime.NextValue());
                        DateTime final = DateTime.Now.Subtract(t);
                        System.Console.WriteLine(final);
                        //tools.setLastReboot(final);
                        sw.WriteLine(final);
                        sw.Close();
                    }
                }

            }
            execute();
        }

        /* =======================================================================================================================================================================================
         * RebootParser.nextParser()
         *   - Pass chunk onto the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            System.Console.WriteLine("In [reboot] chain Parser, going to next Parser");
            SqlParser sqlParser = new SqlParser(lChunk, tools, true, cStore);
            //throw new NotImplementedException();
        }
    }

    /* =======================================================================================================================================================================================
     * SqlParser
     *  - Concrete implementation of Parser abstract class
     *  - SqlParser parses and handles the [sql config] tag in the config file
     *  - Is a Parser in the settings tag parser chain
     * =======================================================================================================================================================================================
     */
    class SqlParser : Parser
    {
        /* =======================================================================================================================================================================================
         * SqlParser.SqlParser(List<string>, Toolbox)
         *   - The "default" constructor for SqlParser as Parsers always need a list of strings for commands and a toolbox for utilities
         *      - Stores input and sends it to preprocessing
         * =======================================================================================================================================================================================
         */
        public SqlParser(List<string> inChunk, Toolbox tIn, ConfStore cIn)
        {
            //Configuration store (prep for SQL upload)
            cStore = cIn;

            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlParser entered");
            strParse = "[sql config]";
            cleanIn(inChunk, false);
        }

        /* =======================================================================================================================================================================================
         * SqlParser.SqlParser(List<string>, Toolbox)
         *   - The "default" constructor for SqlParser as Parsers always need a list of strings for commands and a toolbox for utilities
         *      - Stores input and sends it to preprocessing
         * =======================================================================================================================================================================================
         */
        public SqlParser(List<string> inChunk, Toolbox tIn, bool blnChain, ConfStore cIn)
        {
            //Configuration store (prep for SQL upload)
            cStore = cIn;

            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlParser entered");
            strParse = "[sql config]";
            cleanIn(inChunk, blnChain);
        }

        /* =======================================================================================================================================================================================
         * SqlParser.spawnSub()
         *   - Run [sql config] tag options
         *     - Grabs all sql connection settings
         *          - Achieve this by passing each settings line into a parser chain which parses the information and stores it into the passed in toolbox
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            //go through each individual chunk line and pass it into the sql line parcer chain
            foreach (string strIn in lChunk)
            {
                SqlParserChain sqlParse = new SqlParserChain(strIn, tools);
            }
        }

        /* =======================================================================================================================================================================================
         * SqlParser.nextParser()
         *   - Pass chunk onto the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            System.Console.WriteLine("In [sql config] chain Parser");
            RebConfParser rcParser = new RebConfParser(lChunk, tools, true, cStore);
        }
    }

    /* =======================================================================================================================================================================================
     * RebConfParser
     *  - Concrete implementation of Parser abstract class
     *  - RebConfParser parses and handles the [reboot config] tag in the config file
     *  - Is a Parser in the settings tag parser chain
     * =======================================================================================================================================================================================
     */
    class RebConfParser : Parser
    {
        /* =======================================================================================================================================================================================
         * RebConfParser.RebConfParser(List<string>, Toolbox)
         *   - The "default" constructor for RebConfParser as Parsers always need a list of strings for commands and a toolbox for utilities
         *      - Stores input and sends it to preprocessing
         * =======================================================================================================================================================================================
         */
        public RebConfParser(List<string> inChunk, Toolbox tIn, ConfStore cIn)
        {
            //Configuration store (prep for SQL upload)
            cStore = cIn;

            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("RebConfParser entered");

            //set parser keyword/tag
            strParse = "[reboot config]";
            cleanIn(inChunk, false);
        }

        /* =======================================================================================================================================================================================
         * RebConfParser.RebConfParser(List<string>, Toolbox)
         *   - Constructor for chain link
         * =======================================================================================================================================================================================
         */
        public RebConfParser(List<string> inChunk, Toolbox tIn, bool blnChain, ConfStore cIn)
        {
            //Configuration store (prep for SQL upload)
            cStore = cIn;

            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("RebConfParser entered");

            //set parser keyword/tag
            strParse = "[reboot config]";
            cleanIn(inChunk, blnChain);
        }

        /* =======================================================================================================================================================================================
         * RebConfParser.spawnSub()
         *   - Run [reboot] tag options
         *     - Process reboot configurations
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            //reboot configurations go here
            foreach (string strIn in lChunk)
            {
                RebConfParserChain sqlParse = new RebConfParserChain(strIn, tools);
            }
        }

        /* =======================================================================================================================================================================================
         * RebConfParser.nextParser()
         *   - Pass chunk onto the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            System.Console.WriteLine("In [reboot config] chain Parser, going to next Parser");
            RebTimeParser crtParser = new RebTimeParser(lChunk, tools, true, cStore);
            //throw new NotImplementedException();
        }
    }

    /* =======================================================================================================================================================================================
     * RebTimeParser
     *  - Concrete implementation of Parser abstract class
     *  - RebTimeParser parses and handles the [configured reboot time] tag in the config file
     *  - Is a Parser in the settings tag parser chain
     * =======================================================================================================================================================================================
     */
    class RebTimeParser : Parser
    {
        /* =======================================================================================================================================================================================
         * RebTimeParser.RebTimeParser(List<string>, Toolbox)
         *   - The "default" constructor for RebTimeParser as Parsers always need a list of strings for commands and a toolbox for utilities
         *      - Stores input and sends it to preprocessing
         * =======================================================================================================================================================================================
         */
        public RebTimeParser(List<string> inChunk, Toolbox tIn, ConfStore cIn)
        {
            //Configuration store (prep for SQL upload)
            cStore = cIn;

            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("RebTimeParser entered");

            //set parser keyword/tag
            strParse = "[configured reboot times]";
            cleanIn(inChunk, false);
        }
        /* =======================================================================================================================================================================================
         * RebTimeParser.RebTimeParser(List<string>, Toolbox)
         *   - Constructor for chain link
         * =======================================================================================================================================================================================
         */
        public RebTimeParser(List<string> inChunk, Toolbox tIn, bool blnChain, ConfStore cIn)
        {
            //Configuration store (prep for SQL upload)
            cStore = cIn;

            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("RebTimeParser entered");

            //set parser keyword/tag
            strParse = "[configured reboot times]";
            cleanIn(inChunk, blnChain);
        }

        /* =======================================================================================================================================================================================
         * RebTimeParser.spawnSub()
         *   - Run [reboot] tag options
         *     - Process reboot configurations
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            foreach (string strIn in lChunk)
            {
                RebTimeParserChain rebTimeParse = new RebTimeParserChain(strIn, tools);
            }
        }

        /* =======================================================================================================================================================================================
         * RebTimeParser.nextParser()
         *   - Pass chunk onto the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            System.Console.WriteLine("In configured reboot times chain Parser, going to next Parser");
            LastRebootParser lrParser = new LastRebootParser(lChunk, tools, true, cStore);
            //throw new NotImplementedException();
        }
    }

    /* =======================================================================================================================================================================================
     * LastRebootParser
     *  - Concrete implementation of Parser abstract class
     *  - LastRebootParser parses and handles the [last reboot time] tag in the config file
     *  - Is a Parser in the settings tag parser chain
     * =======================================================================================================================================================================================
     */
    class LastRebootParser : Parser
    {
        /* =======================================================================================================================================================================================
         * LastRebootParser.LastRebootParser(List<string>, Toolbox)
         *   - The "default" constructor for LastRebootParser as Parsers always need a list of strings for commands and a toolbox for utilities
         *      - Stores input and sends it to preprocessing
         * =======================================================================================================================================================================================
         */
        public LastRebootParser(List<string> inChunk, Toolbox tIn, ConfStore cIn)
        {
            //Configuration store (prep for SQL upload)
            cStore = cIn;

            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("LastRebootParser entered");

            //set parser keyword/tag
            strParse = "[last reboot time]";
            cleanIn(inChunk, false);
        }

        /* =======================================================================================================================================================================================
         * LastRebootParser.LastRebootParser(List<string>, Toolbox)
         *   - Constructor for chain link
         * =======================================================================================================================================================================================
         */
        public LastRebootParser(List<string> inChunk, Toolbox tIn, bool blnChain, ConfStore cIn)
        {
            //Configuration store (prep for SQL upload)
            cStore = cIn;

            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("LastRebootParser entered");

            //set parser keyword/tag
            strParse = "[last reboot time]";
            cleanIn(inChunk, blnChain);
        }

        /* =======================================================================================================================================================================================
         * LastRebootParser.spawnSub()
         *   - Run [reboot] tag options
         *     - Process last reboot time and store it
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            //last reboot time configurations go here
            foreach (string x in lChunk)
            {
                DateTime dt = Convert.ToDateTime(x);
                tools.setLastReboot(dt);
                System.Console.WriteLine(dt);
            }


        }

        /* =======================================================================================================================================================================================
         * LastRebootParser.nextParser()
         *   - Pass chunk onto the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            System.Console.WriteLine("In [last reboot time] config chain Parser, going to next Parser");
            chainEnd();
        }
    }

    //end of tag parser family

    //********************************************************************************************************************************************************************************************

    //beginning of info parser chain family

    //''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    /* =======================================================================================================================================================================================
     * InfoParserChain
     *  - Abstract implementation of Parser abstract class
     *  - Acts as the base for all info parsers
     *  
     *  - Parser chain workflow:
     *     - Upon creation preprocess data line
     *     - Check if data is handled by this particular parser
     *          -If yes:
     *              - execute code based on settings
     *          -If no:
     *              - Pass the settings chunk onto the next parser in the chain
     * =======================================================================================================================================================================================
     */
    abstract class InfoParserChain : Parser
    {
        protected string[] strRaw;

        //default constructor should never be touched
        protected InfoParserChain()
        {
            strParse = "";
        }

        /* =======================================================================================================================================================================================
         * InfoParserChain.InfoParserChain(string, Toolbox)
         *   - The "default" constructor for SqlLink as Parsers always need a settings line for commands and a toolbox for utilities
         *      - Stores input and sends it to the decision-making switch
         * =======================================================================================================================================================================================
         */

        /* =======================================================================================================================================================================================
         * InfoParserChain.stringSwitch()
         *   - Decides if the line passed in is processed by this parser and redirects it to either parsing or the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void stringSwitch()
        {
            //Check if the line passed contains the token this link is responsible for...
            if (strIn.Contains(strParse))
            {
                //if yes, process:
                stringParse();
            }
            else
            {
                nextLink();
            }
        }

        /* =======================================================================================================================================================================================
         * InfoParserChain.stringParse()
         *   - Splits the line
         * =======================================================================================================================================================================================
         */
        public void stringParse()
        {
            strRaw = strIn.Split('=');
            try
            {
                strIn = strRaw[1];
            }
            catch (Exception e)
            {
                //System.Console.WriteLine("Setting data missing");
                Program.writeLog("Something wrong with the conf file: " + e.Message);
                Program.exit(1);
            }

            spawnSub();
        }

        /* =======================================================================================================================================================================================
         * InfoParserChain.chainEnd()
         *   - Run by the last parser in the chain to log that a line of supposed SQL creds have not been stored into the object
         * =======================================================================================================================================================================================
         */
        public override void chainEnd()
        {
            Program.writeLog(strIn + "Invalid data");
        }
    }

    //''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    //Reboot Configuration parser chain begins

    //''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    /*=======================================================================================================================================================================================
     * RebConfParserChain
     *  -This parser chain deals with all the configurations of the [reboot config] tag
     *=======================================================================================================================================================================================
     */
    class RebConfParserChain : InfoParserChain
    {
        public RebConfParserChain(string strLine, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlLink entered");
            strParse = "AllowedTime=";
            strIn = strLine.Replace(" ", String.Empty);

            //overriden string parse
            stringSwitch();
        }

        public override void stringSwitch()
        {
            //Check if the line passed contains the token this link is responsible for...
            if (strIn.Contains(strParse))
            {
                //if yes, process:
                stringParse();
            }
            else
            {
                nextLink();
            }
        }

        public override void nextLink()
        {
            RebDelayLink rbDelay = new RebDelayLink(strIn, tools);
        }

        public override void spawnSub()
        {
            //tools.setRebootConfig(0, strIn);

            string[] splitA = strIn.Split(',');

            foreach (string x in splitA)
            {
                tools.setRebootTimes(new DayRange(x));
            }
        }
    }

    /*=======================================================================================================================================================================================
     * RebDelayLink
     *  -This parser link deals with all the CheckDelay setting
     *=======================================================================================================================================================================================
     */
    class RebDelayLink : InfoParserChain
    {
        public RebDelayLink(string strLine, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlLink entered");
            strParse = "CheckDelay=";
            strIn = strLine.Replace(" ", String.Empty);

            //overriden string parse
            stringSwitch();
        }

        public override void stringSwitch()
        {
            //Check if the line passed contains the token this link is responsible for...
            if (strIn.Contains(strParse))
            {
                //if yes, process:
                stringParse();
            }
            else
            {
                nextLink();
            }
        }

        public override void nextLink()
        {
            chainEnd();
        }

        /* =======================================================================================================================================================================================
         * RebDelayLink.spawnSub()
         *   - Stores the reboot check delay info
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            tools.setSql(5, strIn);
            string[] strSplit = strIn.Split(',');
            tools.setRebDelay(strSplit[0], strSplit[1]);
        }
    }

    //SQL info parser chain begins
    /* =======================================================================================================================================================================================
     * SqlParserChain
     *  - Concrete implementation of Parser abstract class
     *  - SqlParserChain parses and stores the SQL access information
     *      -This initial link checks for and stores the host information
     *  - SqlParserChain is the first in a chain of information parsres which allows the script to grab and store SQL data even if it isn't in the right order
     *  
     *  - Parser chain workflow:
     *     - Upon creation preprocess data line
     *     - Check if data is handled by this particular parser
     *          -If yes:
     *              - execute code based on settings
     *          -If no:
     *              - Pass the settings chunk onto the next parser in the chain
     * =======================================================================================================================================================================================
     */
    class SqlParserChain : InfoParserChain
    {
        //protected string strIn;
        //protected string[] strRaw;

        //default constructor should never be touched


        /* =======================================================================================================================================================================================
         * SqlParserChain.SqlParserChain(string, Toolbox)
         *   - The "default" constructor for SqlLink as Parsers always need a settings line for commands and a toolbox for utilities
         *      - Stores input and sends it to the decision-making switch
         * =======================================================================================================================================================================================
         */
        public SqlParserChain(string strLine, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlLink entered");
            strParse = "Host=";
            strIn = strLine.Replace(" ", String.Empty);

            //overriden string parse
            stringSwitch();
        }

        /* =======================================================================================================================================================================================
         * SqlParserChain.stringSwitch()
         *   - Decides if the line passed in is processed by this parser and redirects it to either parsing or the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void stringSwitch()
        {
            //Check if the line passed contains the token this link is responsible for...
            if (strIn.Contains(strParse))
            {
                //if yes, process:
                stringParse();
            }
            else
            {
                nextLink();
            }
        }

        /* =======================================================================================================================================================================================
         * SqlParserChain.stringParse()
         *   - Splits the line
         * =======================================================================================================================================================================================
         */
        public void stringParse()
        {
            strRaw = strIn.Split('=');
            try
            {
                strIn = strRaw[1];
            }
            catch (Exception e)
            {
                //System.Console.WriteLine("SQL data setting data missing.");
                Program.writeLog("Conf file error: " + e);
                Program.writeLog("SQL data error");
                Program.exit(1);
            }

            spawnSub();
        }

        /* =======================================================================================================================================================================================
         * SqlParserChain.spawnSub()
         *   - Stores the SQL info in its appropriate spot in the Toolbox object
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            tools.setSql(0, strIn);
        }

        /* =======================================================================================================================================================================================
         * SqlParserChain.nextLink()
         *   - Redirects the line to the next parser in the parser chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            SqlPortLink sqlPort = new SqlPortLink(strIn, tools);
        }

        /* =======================================================================================================================================================================================
         * SqlParserChain.chainEnd()
         *   - Run by the last parser in the chain to log that a line of supposed SQL creds have not been stored into the object
         * =======================================================================================================================================================================================
         */
        public override void chainEnd()
        {
            Program.writeLog(strIn + "SQL connection data is invalid, check configuration file formatting or check if specific data piece is needed to connect to your SQL connection.");
        }
    }

    /* =======================================================================================================================================================================================
     * SqlPortLink
     *  - SqlPortLink parses and stores the port information
     *  - Is a Parser in the SQL info parser chain
     * =======================================================================================================================================================================================
     */
    class SqlPortLink : InfoParserChain
    {
        /* =======================================================================================================================================================================================
         * SqlPortLink.SqlPortLink(string, Toolbox)
         *   - The "default" constructor for SqlLink as Parsers always need a settings line for commands and a toolbox for utilities
         *      - Stores input and sends it to the decision-making switch
         * =======================================================================================================================================================================================
         */
        public SqlPortLink(string strLine, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlLink entered");
            strParse = "Port=";
            strIn = strLine.Replace(" ", String.Empty);
            //overriden string parse
            stringSwitch();
        }

        /* =======================================================================================================================================================================================
         * SqlPortLink.spawnSub()
         *   - Stores the SQL info in its appropriate spot in the Toolbox object
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            tools.setSql(1, strIn);
        }

        /* =======================================================================================================================================================================================
         * SqlPortLink.nextLink()
         *   - Redirects the line to the next parser in the parser chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            SqlUnameLink sqlPort = new SqlUnameLink(strIn, tools);
        }
    }

    /* =======================================================================================================================================================================================
     * SqlUnameLink
     *  - SqlUnameLink parses and stores the username information
     *  - Is a Parser in the SQL info parser chain
     * =======================================================================================================================================================================================
     */
    class SqlUnameLink : InfoParserChain
    {
        /* =======================================================================================================================================================================================
         * SqlUnameLink.SqlUnameLink(string, Toolbox)
         *   - The "default" constructor for SqlUnameLink as Parsers always need settings line and a toolbox for utilities
         *      - Stores input and sends it to the decision-making switch
         * =======================================================================================================================================================================================
         */
        public SqlUnameLink(string strLine, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlLink entered");
            strParse = "Username=";
            strIn = strLine.Replace(" ", String.Empty);
            //overriden string parse
            stringSwitch();
        }

        /* =======================================================================================================================================================================================
         * SqlUnameLink.spawnSub()
         *   - Stores the SQL info in its appropriate spot in the Toolbox object
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            tools.setSql(2, strIn);
        }

        /* =======================================================================================================================================================================================
         * SqlUnameLink.nextLink()
         *   - Redirects the line to the next parser in the parser chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            SqlPwordLink sqlPort = new SqlPwordLink(strIn, tools);
        }
    }

    /* =======================================================================================================================================================================================
     * SqlPwordLink
     *  - SqlPwordLink parses and stores the password information
     *  - Is a Parser in the SQL info parser chain
     * =======================================================================================================================================================================================
     */
    class SqlPwordLink : InfoParserChain
    {
        /* =======================================================================================================================================================================================
         * SqlPwordLink.SqlPwordLink(string, Toolbox)
         *   - The "default" constructor for SqlPwordLink as Parsers always need settings line and a toolbox for utilities
         *      - Stores input and sends it to the decision-making switch
         * =======================================================================================================================================================================================
         */
        public SqlPwordLink(string strLine, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlLink entered");
            strParse = "Password=";
            strIn = strLine.Replace(" ", String.Empty);
            //overriden string parse
            stringSwitch();
        }

        /* =======================================================================================================================================================================================
         * SqlPwordLink.spawnSub()
         *   - Stores the SQL info in its appropriate spot in the Toolbox object
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            tools.setSql(3, strIn);
        }

        /* =======================================================================================================================================================================================
         * SqlPwordLink.nextLink()
         *   - Redirects the line to the next parser in the parser chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            SqlDbLink sqlPort = new SqlDbLink(strIn, tools);
        }
    }

    /* =======================================================================================================================================================================================
     * SqlDbLink
     *  - SqlDbLink parses and stores the database information
     *  - Is a Parser in the SQL info parser chain
     * =======================================================================================================================================================================================
     */
    class SqlDbLink : InfoParserChain
    {
        /* =======================================================================================================================================================================================
         * SqlDbLink.SqlDbLink(string, Toolbox)
         *   - The "default" constructor for SqlDbLink as Parsers always need settings line and a toolbox for utilities
         *      - Stores input and sends it to the decision-making switch
         * =======================================================================================================================================================================================
         */
        public SqlDbLink(string strLine, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlLink entered");
            strParse = "Database=";
            strIn = strLine.Trim();
            //overriden string parse
            stringSwitch();
        }

        /* =======================================================================================================================================================================================
         * SqlDbLink.spawnSub()
         *   - Stores the SQL info in its appropriate spot in the Toolbox object
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            tools.setSql(4, strIn);
        }

        /* =======================================================================================================================================================================================
         * SqlDbLink.nextLink()
         *   - Redirects the line to the next parser in the parser chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            SqlCInLink sqlPort = new SqlCInLink(strIn, tools);
        }
    }

    /* =======================================================================================================================================================================================
     * SqlCInLink
     *  - SqlCInLink parses and stores the check in time information
     *  - Is a Parser in the SQL info parser chain
     * =======================================================================================================================================================================================
     */
    class SqlCInLink : InfoParserChain
    {
        /* =======================================================================================================================================================================================
         * SqlCInLink.SqlDbLink(string, Toolbox)
         *   - The "default" constructor for SqlCInLink as Parsers always need settings line and a toolbox for utilities
         *      - Stores input and sends it to the decision-making switch
         * =======================================================================================================================================================================================
         */
        public SqlCInLink(string strLine, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlLink entered");
            strParse = "CheckinTime=";
            strIn = strLine.Replace(" ", String.Empty);
            //overriden string parse
            stringSwitch();
        }

        /* =======================================================================================================================================================================================
         * SqlCInLink.spawnSub()
         *   - Stores the SQL info in its appropriate spot in the Toolbox object
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            tools.setSql(5, strIn);
            string[] strSplit = strIn.Split(',');
            tools.setSqlInterval(strSplit[0], strSplit[1]);
        }

        /* =======================================================================================================================================================================================
         * SqlCInLink.nextLink()
         *   - Redirects the line to the next parser in the parser chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            //currently the last chain in the link so this runs chainEnd()
            chainEnd();
        }
    }

    //SQL info parser chain ends

    //''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    //Reboot Configuration parser chain begins

    //''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    //Start of the Configured Reboot Time settings parsers
    /* =======================================================================================================================================================================================
     * RebTimeParserChain
     *  - RebTimeParserChain parses and stores the check in time information
     *  - Is a Parser in the Reboot Time parser chain
     * =======================================================================================================================================================================================
     */
    class RebTimeParserChain : InfoParserChain
    {
        /* =======================================================================================================================================================================================
         * RebTimeParserChain.SqlDbLink(string, Toolbox)
         *   - The "default" constructor for RebTimeParserChain as Parsers always need settings line and a toolbox for utilities
         *      - Stores input and sends it to the decision-making switch
         * =======================================================================================================================================================================================
         */
        public RebTimeParserChain(string strLine, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlLink entered");
            strParse = "Start=";
            //strIn = strLine.Replace(" ", String.Empty);
            strIn = strLine;
            //overriden string parse
            stringSwitch();
        }

        /* =======================================================================================================================================================================================
         * RebTimeParserChain.spawnSub()
         *   - Stores the SQL info in its appropriate spot in the Toolbox object
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            //ADD DATE LOGIC
            DateTime dt = Convert.ToDateTime(strIn);
            tools.setConfiguredRebootTime(dt);
        }

        /* =======================================================================================================================================================================================
         * RebTimeParserChain.nextLink()
         *   - Redirects the line to the next parser in the parser chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            //currently the last chain in the link so this runs chainEnd()
            RebTimeIntLink rbtLink = new RebTimeIntLink(strIn, tools);
        }
    }

    /* =======================================================================================================================================================================================
     * RebTimeIntLink
     *  - RebTimeIntLink parses and stores the check in time information
     *  - Is a Parser in the Reboot Time parser chain
     * =======================================================================================================================================================================================
     */
    class RebTimeIntLink : InfoParserChain
    {
        /* =======================================================================================================================================================================================
         * RebTimeIntLink.SqlDbLink(string, Toolbox)
         *   - The "default" constructor for RebTimeIntLink as Parsers always need settings line and a toolbox for utilities
         *      - Stores input and sends it to the decision-making switch
         * =======================================================================================================================================================================================
         */
        public RebTimeIntLink(string strLine, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlLink entered");
            strParse = "Interval=";
            strIn = strLine.Replace(" ", String.Empty);
            //overriden string parse
            stringSwitch();
        }

        /* =======================================================================================================================================================================================
         * RebTimeIntLink.spawnSub()
         *   - Stores the SQL info in its appropriate spot in the Toolbox object
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            //tools.setSql(5, strIn);            
            string[] strSplit = strIn.Split(',');
            tools.setRebInterval(strSplit[0], strSplit[1]);
        }

        /* =======================================================================================================================================================================================
         * RebTimeIntLink.nextLink()
         *   - Redirects the line to the next parser in the parser chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            //currently the last chain in the link so this runs chainEnd()
            chainEnd();
        }
    }

    //end of parser family

    /* =======================================================================================================================================================================================
     * Toolbox
     *   - Toolbox contains utilities to check information which will be used by the main program and also acts as a storage location 
     *     for all the information which the parser stores for the main program to use
     * =======================================================================================================================================================================================
     */
    class Toolbox
    {
        string[] sqlInfo;
        DateTime dtLastReb;
        long rebootDelay, rebootInterval, sqlInterval;
        List<DayRange> allowedRebootTimes;
        StartupParser stParse;
        RebootParser rbParse;
        DateTime configuredRebootTime;
        MySqlConnection connect;
        string localHash;
        string strMachine;

        /*=======================================================================================================================================================================================
         * Toolbox.Toolbox()
         *      - Default constructor for Toolbox
         *          - Initiates the empty sqlInfo array
         * =======================================================================================================================================================================================
         */
        public Toolbox()
        {
            sqlInfo = new string[6];
            strMachine = System.Environment.MachineName;
            allowedRebootTimes = new List<DayRange>();
            StreamReader sr = new StreamReader("./md5");
            localHash = sr.ReadLine();
            sr.Close();
        }

        //===========================================
        //Connect to SQL server as per Conf.cfg file.
        //===========================================
        public void connectSql()
        {
            //attempt connection here
            Program.writeLog("Attempting to connect to SQL");

            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();

            string[] sqlInfo = getSql();
            builder.Add("Server", sqlInfo[0]);
            builder.Add("Port", sqlInfo[1]);
            builder.Add("Uid", sqlInfo[2]);
            builder.Add("Pwd", sqlInfo[3]);
            builder.Add("Database", sqlInfo[4]);
            //Console.WriteLine(builder.ConnectionString);

            connect = new MySqlConnection();
            connect.ConnectionString = builder.ConnectionString;

            try
            {
                connect.Open();
                Program.writeLog("Connection to SQL success");
            }
            catch (Exception e)
            {
                Program.writeLog("Connection to SQL failed: " + e.Message);
                //Program.exit(2);
            }

        }

        public bool checkConnection()
        {
            return connect.Ping();
        }

        public bool checkRebootTime()
        {
            foreach (DayRange period in allowedRebootTimes)
            {
                if (period.inDayRange() && period.inTimeRange())
                {
                    return true;
                }
            }
            return false;
        }

        public void updateLastCheckin()
        {
            //TODO: Update SQL for last check in date TALK TO ROY ABOUT ANOTHER TABLE FOR IT
            //sqlQuery("");
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = connect;
                cmd.CommandText = "test";//TODO change query
                cmd.Prepare();

                cmd.Parameters.AddWithValue("@test", null);
                 
            }catch(Exception e)
            {
                Program.writeLog("Something went wrong with updating last check in date: " + e.Message);
                Program.exit(4);
            }

        }

        /*=======================================================================================================================================================================================
          * Toolbox.setStartupParser()
          *      - Setter for reboot times
          * =======================================================================================================================================================================================
          */
        public void setStartupParser(StartupParser stIn)
        {
            stParse = stIn;
        }

        /*=======================================================================================================================================================================================
         * Toolbox.runStart()
         *      - Execute startup
         * =======================================================================================================================================================================================
         */
        public void runStart()
        {
            stParse.run();
        }

        /*=======================================================================================================================================================================================
         * Toolbox.setRebootTimes()
         *      - Setter for reboot times
         * =======================================================================================================================================================================================
         */
        public void setRebootTimes(DayRange i)
        {

            allowedRebootTimes.Add(i);
        }

        /*=======================================================================================================================================================================================
         * Toolbox.setRebootParser()
         *      - Setter for reboot times
         * =======================================================================================================================================================================================
         */
        public void setRebootParser(RebootParser rebIn)
        {
            rbParse = rebIn;
        }

        /*=======================================================================================================================================================================================
         * Toolbox.setConfiguredRebootTime()
         *      - Setter for configured reboot times
         * =======================================================================================================================================================================================
         */
        public void setConfiguredRebootTime(DateTime i)
        {
            configuredRebootTime = i;
        }

        /*=======================================================================================================================================================================================
         * Toolbox.runReb()
         *      - Execute reboot
         * =======================================================================================================================================================================================
         */
        public void runReb()
        {
            rbParse.run();
        }

        /*=======================================================================================================================================================================================
         * Toolbox.intervalMath(string, string)
         *      - Takes in 2 strings
         *          - string1 contains the time which will be parsed
         *          - string2 contains the measurement
         *      - Returns an int in milliseconds
         *      
         * =======================================================================================================================================================================================
         */
        public long intervalMath(string strTime, string strInterval)
        {
            long intMs = 0;
            //string[] strInterval;
            int intT;

            //strInterval = strInterval.Split(',');

            intT = int.Parse(strTime);

            //if tree for intervals

            //convert from seconds
            intMs = intT * 1000;
            strInterval = strInterval.ToLower();
            //check if seconds is needed conversion
            if (strInterval.Equals("s") || strInterval.Equals("second"))
            {
                //if yes return
                return intMs;
            }
            else
            {
                //convert to minutes
                intMs = intMs * 60;
            }

            if (strInterval.Equals("m") || strInterval.Equals("min") || strInterval.Equals("minute"))
            {
                //if yes return
                return intMs;
            }
            else
            {
                //if yes return
                intMs = intMs * 60;
            }

            if (strInterval.Equals("h") || strInterval.Equals("hour"))
            {
                //if yes return
                return intMs;
            }
            else
            {
                intMs = intMs * 24;
            }

            if (strInterval.Equals("d") || strInterval.Equals("day"))
            {
                //if yes return
                return intMs;
            }
            else
            {
                intMs = intMs * 7;
            }
            if (strInterval.Equals("w") || strInterval.Equals("week"))
            {
                //if yes return
                return intMs;
            }
            else
            {
                intMs = intMs * 30;
            }
            if (strInterval.Equals("month") || strInterval.Equals("mon"))
            {
                //month is the largest
                return intMs;
            }
            else
            {
                //conversion tag invalid
                System.Console.WriteLine("Conversion unit does not exist");
                return -1;
            }
        }

        /*=======================================================================================================================================================================================
         * Toolbox.setRebInterval()
         *      -Set the interval for reboots
         * =======================================================================================================================================================================================
         */
        public void setRebInterval(string strTime, string strInterval)
        {
            rebootInterval = intervalMath(strTime, strInterval);
        }

        /*=======================================================================================================================================================================================
         * Toolbox.setSqlInterval()
         *      -Set the interval for checking SQL
         * =======================================================================================================================================================================================
         */
        public void setSqlInterval(string strTime, string strInterval)
        {
            sqlInterval = intervalMath(strTime, strInterval);
        }


        /*=======================================================================================================================================================================================
         * Toolbox.setRebDelay()
         *      -Set the reboot delay
         * =======================================================================================================================================================================================
         */
        public void setRebDelay(string strTime, string strInterval)
        {
            rebootDelay = intervalMath(strTime, strInterval);
        }

        /* =======================================================================================================================================================================================
         * Toolbox.setSql(int, string)
         *   - Set the SQL information into the storage fields of the program
         *      - int argument is the index 
         *      - string is the associated data/cred
         *   - Note the function assumes developer knows the structure of the sqlInfo array
         * =======================================================================================================================================================================================
         */
        public void setSql(int intIndex, string strIn)
        {
            /*
             * =======================================================================================================================================================================================
             * index for SQL info array:
             * 
             * 0: Host IP (eg: 192.168.0.10)
             * 1: Port number (eg: 1433)
             * 2: Username (eg: SomeUser)
             * 3: Password (eg: SomePassword)
             * 4: Database (eg: SomeDb)
             * 5: CheckinTime (by minutes?) (eg: 2) //will be stored as a number in intSqlTime
             * =======================================================================================================================================================================================
             */
            sqlInfo[intIndex] = strIn;
        }

        /* =======================================================================================================================================================================================
         * Toolbox.getSql()
         *   - Returns the sqlInfo array so developer can use the SQL data
         * =======================================================================================================================================================================================
         */
        public string[] getSql()
        {
            return sqlInfo;
        }
        public long getSqlInterval()
        {
            return sqlInterval;
        }

        /* =======================================================================================================================================================================================
         * Toolbox.setLastReboot()
         *   - Set last reboot time
         * =======================================================================================================================================================================================
         */
        public void setLastReboot(DateTime dtIn)
        {
            dtLastReb = dtIn;
        }

        /* =======================================================================================================================================================================================
         * Toolbox.checkSql()
         *   - Verifies the sqlInfo array is fully populated
         *      - Returns true or false depending on if all the data has been filled in
         * =======================================================================================================================================================================================
         */
        public bool checkSql()
        {
            //parser orders info, so if it exists it is in the right spot
            //attempt to loop through the entire sqlInfo array, if successful, return true
            try
            {
                for (int i = 0; i < 6; i++)
                {
                    //dummy code to iterate through the array
                    sqlInfo[i].Contains("");
                }
                return true;
            }
            catch (Exception e)
            {
                Program.writeLog("Conf File error: " + e.Message);
                Program.writeLog("SQL data error");
                Program.exit(1);
                return false;
            }
        }

        /* =======================================================================================================================================================================================
         * Toolbox.sqlRebootTime()
         *   - Check if the reboot time on the Conf file matches the log file
         * =======================================================================================================================================================================================
         */
        public void sqlRebootTime()
        {
            //parser orders info, so if it exists it is in the right spot
            //attempt to loop through the entire sqlInfo array, if successful, return true
            try
            {
                for (int i = 0; i < 6; i++)
                {
                    //dummy code to iterate through the array
                    sqlInfo[i].Contains("");
                }
                //return true;
            }
            catch (Exception e)
            {
                Program.writeLog("Conf File error: " + e.Message);
                Program.writeLog("SQL data error");
                Program.exit(1);
                //return false;
            }
        }

        /* =======================================================================================================================================================================================
         * Toolbox.clearCsv()
         *   - Resets the CSV file
         * =======================================================================================================================================================================================
         */
        public void clearCsv()
        {
            if (File.Exists(Program.postUrl))
            {
                File.Delete(Program.postUrl);
            }
            StreamWriter sw = new StreamWriter(Program.postUrl);
            sw.WriteLine("StartupTime,ServerName,Status,Service,SubService,SubService,Error,");
            sw.Close();
        }

        //=====================
        //Attempt to upload CSV
        //=====================
        public void uploadCsv()
        {
            if (!checkConnection())
            {
                //Queue the CSV for next available upload
                string postStamp = "./post/topost/" + DateTime.Now.ToString("yyyyMMdd-HHmm") + "_Post.csv";
                File.Move(Program.postUrl, postStamp);
                Program.writeLog("Can not upload to SQL yet, Post has been queued");
            }
            else
            {
                //Upload CSV to SQL
                readCsvForUpload();
                Directory.CreateDirectory("./post/posted");
                string postStamp = "./post/posted/" + DateTime.Now.ToString("yyyyMMdd-HHmm") +  "_Post.csv";
                File.Move(Program.postUrl, postStamp);
                Program.writeLog("Post has been uploaded");
            }
        }
        
        public void readCsvForUpload()
        {
            
            DateTime lastModified = File.GetLastWriteTime(Program.postUrl);
            StreamReader sr = new StreamReader(Program.postUrl, System.Text.Encoding.Default);
            sr.ReadLine();
            string line;
            while (!string.IsNullOrEmpty(line = sr.ReadLine()))
            {
                List<string> csvLine = new List<string>();                
                string[] splitLine = line.Split(',');
                csvLine.AddRange(splitLine);
                csvLine.Add(lastModified.ToString());
                csvSqlInsert(csvLine);
            }
            sr.Close();
        }

        public void readCsvForUpload(string oldPost)
        {
            DateTime lastModified = File.GetLastWriteTime(oldPost);
            StreamReader sr = new StreamReader(Program.postUrl, System.Text.Encoding.Default);
            sr.ReadLine();
            string line;
            while (!string.IsNullOrEmpty(line = sr.ReadLine()))
            {
                List<string> csvLine = new List<string>();
                string[] splitLine = line.Split(',');
                csvLine.AddRange(splitLine);
                csvLine.Add(lastModified.ToString());
                csvSqlInsert(csvLine);
            }
            sr.Close();
        }

        public void csvSqlInsert(List<string> query)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = connect;
                cmd.CommandText = "INSERT INTO " +
                    "csv_service(csv_id, csv_startup, csv_server, csv_status, csv_service, csv_subservice, csv_error,  csv_timestmp) " +
                    "VALUES(@csv_id, @csv_startup, @csv_server, @csv_status, @csv_service, @csv_subservice, @csv_error, @csv_timestmp)";
                cmd.Prepare();

                cmd.Parameters.AddWithValue("@csv_id", null);
                cmd.Parameters.AddWithValue("@csv_startup", Convert.ToDateTime(query[0].Trim('"')));
                cmd.Parameters.AddWithValue("@csv_server", query[1].Trim('"'));
                cmd.Parameters.AddWithValue("@csv_status", query[2].Trim('"'));
                cmd.Parameters.AddWithValue("@csv_service", query[3].Trim('"'));
                cmd.Parameters.AddWithValue("@csv_subservice", query[4].Trim('"'));
                cmd.Parameters.AddWithValue("@csv_error", query[5].Trim('"'));                
                cmd.Parameters.AddWithValue("@csv_timestmp", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Program.writeLog("SQL query went wrong when writing to CSV sql:" + e.Message);
                Program.exit(4);
            }

        }

        //===================================
        //Upload CSVs that are not posted yet
        //===================================
        public void checkPostQueue()
        {
            var files = Directory.GetFiles("./post/topost/*.csv");
            if (files.Length > 0)
            {

                foreach (string file in files)
                {
                    //TODO: CSV upload to SQL here, then move them to posted after rename
                    string[] firstAttempt = file.Split('_');

                    Program.writeLog("Uploaded old log: " + Path.GetFileName(file));
                    File.Move(file, "./post/posted/" + firstAttempt[0] + "_" + Program.todayDate + "_Post.csv");
                }
            }
        }

        /* =======================================================================================================================================================================================
         * Toolbox.clearCsv()
         *   - Vreifies the integrity of the CSV file
         * =======================================================================================================================================================================================
         */
        public void checkCsv()
        {
            //The regex pattern is: ^"(.+)" ?,"(\w+)","(.+)","([A-Z]+)","(.+)","(.*)"$
            StreamReader sr;            
            bool error = false;
            string subject;
            sr = new StreamReader(Program.postUrl, System.Text.Encoding.Default);
            subject = sr.ReadLine(); //skips first line
            string pattern = "^\"(.+)\" ?,\"(\\w+)\",\"(.+)\",\"([A-Z]+)\",\"(.+)\",\"(.*)\"$";
            Regex rgx = new Regex(@pattern);

            while (!error && subject != null)
            {
                subject = sr.ReadLine();
                if (subject != null)
                {
                    if (!rgx.IsMatch(subject))
                    {
                        Program.writeLog("Error in the CSV file");
                        Program.exit(3);
                    }
                }
            }
            sr.Close();            
        }

        public bool compareHashWithSql()
        {
            //SELECT conf_md5hash FROM server_programs.configfile_info ORDER BY conf_id DESC LIMIT 1

            string query = "SELECT conf_md5hash FROM server_programs.configfile_info ORDER BY conf_id DESC LIMIT 1";
            MySqlCommand cmd = new MySqlCommand(query, connect);

            StreamReader sr = new StreamReader("./md5");
            string local = sr.ReadLine();
            sr.Close();

            string remote = Convert.ToString(cmd.ExecuteScalar());

            if(String.Compare(local, remote) == 0)
            {
                return true;
            } else
            {
                return false;
            }

        }

        public void updateConfInSql(ConfStore input)
        {

            DateTime uploadDate = File.GetLastWriteTime(Program.confUrl);

            List<string> tags = input.getTag();
            List<Chunk> chunks = input.getChunk();

            if (tags.Count != chunks.Count)
            {
                Program.writeLog("Something is wrong with conf file/conf file parsing");
                Program.exit(1);
            }
            else
            {
                for(int x = 0; x < tags.Count; x++)
                {
                    List<string> query = new List<string>();
                    query.Add(uploadDate.ToString());
                    query.Add(localHash);
                    query.Add(tags[x]);
                    query.Add(chunks[x].ToString());

                    writeConfLinetoSql(query);
                }
            }

        }

        public void writeConfLinetoSql(List<string> input)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = connect;
                cmd.CommandText = "INSERT INTO " +
                    "configfile_info(conf_id, conf_uldate, conf_md5hash, conf_tagline, conf_settings, conf_timestmp) " +
                    "VALUES(@conf_id, @conf_uldate, @conf_md5hash, @conf_tagline, @conf_settings, @conf_timestmp)";
                cmd.Prepare();

                cmd.Parameters.AddWithValue("@conf_id", null);
                cmd.Parameters.AddWithValue("@conf_uldate", Convert.ToDateTime(input[0]));
                cmd.Parameters.AddWithValue("@conf_md5hash", input[1]);                cmd.Parameters.AddWithValue("@conf_tagline", input[2]);
                cmd.Parameters.AddWithValue("@conf_settings", input[3]);
                cmd.Parameters.AddWithValue("@conf_timestmp", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Program.writeLog("Something went wrong when writing to Conf SQL: " + e.Message);
                Program.exit(4);
            }
        }

    }

    /* =======================================================================================================================================================================================
     * Chunk
     *   - List of strings
     *      - A chunk holds a block of settings 
     *          - A chunk starts from the settings tag and ends when a new line character is detected
     * =======================================================================================================================================================================================
     */
    class Chunk
    {
        List<string> lines;
        public Chunk(List<string> i)
        {
            lines = i;
        }

        public List<string> getChunk()
        {
            return lines;
        }
        public override string ToString()
        {
            string result = "";
            foreach(string line in lines)
            {
                result += line;
                result += "\n";    
            }
            return result;
        }

    }

    /* =======================================================================================================================================================================================
     * ConfStore
     *  - ConfStore stores the data of the config file and uses it for uploading
     * 
     * =======================================================================================================================================================================================
     */
     class ConfStore {
        //Parallel string lists which store the tag and associated Chunk lines
        List<string> lTag;
        List<Chunk> lChunk;

        //MD5 String
        string strMd5;

        public ConfStore()
        {
            lTag = new List<string>();
            lChunk = new List<Chunk>();
        }

        public void addTag(string strIn)
        {
            lTag.Add(strIn);
        }

        public void addChunk(Chunk cIn)
        {
            lChunk.Add(cIn);
        }

        public void changeChunk(Chunk cIn, int intIndex)
        {
            lChunk[intIndex] = cIn;
        }
        public List<string> getTag()
        {
            return lTag;
        }

        public List<Chunk> getChunk()
        {
            return lChunk;
        }
        
        public void writeConf()
        {
            //This will create a log if it doesn't exist
            StreamWriter sw = new StreamWriter(Program.confUrl, false);

            int intMax = 0;
            string strLine;
            //Verify integrity of chunks
            if(lChunk.Count == lTag.Count)
            {
                intMax = lChunk.Count;
            }else
            {
                //TODO add error code
                Program.writeLog("Could not update new Conf file");
                Program.exit(1);
            }

            for(int i = 0; i<intMax; i++)
            {
                sw.Write(lTag[i]);
                sw.Write(lChunk[i]);
                sw.Write("\n");
            }
            sw.Close();
        }
     }

    /* =======================================================================================================================================================================================
     * =======================================================================================================================================================================================
     */
    class DayRange
    {
        int dayX;
        int dayY;
        TimeSpan timeX;
        TimeSpan timeY;

        public DayRange()
        {

        }
        public DayRange(string i)
        {
            string[] splitA = i.Split('|');

            string[] splitDay = splitA[0].Split('-');
            if (splitDay.Length == 1)
            {
                dayX = twoLetterDay(splitDay[0]);
                dayY = dayX;
            }
            else
            {
                dayX = twoLetterDay(splitDay[0]);
                dayY = twoLetterDay(splitDay[1]);
            }
            string[] splitTime = splitA[1].Split('-');
            timeX = Convert.ToDateTime(splitTime[0]).TimeOfDay;
            timeY = Convert.ToDateTime(splitTime[1]).TimeOfDay;
        }

        //=====================================================
        // Check to see if today's day of week is within range
        // of the days of weeks outlined in parameters
        //=====================================================
        public bool inDayRange()
        {
            int i = (int)DateTime.Today.DayOfWeek;
            if (dayX < dayY)
            {
                if (i >= dayX && i <= dayY)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (dayX == dayY)
                {
                    if (i == dayX)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (i >= dayY || i <= dayX)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        //=============================================================
        // Check if current time is in the range of the times outlined
        // in parameters
        //=============================================================
        public bool inTimeRange()
        {
            TimeSpan i = DateTime.Now.TimeOfDay;

            if (timeY > timeX)
            {
                if (i >= timeX && i <= timeY)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (i >= timeX && i > timeY)
                {
                    return true;
                }
                else if (i < timeX && i <= timeY)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        //=================================================
        //Returns two letter weekday format as int.
        //Follows the same format the DateTime object uses.
        //=================================================
        public int twoLetterDay(string i)
        {
            if (String.Compare("su", i) == 0)
            {
                return 0;
            }
            else if (String.Compare("mo", i) == 0)
            {
                return 1;
            }
            else if (String.Compare("tu", i) == 0)
            {
                return 2;
            }
            else if (String.Compare("we", i) == 0)
            {
                return 3;
            }
            else if (String.Compare("th", i) == 0)
            {
                return 4;
            }
            else if (String.Compare("fr", i) == 0)
            {
                return 5;
            }
            else if (String.Compare("sa", i) == 0)
            {
                return 6;
            }
            else //In case of invalid input
            {
                return -1;
            }
        }
        public void getRebootTimeFromConf()
        {
            StreamReader sr = new StreamReader(Program.confUrl, System.Text.Encoding.Default);
            string line = "";
            while (String.Compare(line, "[Configured Reboot Times]") != 0)
            {
                line = sr.ReadLine();
            }

            string[] starSplit;
            line = sr.ReadLine();
            starSplit = line.Split('=');
            DateTime startTime = Convert.ToDateTime(starSplit[1]);

        }
    }


}