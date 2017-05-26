using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        //==============================================================
        //This function reads and checks the Conf.cfg file for validity.
        //==============================================================
        private static void readConf()
        {
            //Check goes here


            if (!false) //Conf does not clear check
            {

                writeLog("There is something wrong with the conf file");
                exit(1);
            }
            else //Conf clears check
                {
                    writeLog("Conf file is clear");
                    //Do hash configuration here
                    createHash();
                }
        }

        //=======================================================
        //This function creates an MD5 hash of the Conf.cfg file.
        //Then passes it into createHashFile.
        //=======================================================
        private static void createHash()
        {


            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(confUrl))
                {
                    createHashFile(Encoding.Default.GetString(md5.ComputeHash(stream)));
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

        //===========================================
        //Connect to SQL server as per Conf.cfg file.
        //===========================================
        private static void connectSql()
        {
            //attempt connection here
            writeLog("Attempting to connect to SQL");


            // TODO: find a way to get SQL connection credentials
            SqlConnection myConnection = new SqlConnection("user id=username;" +
                                       "password=password;server=serverurl;" +
                                       "Trusted_Connection=yes;" +
                                       "database=database; " +
                                       "connection timeout=30");

            try
            {
                myConnection.Open();
                writeLog("Connection to SQL success");
            }
            catch (Exception e)
                {
                    writeLog("Connection to SQL failed " + e);
                }

        }

        //====================================================
        //This function causes the program to wait min minutes
        //====================================================
        private static void delayWait(int min) 
        {
            int minToSec = 1000 * 60 * min;
            System.Threading.Thread.Sleep(minToSec);
        }

        //=====================================================
        //Check if Post.csv exists.  If yes, delete.
        //Then create a new one
        //=====================================================
        private static void checkPost()
        {
            if (File.Exists(postUrl))
            {
                File.Delete(postUrl);
                File.Create(postUrl);
            }
        }

        static void Main(string[] args)
        {
           
            Toolbox magicBox = new Toolbox();
            //Process.Start("shutdown", "/s /t 0");
            //Process.Start("shutdown", "-r -f -t 0");
            //Process.Start("C:\\Program Files (x86)\\Notepad++\\notepad++.exe");

            //readConf();

            //connectSql();
            //delayWait(5);


            //test replacement code
            /*
            string strChunk = "C:\\Program Files (x86)\\Notepad++\\notepad++.exe";
            string strReplace = "C:\\Program Files (x86)\\Notepad++\\notepad++.exe";
            strReplace = strChunk.Replace("\\", "\\\\");
            System.Console.WriteLine("strReplace " + strReplace);
            System.Console.WriteLine("strChunk: " + strChunk);
            */

            /*
            Process.Start("C:\\Program Files (x86)\\Notepad++\\notepad++.exe");
            System.Console.WriteLine("Notepad++ launch test");
            Process.Start("C:\\Windows\\System32\\taskkill.exe","-F -IM notepad++.exe");
            System.Console.WriteLine("Notepad++ is dead");
            Process.Start("C:\\Program Files (x86)\\Notepad++\\notepad++.exe", " ");
            System.Console.WriteLine("Notepad++ launch test");
            */

            //testing argument parse logic
            /*
            string strTest;
            string[] strRegEx = new string[] { ".exe " };
            string[] strSplit;
            strTest = "C:\\Windows\\System32\\taskkill.exe -F -IM notepad++.exe";
            System.Console.WriteLine("strTest control: " + strTest);
            strSplit = strTest.Split(strRegEx, StringSplitOptions.None);
            strSplit[0] = strSplit[0] + ".exe";
            System.Console.WriteLine("Split output test: " + strSplit[0]);
            System.Console.WriteLine("Split output test: " + strSplit[1]);
            System.Console.WriteLine("Test end ");
            */
            //createHash();
            //readChunks();

            //run full battery of tests
            
            readChunks(magicBox);

            
            
            //checkPost();

            //PLACE KILLSWITCH HERE
            //Process.Start("shutdown", "-r -f -t 0");

        }

        //=========================================================
        //Exits the program.
        //This should only occur if a problem has been encountered.
        //=========================================================
        public static void exit(int code)
        {
            if(code > 0)
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
        private static void readChunks(Toolbox tIn)
        {
            StreamReader sr = new StreamReader(confUrl, System.Text.Encoding.Default);

            string line;
            List<string> currentList = new List<string>();
            int currentChunk = 0;
            while((line = sr.ReadLine()) != null) //until EOF
            {
                if (currentChunk < 5) 
                    //If still working on a chunk, added to the list
                    //currentChunk == 4 means all chunks are done
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
                            /*
                            case 4:
                                //lastRebootChunk = currentList;
                                chunks.Add(new Chunk(currentList));
                                break;
                            */
                            case 4:
                                //configuredRebootChunk = currentList;
                                chunks.Add(new Chunk(currentList));
                                break;
                            default:
                                currentList = null;
                                break;
                        }
                        currentList = new List<string>();
                        currentChunk++;
                        }
                }
                
            }

            sr.Close();
            List<string> external = new List<string>();
            if (File.Exists("./lastreboottime.txt"))
            {
                StreamReader sr_b = new StreamReader("./lastreboottime.txt", System.Text.Encoding.Default);                
                external.Add(sr_b.ReadLine());
                external.Add(sr_b.ReadLine());

                sr_b.Close();
            }

            //Send chunks to functions here

            Toolbox tb = tIn;

            ParserChain tc = new ParserChain(chunks[2].getChunk(), tb);
            foreach (Chunk c in chunks)
            {
                ParserChain ts = new ParserChain(c.getChunk(), tb);
            }

            tc = new ParserChain(external, tb);
            string strTest = tb.ToString();
            System.Console.WriteLine(tb.checkSql());
            //System.Console.WriteLine(startupChunk[1]);
            //Process.Start(startupChunk[1]);

            //ParserChain ts = new ParserChain(chunks[0].getChunk(), new Toolbox());

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
        protected List<string>rChunk;

        //Chunk storage
        protected List<string> lChunk;
        

        /* =======================================================================================================================================================================================
         * cleanIn(List<string> strInput)
         *  Function:
         *   -Cleans the chunk data and prepares it for processing
         *   -Calls stringSwitch to decide whether this chain link performs the required actions for the settings
         * 
         * =======================================================================================================================================================================================
         */
        public void cleanIn(List<string> inChunk)
        {
            //preprocess the settings tag
            string strUnIn;
            strUnIn = inChunk[0];
            strUnIn = strUnIn.Trim();
            strIn = strUnIn.ToLower();

            //store chunk
            lChunk = inChunk;

            //call switch function
            stringSwitch();
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
                    strSplit = strChunk.Split(strRegEx, 2, StringSplitOptions.None);

                    if (!strSplit[0].Contains(".exe"))
                    {
                        strSplit[0] = strSplit[0] + ".exe";
                    }
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
                        Program.writeLog("Failed to open " + strChunk);
                    }

            }
        }

        /* =======================================================================================================================================================================================
         * SqlParserChain.chainEnd()
         *   - Run by the last parser in the chain to log that an unknown setting has not been processed by the script as it has not been implemented or incorrectly named
         * =======================================================================================================================================================================================
         */
        public virtual void chainEnd() {
            Program.writeLog(strIn + "Command not found, check configuration file formatting or check if specific command has been implemented.");
        }
    }

