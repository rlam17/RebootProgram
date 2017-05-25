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
        static public string todayDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        static public string postUrl = "./post/post.csv";
        static List<confChunk> chunks = new List<confChunk>();
        public static void writeLog(string logMessage)
        {
            //This will create a log if it doesn't exist
            StreamWriter sw = new StreamWriter(logUrl, true);

            logMessage = todayDate + "| " + logMessage;

            sw.WriteLine(logMessage);
            sw.Close();

        }
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
            } catch(Exception e)
            {
                writeLog("Connection to SQL failed " + e);
            }

        }
        private static void delayWait(int min) //Input will be in minutes
        {
            int minToSec = 1000 * 60 * min;
            System.Threading.Thread.Sleep(minToSec);
        }

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
            writeLog("Starting program");
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

            readChunks();

            //checkPost();
        }
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

        private static void readChunks()
        {

            List<string> sqlChunk = new List<string>();
            List<string> rebootConfigChunk = new List<string>();
            List<string> startupChunk = new List<string>();
            List<string> rebootChunk = new List<string>();
            List<string> lastRebootChunk = new List<string>();
            List<string> configuredRebootChunk = new List<string>();

            StreamReader sr = new StreamReader(confUrl);

            string line;
            List<string> currentList = new List<string>();
            int currentChunk = 0;
            while((line = sr.ReadLine()) != null) //until EOF
            {
                if (currentChunk < 6) 
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
                                chunks.Add(new confChunk(currentList));
                                break;
                            case 1:
                                //rebootConfigChunk = currentList;
                                chunks.Add(new confChunk(currentList));
                                break;
                            case 2:
                                //startupChunk = currentList;
                                chunks.Add(new confChunk(currentList));
                                break;
                            case 3:
                                //rebootChunk = currentList;
                                chunks.Add(new confChunk(currentList));
                                break;
                            case 4:
                                //lastRebootChunk = currentList;
                                chunks.Add(new confChunk(currentList));
                                break;
                            case 5:
                                //configuredRebootChunk = currentList;
                                chunks.Add(new confChunk(currentList));
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


            //Send chunks to functions here

            Toolbox tb = new Toolbox();
            ParserChain ts = new ParserChain(chunks[0].getChunk(), tb);
            
            foreach (string x in rebootChunk){
                System.Console.WriteLine(x);
            }

            string strTest = tb.ToString();
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
     *        - Last chain in parser chain always calls ChainEnd()
     *      
     * =======================================================================================================================================================================================
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
         * CleanIn(List<string> strInput)
         *  Function:
         *   -Cleans the chunk data and prepares it for processing
         *   -Calls stringSwitch to decide whether this chain link performs the required actions for the settings
         * 
         * =======================================================================================================================================================================================
         */
        public void CleanIn(List<string> inChunk)
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
        public abstract void SpawnSub();

        //abstract base class for going to next link in chain if this link isn't responsible for the settings tag
        public abstract void NextLink();

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
                SpawnSub();
            }
            else
            {
                //Call next in chain
                //Chain tail should ouput a log
                System.Console.WriteLine(strIn + " link does not handle this option, going to next");
                NextLink();
            }
        }

        /* =======================================================================================================================================================================================
         * TagParse.Execute()
         *  - executes/launches programs based on input stored in the chunk
         * =======================================================================================================================================================================================
         */
        public virtual void Execute()
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
                    //strSplit[0] = strSplit[0] + ".exe";
                    strPath = strSplit[0];
                    
                    //Grabs the command line arguments
                    // - try/catch block used because command line arguments might not be there and the strSplit[1] index might not exist
                    // - it is normal for the code to continue even after the exception is caught (cmd args not found)
                    try
                    {
                        strArgs = strSplit[1];
                    }
                    catch (Exception e) { }


                    System.Console.WriteLine(strPath + " | " + strArgs);
                    
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

        //Run when end of chain is reached
        public virtual void ChainEnd() {
            Program.writeLog(strIn + "Command not found, check configuration file formatting or check if specific command has been implemented.");
        }
    }

    /* =======================================================================================================================================================================================
     * ParserChain
     *  - Concrete implementation of Parser abstract class
     *  - ParserChain parses and handles the settings tags in the config file
     *  - ParserChain is the first in a chain of tag parsers which allows the script to handle settings tags regardless of order
     *  
     *  - Parser chain workflow:
     *     - Upon creation preprocess settings tag
     *     - Check if tag is handled by this particular parser
     *          -If yes:
     *              - Execute code based on settings
     *          -If no:
     *              - Pass the settings chunk onto the next parser in the chain
     * =======================================================================================================================================================================================
     */
    class ParserChain : Parser
    {
        //Default constructor should never be used by user
        private ParserChain()
        {
            strParse = "[startup]";
        }

        /* =======================================================================================================================================================================================
         * ParserChain.ParserChain()
         *   - The "default" constructor for ParserChain as Parsers always need a list of strings for commands and a toolbox for utilities
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
            CleanIn(inChunk);
        }

        /* =======================================================================================================================================================================================
         * ParserChain.SpawnSub()
         *   - Run [startup] tag options
         *     - Execute programs on startup (run Execute())
         * =======================================================================================================================================================================================
         */
        public override void SpawnSub()
        {
            //run execute function
            Execute();
        }

        /* =======================================================================================================================================================================================
         * ParserChain.NextLink()
         *   - Pass chunk onto the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void NextLink()
        {
            System.Console.WriteLine("In startup chain link, going to next link");
            RebootLink rlParse = new RebootLink(lChunk, tools);
        }
    }

    /* =======================================================================================================================================================================================
     * RebootLink
     *  - Concrete implementation of Parser abstract class
     *  - RebootLink parses and handles the settings tags in the config file
     *  - Is a Parser in the settings tag parser chain
     * =======================================================================================================================================================================================
     */
    class RebootLink : Parser
    {
        //privated as the default constructor for Parsers should never be used
        private RebootLink()
        {
            strParse = "[reboot]";
        }

        /* =======================================================================================================================================================================================
         * RebootLink.RebootLink()
         *   - The "default" constructor for RebootLink as Parsers always need a list of strings for commands and a toolbox for utilities
         * =======================================================================================================================================================================================
         */
        public RebootLink(List<string> inChunk, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("RebootLink entered");

            //set parser keyword/tag
            strParse = "[reboot]";
            CleanIn(inChunk);
        }

        /* =======================================================================================================================================================================================
         * RebootLink.SpawnSub()
         *   - Run [reboot] tag options
         *     - Execute programs on reboot (run Execute())
         * =======================================================================================================================================================================================
         */
        public override void SpawnSub()
        {
            Execute();
        }

        /* =======================================================================================================================================================================================
         * RebootLink.NextLink()
         *   - Pass chunk onto the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void NextLink()
        {
            System.Console.WriteLine("In reboot chain link, going to next link");
            SqlLink sqlParse = new SqlLink(lChunk, tools);
            //throw new NotImplementedException();
        }
    }

    /* =======================================================================================================================================================================================
     * SqlLink
     *  - Concrete implementation of Parser abstract class
     *  - SqlLink parses and handles the settings tags in the config file
     *  - Is a Parser in the settings tag parser chain
     * =======================================================================================================================================================================================
     */
    class SqlLink : Parser
    {
        //try to avoid (privated for safety)
        private SqlLink()
        {
            strParse = "[sql config]";
        }

        /* =======================================================================================================================================================================================
         * SqlLink.SqlLink()
         *   - The "default" constructor for SqlLink as Parsers always need a list of strings for commands and a toolbox for utilities
         * =======================================================================================================================================================================================
         */
        public SqlLink(List<string> inChunk, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlLink entered");
            strParse = "[sql config]";
            CleanIn(inChunk);
        }

        /* =======================================================================================================================================================================================
         * SqlLink.SpawnSub()
         *   - Run [sql config] tag options
         *     - Grabs all sql connection settings
         *          - Achieve this by passing each settings line into a parser chain which parses the information and stores it into the passed in toolbox
         * =======================================================================================================================================================================================
         */
        public override void SpawnSub()
        {
            //go through each individual chunk line and pass it into the sql line parcer chain
            foreach (string strIn in lChunk)
            {
                SqlParseChain sqlParse = new SqlParseChain(strIn, tools);
            }
        }

        /* =======================================================================================================================================================================================
         * SqlLink.NextLink()
         *   - Pass chunk onto the next parser in the chain
         * =======================================================================================================================================================================================
         */
        public override void NextLink()
        {
            System.Console.WriteLine("In sql chain link");
            throw new NotImplementedException();
        }
    }
    //end of tag parser family

    //beginning of sql parser chain family
    class SqlParseChain : Parser
    {
        protected string strSqlIn;
        protected string[] strRaw;
        protected SqlParseChain()
        {
            strParse = "";
        }
        public SqlParseChain(string strLine, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlLink entered");
            strParse = "Host=";
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
                StringParse();
            }else
            {
                NextLink();
            }
        }

        public void StringParse()
        {
            strRaw = strIn.Split('=');
            try
            {
                strSqlIn = strRaw[1];
            }
            catch (Exception e)
            {
                System.Console.WriteLine("SQL data setting data missing.");
            }

            SpawnSub();
        }

        //Startup tag will run processes based off of the parsed string paths 
        public override void SpawnSub()
        {
            tools.SetSql(0, strSqlIn);
        }
        public override void NextLink()
        {
            SqlPortLink sqlPort = new SqlPortLink(strIn, tools);
        }
    }

    //port line parser
    class SqlPortLink : SqlParseChain
    {

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
        //Startup tag will run processes based off of the parsed string paths 
        public override void SpawnSub()
        {
            tools.SetSql(1, strSqlIn);
        }
        public override void NextLink()
        {
            SqlUnameLink sqlPort = new SqlUnameLink(strIn, tools);
        }
    }

    //Username line parser
    class SqlUnameLink : SqlParseChain
    {

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
        //Startup tag will run processes based off of the parsed string paths 
        public override void SpawnSub()
        {
            tools.SetSql(2, strSqlIn);
        }
        public override void NextLink()
        {
            SqlPwordLink sqlPort = new SqlPwordLink(strIn, tools);
        }
    }

    //Password line parser
    class SqlPwordLink : SqlParseChain
    {

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
        //Startup tag will run processes based off of the parsed string paths 
        public override void SpawnSub()
        {
            tools.SetSql(3, strSqlIn);
        }
        public override void NextLink()
        {
            SqlDbLink sqlPort = new SqlDbLink(strIn, tools);
        }
    }

    //Database line parser
    class SqlDbLink : SqlParseChain
    {

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
        //Startup tag will run processes based off of the parsed string paths 
        public override void SpawnSub()
        {
            tools.SetSql(4, strSqlIn);
        }
        public override void NextLink()
        {
            SqlCInLink sqlPort = new SqlCInLink(strIn, tools);
        }
    }

    //CheckIn line parser
    class SqlCInLink : SqlParseChain
    {

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
        //Startup tag will run processes based off of the parsed string paths 
        public override void SpawnSub()
        {
            tools.SetSql(5, strSqlIn);
        }
        public override void NextLink()
        {
            ChainEnd();
        }
    }

    //end of parser family

    //Toolbox holds information which main will used in a convenient bundle for methods to pass around
    class Toolbox
    {
        string[] sqlInfo;
        StreamReader sr;
        public Toolbox()
        {
            sqlInfo = new string[6];
        }

        //set the SQL property array (index, string)
        public void SetSql(int intIndex, string strIn)
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
             * 5: CheckinTime (by minutes?) (eg: 2)
             * =======================================================================================================================================================================================
             */
            sqlInfo[intIndex] = strIn;
        }
        
        public string[] GetSql()
        {
            return sqlInfo;
        }

        public bool CheckSql()
        {
            //parser orders info, so if it exists it is in the right spot
            //attempt to loop through the entire sqlInfo array, if successful, return true
            try
            {
                for(int i = 0; i<5; i++)
                {
                    sqlInfo[i].Contains("");
                }
                return true;
            }catch(Exception e)
            {
                return false;
            }
        }
        private void clearCsv()
        {
            if (File.Exists(Program.postUrl))
            {
                File.Delete(Program.postUrl);
            }
            StreamWriter sw = new StreamWriter(Program.postUrl);
            sw.WriteLine("StartupTime,ServerName,Status,Service,Error,");
            sw.Close();
        }
        public bool checkCsv()
        {
            //The regex pattern is: ^"(.+)" ?,"(\w+)","([A-Z]+)","(.+)","(.*)"$
            bool result = true;
            bool error = false;
            string subject;
            sr = new StreamReader(Program.postUrl);
            subject = sr.ReadLine(); //skips first line
            string pattern = "^\"(.+)\" ?,\"(\\w+)\",\"([A-Z]+)\",\"(.+)\",\"(.*)\"$";
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
    }

    class confChunk
    {
        List<string> lines;
        public confChunk(List<string> i)
        {
            lines = i;
        }

        public List<string> getChunk()
        {
            return lines;
        }
    }
}