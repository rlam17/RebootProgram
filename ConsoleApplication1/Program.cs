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
            readChunks();

            //checkPost();
        }
        private static void exit(int code)
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

            //ParserChain ts = new ParserChain(startupChunk);
            /*
            foreach (string x in rebootChunk){
                System.Console.WriteLine(x);
            }
            */
            //System.Console.WriteLine(startupChunk[1]);
            //Process.Start(startupChunk[1]);
            for(int i = 0; i < chunks.Count; i++)
            {
                ParserChain ts = new ParserChain(chunks[i].getChunk(), new Toolbox());
            }
            
        }
    }
    /*Template method for 
       - name scheme for all the concrete parsers will follow the Tag[tag-name] naming convention
    
      Method operation chain:
      1) Create parser object
            - Object initiates tag keyword
            - Passes tag chunk to input sanitation function
      2) Input sanitation 
            - Sanitizes and stores the tag
            - Removes the tag from the chunk and stores it
      3) Process the chunk
            - Follow specific rules on a chunk specific basis

        ***NOTE TO PEOPLE ADDING LINKS***
        * New end links to the chain 
    */
    abstract class Parser
    {
        //toolbox variable
        protected Toolbox tools;
        //variable which stores the parse word
        protected string strParse;
        //Sanitized Tag Input
        protected string strIn;
        //Chunk storage
        protected List<string> lChunk;
        /*
         * Input Sanitation
         *  - Check for input inconsistencies(extra whitespaces)
         */

        //rChunk == rawChunk
        public void CleanIn(List<string> inChunk)
        {
            string strUnIn;
            //clean and store the tag in the chunk
            strUnIn = inChunk[0];
            strUnIn = strUnIn.Trim();
            strIn = strUnIn.ToLower();
            //store the chunk to the object and remove the tag
            lChunk = inChunk;
            lChunk.RemoveAt(0);
            stringParse();
        }
        //Spawn specific subparser
        public abstract void SpawnSub();
        //abstract base for spawning logs
        // -add error code int array(?) if needed in the future to spawn specific messages
        public abstract void NextLink();
        public virtual void stringParse()
        {
            if (strIn.Equals(strParse))
            {
                System.Console.WriteLine(strIn + " this is chunk 1");
                //spawn specific subprocess parser
                SpawnSub();
            }
            else
            {
                //Call next in chain
                //Chain tail should ouput a log
                System.Console.WriteLine(strIn + " this is chunk 2");
                NextLink();
            }
        }

        //Execute parsed functions
        public virtual void Execute()
        {
            foreach (string strChunk in lChunk)
            {
                /*
                //read takes in string literals, no need for extra processing
                //chunks are super volitile right now needs further testing
                strProcessed = strChunk.Replace("\\", "\\\\");
                Process.Start(strProcessed);
                */
                try
                {
                    string strPath, strArgs;
                    string[] strRegEx = new string[] { ".exe " };
                    string[] strSplit;
                    strArgs = " ";
                    strSplit = strChunk.Split(strRegEx, 2, StringSplitOptions.None);
                    //strSplit[0] = strSplit[0] + ".exe";
                    strPath = strSplit[0];
                    
                    try
                    {
                        strArgs = strSplit[1];
                    }
                    catch (Exception e) { }


                    System.Console.WriteLine(strPath + " | " + strArgs);
                    var process = Process.Start(strPath, strArgs);
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
            Program.writeLog("Command not found, check configuration file formatting or check if specific command has been implemented.");
        }
    }
    
    //concrete tag parser chain
    class ParserChain : Parser
    {
        //try to avoid (privated for safety)
        private ParserChain()
        {
            strParse = "[startup]";
        }

        public ParserChain(List<string> inChunk, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;
            //System.Console.WriteLine("ParserChain entered");
            strParse = "[startup]";
            CleanIn(inChunk);
        }
        
        //Startup tag will run processes based off of the parsed string paths 
        public override void SpawnSub()
        {
            //string strProcessed;
            Execute();
            //throw new NotImplementedException();
        }

        public override void NextLink()
        {
            RebootLink rlParse = new RebootLink(lChunk, tools);
        }
    }

    class RebootLink : Parser
    {
        //try to avoid (privated for safety)
        private RebootLink()
        {
            strParse = "[reboot]";
        }
        public RebootLink(List<string> inChunk, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("RebootLink entered");
            strParse = "[reboot]";
            CleanIn(inChunk);
        }
        
        //Startup tag will run processes based off of the parsed string paths 
        public override void SpawnSub()
        {
            Execute();
           
            //throw new NotImplementedException();
        }
        public override void NextLink()
        {
            System.Console.WriteLine("In reboot chain link");
            throw new NotImplementedException();
        }
    }

    //SQL Link
    class SqlLink : Parser
    {
        //try to avoid (privated for safety)
        private SqlLink()
        {
            strParse = "[SQL Config]";
        }
        public SqlLink(List<string> inChunk, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlLink entered");
            strParse = "[SQL Config]";
            CleanIn(inChunk);
        }

        //Startup tag will run processes based off of the parsed string paths 
        public override void SpawnSub()
        {
            Execute();

            //throw new NotImplementedException();
        }
        public override void NextLink()
        {
            System.Console.WriteLine("In reboot chain link");
            throw new NotImplementedException();
        }
    }
    //end of tag parser family

    //beginning of sql parser chain family
    class SqlParseChain : Parser
    {
        private SqlParseChain()
        {
            strParse = "";
        }
        public SqlParseChain(string strLine, Toolbox tIn)
        {
            //toolbox in
            tools = tIn;

            //System.Console.WriteLine("SqlLink entered");
            strParse = "Host=";
            strIn = strLine.Trim();
            //overriden string parse
            stringParse();
        }
        public override void stringParse()
        {
            //Check if the line passed contains the token this link is responsible for...
            if (strIn.Contains(strParse))
            {
                //if yes, process:
                
            }else
            {
                //go to next in chain
            }
        }

        //Startup tag will run processes based off of the parsed string paths 
        public override void SpawnSub()
        {
            Execute();

            //throw new NotImplementedException();
        }
        public override void NextLink()
        {
            System.Console.WriteLine("In reboot chain link");
            throw new NotImplementedException();
        }
    }
    //end of parser family
    class Toolbox
    {
        string[] sqlInfo;
        StreamReader sr;
        public Toolbox()
        {
            sqlInfo = new string[5];
        }

        //set the SQL property array (index, string)
        public void SetSql(int intIndex, string strIn)
        {
           /*
            * index for SQL info array:
            * 
            * 0: Host IP (eg: 192.168.0.10)
            * 1: Port number (eg: 1433)
            * 2: Username (eg: SomeUser)
            * 3: Password (eg: SomePassword)
            * 4: Database (eg: SomeDb)
            * 5: CheckinTime (by minutes?) (eg: 2)
            */
            sqlInfo[intIndex] = strIn;
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
            bool result = true;
            bool error = false;
            sr = new StreamReader(Program.postUrl);
            sr.ReadLine();
            while (!error)
            {
                string subject = sr.ReadLine();
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