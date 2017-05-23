﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Websdepot
{
    class Program
    {
        static string logUrl = "./log/Log.txt";
        static string confUrl = "./Conf.cfg";
        static string todayDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        static void writeLog(String logMessage)
        {
            //This will create a log if it doesn't exist
            StreamWriter sw = new StreamWriter(logUrl, true);

            logMessage = todayDate + "| " + logMessage;

            sw.WriteLine(logMessage);
            sw.Close();

        }
        static void readConf()
        {

            //Check goes here

            if (!false) //Conf does not clear check
            {

                writeLog("There is something wrong with the log file");
                exit(1);
            }
        }

        static void Main(string[] args)
        {
            writeLog("Starting program");
            readConf();

        }
        static void exit(int code)
        {
            writeLog("Exitting program");
            System.Environment.Exit(code);
        }

        static void readChunks()
        {

            List<String> sqlChunk = new List<String>();
            List<String> rebootConfigChunk = new List<String>();
            List<String> startupChunk = new List<String>();
            List<String> rebootChunk = new List<String>();

            StreamReader sr = new StreamReader(confUrl);

            String line;
            List<String> currentList = sqlChunk;
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

            //Send chunks to functions here

        }
    }
    /*Template method for 
       - name scheme for all the concrete parsers will follow the Tag[tag-name] naming convention
    */
    abstract class TagParse
    {
        //variable which stores the parse word
        string strParse;
        //Sanitized Tag Input
        string strIn;
        //Chunk storage
        List<string> lChunk;
        /*
         * Input Sanitation
         *  - Check for input inconsistencies(extra whitespaces)
         */

        //strUnIn == strUnprocessedInput
        public void CleanIn(string strUnIn) {
            strIn = strUnIn.Trim();
        }
        //Spawn specific subparser
        public abstract void SpawnSub();
        //abstract base for spawning logs
        // -add error code int array(?) if needed in the future to spawn specific messages
        public abstract void SpawnLog();
        public void StringParse()
        {
            if (strIn.Equals(strParse))
            {
                //spawn specific subprocess parser
                SpawnSub();
            }else
            {
                //spawn error/log message
                SpawnLog();
            }
        }
    }

    class TagStartup : TagParse
    {
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