//********************************************************************************************************************************************************************************************
//settings tag parser chain begins
    /* =======================================================================================================================================================================================
     * ParserChain
     *  - Concrete implementation of Parser abstract class
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
    class ParserChain : Parser
    {
        /* =======================================================================================================================================================================================
         * ParserChain.ParserChain(List<string>, Toolbox)
         *   - The "default" constructor for ParserChain as Parsers always need a list of strings for commands and a toolbox for utilities
         *      - Stores input and sends it to preprocessing
         * =======================================================================================================================================================================================
         */
        public ParserChain(List<string> inChunk, Toolbox tIn)
        {
            //Toolbox to use if the parser needs utilites implemented within it
            tools = tIn;

            //System.Console.WriteLine("ParserChain entered");

            //set parser keyword/tag
            strParse = "[startup]";

            //start tag preprocessing
            cleanIn(inChunk);
        }

        /* =======================================================================================================================================================================================
         * ParserChain.spawnSub()
         *   - Run [startup] tag options
         *     - execute programs on startup (run execute())
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            //run execute function
            execute();
        }

        /* =======================================================================================================================================================================================
         * ParserChain.nextLink()
         *   - Pass chunk onto the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            System.Console.WriteLine("In [startup] chain link, going to next link");
            RebootLink rlLink = new RebootLink(lChunk, tools);
        }
    }

    /* =======================================================================================================================================================================================
     * RebootLink
     *  - Concrete implementation of Parser abstract class
     *  - RebootLink parses and handles the [reboot] in the config file
     *  - Is a Parser in the settings tag parser chain
     * =======================================================================================================================================================================================
     */
    class RebootLink : Parser
    {
        /* =======================================================================================================================================================================================
         * RebootLink.RebootLink(List<string>, Toolbox)
         *   - The "default" constructor for RebootLink as Parsers always need a list of strings for commands and a toolbox for utilities
         *      - Stores input and sends it to preprocessing
         * =======================================================================================================================================================================================
         */
        public RebootLink(List<string> inChunk, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("RebootLink entered");

            //set parser keyword/tag
            strParse = "[reboot]";
            cleanIn(inChunk);
        }

        /* =======================================================================================================================================================================================
         * RebootLink.spawnSub()
         *   - Run [reboot] tag options
         *     - execute programs on reboot (run execute())
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
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
         * RebootLink.nextLink()
         *   - Pass chunk onto the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            System.Console.WriteLine("In [reboot] chain link, going to next link");
            SqlLink sqlLink = new SqlLink(lChunk, tools);
            //throw new NotImplementedException();
        }
    }

    /* =======================================================================================================================================================================================
     * SqlLink
     *  - Concrete implementation of Parser abstract class
     *  - SqlLink parses and handles the [sql config] tag in the config file
     *  - Is a Parser in the settings tag parser chain
     * =======================================================================================================================================================================================
     */
    class SqlLink : Parser
    {
        /* =======================================================================================================================================================================================
         * SqlLink.SqlLink(List<string>, Toolbox)
         *   - The "default" constructor for SqlLink as Parsers always need a list of strings for commands and a toolbox for utilities
         *      - Stores input and sends it to preprocessing
         * =======================================================================================================================================================================================
         */
        public SqlLink(List<string> inChunk, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlLink entered");
            strParse = "[sql config]";
            cleanIn(inChunk);
        }

        /* =======================================================================================================================================================================================
         * SqlLink.spawnSub()
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
         * SqlLink.nextLink()
         *   - Pass chunk onto the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            System.Console.WriteLine("In [sql config] chain link");
            RebConfLink rcLink = new RebConfLink(lChunk, tools);
        }
    }

    /* =======================================================================================================================================================================================
     * RebConfLink
     *  - Concrete implementation of Parser abstract class
     *  - RebConfLink parses and handles the [reboot config] tag in the config file
     *  - Is a Parser in the settings tag parser chain
     * =======================================================================================================================================================================================
     */
    class RebConfLink : Parser
    {
        /* =======================================================================================================================================================================================
         * RebConfLink.RebConfLink(List<string>, Toolbox)
         *   - The "default" constructor for RebConfLink as Parsers always need a list of strings for commands and a toolbox for utilities
         *      - Stores input and sends it to preprocessing
         * =======================================================================================================================================================================================
         */
        public RebConfLink(List<string> inChunk, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("RebConfLink entered");

            //set parser keyword/tag
            strParse = "[reboot config]";
            cleanIn(inChunk);
        }

        /* =======================================================================================================================================================================================
         * RebConfLink.spawnSub()
         *   - Run [reboot] tag options
         *     - Process reboot configurations
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            //reboot configurations go here

        }

        /* =======================================================================================================================================================================================
         * RebConfLink.nextLink()
         *   - Pass chunk onto the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            System.Console.WriteLine("In [reboot config] chain link, going to next link");
            ConfRebTimeLink crtLink = new ConfRebTimeLink(lChunk, tools);
            //throw new NotImplementedException();
        }
    }

    /* =======================================================================================================================================================================================
     * ConfRebTimeLink
     *  - Concrete implementation of Parser abstract class
     *  - ConfRebTimeLink parses and handles the [configured reboot time] tag in the config file
     *  - Is a Parser in the settings tag parser chain
     * =======================================================================================================================================================================================
     */
    class ConfRebTimeLink : Parser
    {
        /* =======================================================================================================================================================================================
         * ConfRebTimeLink.ConfRebTimeLink(List<string>, Toolbox)
         *   - The "default" constructor for ConfRebTimeLink as Parsers always need a list of strings for commands and a toolbox for utilities
         *      - Stores input and sends it to preprocessing
         * =======================================================================================================================================================================================
         */
        public ConfRebTimeLink(List<string> inChunk, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("ConfRebTimeLink entered");

            //set parser keyword/tag
            strParse = "[reboot config]";
            cleanIn(inChunk);
        }

        /* =======================================================================================================================================================================================
         * ConfRebTimeLink.spawnSub()
         *   - Run [reboot] tag options
         *     - Process reboot configurations
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            //reboot configurations go here

        }

        /* =======================================================================================================================================================================================
         * ConfRebTimeLink.nextLink()
         *   - Pass chunk onto the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            System.Console.WriteLine("In reboot config chain link, going to next link");
            LastRebootLink lrLink = new LastRebootLink(lChunk, tools);
            //throw new NotImplementedException();
        }
    }

    /* =======================================================================================================================================================================================
     * LastRebootLink
     *  - Concrete implementation of Parser abstract class
     *  - LastRebootLink parses and handles the [last reboot time] tag in the config file
     *  - Is a Parser in the settings tag parser chain
     * =======================================================================================================================================================================================
     */
    class LastRebootLink : Parser
    {
        /* =======================================================================================================================================================================================
         * LastRebootLink.LastRebootLink(List<string>, Toolbox)
         *   - The "default" constructor for LastRebootLink as Parsers always need a list of strings for commands and a toolbox for utilities
         *      - Stores input and sends it to preprocessing
         * =======================================================================================================================================================================================
         */
        public LastRebootLink(List<string> inChunk, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("LastRebootLink entered");

            //set parser keyword/tag
            strParse = "[last reboot time]";
            cleanIn(inChunk);
        }

        /* =======================================================================================================================================================================================
         * LastRebootLink.spawnSub()
         *   - Run [reboot] tag options
         *     - Process last reboot time and store it
         * =======================================================================================================================================================================================
         */
        public override void spawnSub()
        {
            //last reboot time configurations go here
            foreach(string x in lChunk)
            {
                DateTime dt = Convert.ToDateTime(x);
                tools.setLastReboot(dt);
                System.Console.WriteLine(dt);
            }
            
            
        }

        /* =======================================================================================================================================================================================
         * LastRebootLink.nextLink()
         *   - Pass chunk onto the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void nextLink()
        {
            System.Console.WriteLine("In [last reboot time] config chain link, going to next link");
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
                StringParse();
            }
            else
            {
                nextLink();
            }
        }

        /* =======================================================================================================================================================================================
         * InfoParserChain.StringParse()
         *   - Splits the line
         * =======================================================================================================================================================================================
         */
        public void StringParse()
        {
            strRaw = strIn.Split('=');
            try
            {
                strIn = strRaw[1];
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Setting data missing");
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
        protected string strIn;
        protected string[] strRaw;

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
                StringParse();
            }else
            {
                nextLink();
            }
        }

        /* =======================================================================================================================================================================================
         * SqlParserChain.StringParse()
         *   - Splits the line
         * =======================================================================================================================================================================================
         */
        public void StringParse()
        {
            strRaw = strIn.Split('=');
            try
            {
                strIn = strRaw[1];
            }
            catch (Exception e)
            {
                System.Console.WriteLine("SQL data setting data missing.");
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
        int intRebDelay, intRebInterval, intSqlInterval;

        StreamReader sr;

        /*=======================================================================================================================================================================================
         * Toolbox.Toolbox()
         *      - Default constructor for Toolbox
         *          - Initiates the empty sqlInfo array
         * =======================================================================================================================================================================================
         */
        public Toolbox()
        {
            sqlInfo = new string[6];
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
         public int intervalMath(string strTime, string strInterval)
        {
            int intSeconds = 0;
            //temp
            return intSeconds;
        }

        /*=======================================================================================================================================================================================
         * Toolbox.setSqlInterval()
         *      -Set the interval for checking SQL
         * =======================================================================================================================================================================================
         */
        public void setSqlInterval(string strTime, string strInterval)
        {
            intSqlInterval = intervalMath(strTime, strInterval);
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
                for(int i = 0; i<6; i++)
                {
                    //dummy code to iterate through the array
                    sqlInfo[i].Contains("");
                }
                return true;
            }catch(Exception e)
            {
                return false;
            }
        }

        /* =======================================================================================================================================================================================
         * Toolbox.clearCsv()
         *   - Resets the CSV file
         * =======================================================================================================================================================================================
         */
        private void clearCsv()
        {
            if (File.Exists(Program.postUrl))
            {
                File.Delete(Program.postUrl);
            }
            StreamWriter sw = new StreamWriter(Program.postUrl);
            sw.WriteLine("StartupTime,ServerName,Subcategory,Status,Service,Error,");
            sw.Close();
        }

        /* =======================================================================================================================================================================================
         * Toolbox.clearCsv()
         *   - Vreifies the integrity of the CSV file
         * =======================================================================================================================================================================================
         */
        public bool checkCsv()
        {
            //The regex pattern is: ^"(.+)" ?,"(\w+)","(.+)","([A-Z]+)","(.+)","(.*)"$
            bool result = true;
            bool error = false;
            string subject;
            sr = new StreamReader(Program.postUrl, System.Text.Encoding.Default);
            subject = sr.ReadLine(); //skips first line
            string pattern = "^\"(.+)\" ?,\"(\\w+)\",\"(.+)\",\"([A-Z]+)\",\"(.+)\",\"(.*)\"$";
            Regex rgx = new Regex(@pattern);
            
            while (!error && subject != null)
            {
                subject = sr.ReadLine();
                if(subject != null)
                {
                    if (rgx.IsMatch(subject))
                    {
                        //do nothing
                    }
                    else
                        {
                            Program.writeLog("Error in the CSV file");
                            Program.exit(2);
                        }
                }                
            }
            return result;
        }

        //=================================================
        //Returns two letter weekday format as int.
        //Follows the same format the DateTime object uses.
        //=================================================
        public int twoLetterDay(string i)
        {
            if (i.CompareTo("su") == 1)
            {
                return 0;
            }
            else if(i.CompareTo("mo") == 1)
            {
                return 1;
            }
            else if (i.CompareTo("tu") == 1)
            {
                return 2;
            }
            else if (i.CompareTo("we") == 1)
            {
                return 3;
            }
            else if (i.CompareTo("th") == 1)
            {
                return 4;
            }
            else if (i.CompareTo("fr") == 1)
            {
                return 5;
            }
            else if (i.CompareTo("sa") == 1)
            {
                return 6;
            }
            else //In case of invalid input
                {
                    return -1;
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
    }
}