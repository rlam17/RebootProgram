using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        private static void writeLog(string logMessage)
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
                string hashedConf = createHash();
            }
        }
        private static string createHash()
        {
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


            //find a way to get SQL connection credentials
            SqlConnection myConnection = new SqlConnection("user id=username;" +
                                       "password=password;server=serverurl;" +
                                       "Trusted_Connection=yes;" +
                                       "database=database; " +
                                       "connection timeout=30");

            try
            {
                myConnection.Open();
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

        static void Main(string[] args)
        {
            writeLog("Starting program");
            readConf();

            connectSql();
            delayWait(5);

        }
        private static void exit(int code)
        {
            writeLog("Exitting program");
            System.Environment.Exit(code);
        }

        private void readChunks()
        {

            List<string> sqlChunk = new List<string>();
            List<string> rebootConfigChunk = new List<string>();
            List<string> startupChunk = new List<string>();
            List<string> rebootChunk = new List<string>();

            StreamReader sr = new StreamReader(confUrl);

            string line;
            List<string> currentList = sqlChunk;
            int currentChunk = 0;
            while((line = sr.ReadLine()) != null) //until EOF
            {
                switch (currentChunk) //Check which chunk it is on
                {
                    case 0:
                        currentList = sqlChunk;
                        break;
                    case 1:
                        currentList = rebootConfigChunk;
                        break;
                    case 2:
                        currentList = startupChunk;
                        break;
                    case 3:
                        currentList = rebootChunk;
                        break;
                    default:
                        currentList = null;
                        break;
                }

                if (currentChunk < 4) 
                    //If still working on a chunk, added to the list
                    //currentChunk == 4 means all chunks are done
                {
                    if (line != "")
                    {
                        currentList.Add(line);
                    }
                    else
                    {
                        currentChunk++;
                    }
                }
                
            }

            sr.Close();


            //Send chunks to functions here

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
        List<string> lChunk;
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
        }
        //Spawn specific subparser
        public abstract void SpawnSub();
        //abstract base for spawning logs
        // -add error code int array(?) if needed in the future to spawn specific messages
        public abstract void SpawnLog();
        public void stringParse()
        {
            if (strIn.Equals(strParse))
            {
                //spawn specific subprocess parser
                SpawnSub();
            }
            else
            {
                //Call next in chain
                //Chain tail should ouput a log
                SpawnLog();
            }
        }
    }

    class TagStartup : TagParse
    {
        //try to avoid (privated for safety)
        private TagStartup()
        {
            strParse = "[startup]";
        }
        public TagStartup(List<string> inChunk)
        {
            strParse = "[startup]";
            CleanIn(inChunk);
        }
        public override void SpawnSub()
        {

            throw new NotImplementedException();
        }
        public override void SpawnLog()
        {
            throw new NotImplementedException();
        }
    }

}