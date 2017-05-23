using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
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
}
