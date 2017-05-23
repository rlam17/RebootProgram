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
        static void writeLog()
        {
            string logUrl = "./log/Log.txt";
            if (!File.Exists(logUrl))
            {
                System.Console.WriteLine("Log doesn't exists, creating...");
                File.Create(logUrl);
            }


        }
        static void readConf()
        {
            System.Console.WriteLine("Hello world!");
        }

        static void Main(string[] args)
        {
            writeLog();
        }
    }
}
