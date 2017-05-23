using System;
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
        static void writeLog(String logMessage)
        {
            //This will create a log if it doesn't exist
            StreamWriter sw = new StreamWriter(logUrl, true);

            sw.WriteLine(logMessage);
            sw.Close();


        }
        static void readConf()
        {
            System.Console.WriteLine("Hello world!");
        }

        static void Main(string[] args)
        {
            writeLog("This is a test");
        }
    }
    //Template method for parsing
    abstract class TagParse
    {
        //variable which stores the parse word
        string strParse;
        //Set parse 
        //Spawn specific subparser
        public abstract void SpawnSub();
        //abstract base for spawning logs
        // -add error code ints(?) if needed in the future to spawn specific messages
        public abstract void SpawnLog();
        public void StringParse(string strIn)
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


}
