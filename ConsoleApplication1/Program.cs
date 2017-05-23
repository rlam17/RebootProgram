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

            sw.WriteLine(logMessage);
            sw.Close();


        }
        static void readConf()
        {
            System.Console.WriteLine("Hello world!");
        }

        static void Main(string[] args)
        {
            System.Console.WriteLine(todayDate);
        }
    }
}
