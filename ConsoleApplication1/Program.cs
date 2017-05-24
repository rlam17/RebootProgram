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
        static string logUrl = "./log/Log.txt";
        static string confUrl = "./Conf.cfg";
        static string todayDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        static string postUrl = "./post/post.csv";
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
            // TODO: Actually pass into the Parse classes
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
                string hashedConf = createHash();
            }
        }
        private static string createHash()
        {

            // TODO: Ask how to store the MD5

            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(confUrl))
                {
                    return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
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

            //Process.Start(strChunk);
            string confHash = createHash();
            readChunks();

            checkPost();
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
                                sqlChunk = currentList;
                                break;
                            case 1:
                                rebootConfigChunk = currentList;
                                break;
                            case 2:
                                startupChunk = currentList;
                                break;
                            case 3:
                                rebootChunk = currentList;
                                break;
                            case 4:
                                lastRebootChunk = currentList;
                                break;
                            case 5:
                                configuredRebootChunk = currentList;
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

            ParserChain ts = new ParserChain(startupChunk);
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
    */
    abstract class TagParse
    {
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
        public void stringParse()
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
    }

    class ParserChain : TagParse
    {
        //try to avoid (privated for safety)
        private ParserChain()
        {
            strParse = "[startup]";
        }
        public ParserChain(List<string> inChunk)
        {
            //System.Console.WriteLine("ParserChain entered");
            strParse = "[startup]";
            CleanIn(inChunk);
        }
        
        //Startup tag will run processes based off of the parsed string paths 
        public override void SpawnSub()
        {
            //string strProcessed;
            foreach (string strChunk in lChunk){
                /*
                //read takes in string literals, no need for extra processing
                //chunks are super volitile right now needs further testing
                strProcessed = strChunk.Replace("\\", "\\\\");
                Process.Start(strProcessed);
                */
                try {
                    Process.Start(strChunk);
                }catch(Win32Exception e)
                {
                    Program.writeLog("Failed to open " + strChunk);
                }
                
            }
            //throw new NotImplementedException();
        }
        public override void NextLink()
        {
            RebootLink rlParse = new RebootLink(lChunk);
        }
    }

    class RebootLink : TagParse
    {
        //try to avoid (privated for safety)
        private RebootLink()
        {
            strParse = "[startup]";
        }
        public RebootLink(List<string> inChunk)
        {
            //System.Console.WriteLine("RebootLink entered");
            strParse = "[startup]";
            CleanIn(inChunk);
        }
        
        //Startup tag will run processes based off of the parsed string paths 
        public override void SpawnSub()
        {
            //string strProcessed;
            foreach (string strChunk in lChunk){
                try {
                    Process.Start(strChunk);
                }catch (FileNotFoundException e)
                {
                    Program.writeLog("Failed to open " + strChunk);
                }
                
            }
            //throw new NotImplementedException();
        }
        public override void NextLink()
        {
            System.Console.WriteLine("In reboot chain link");
            throw new NotImplementedException();
        }
    }
}