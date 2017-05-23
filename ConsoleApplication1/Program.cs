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

            if (false) //Conf does not clear check
            {
                writeLog("There is something wrong with the log file");
                System.Environment.Exit(1);
            }
        }

        static void Main(string[] args)
        {
            writeLog("Starting program");
            readConf();

        }
    }
}
