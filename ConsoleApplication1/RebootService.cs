using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Websdepot
{
    public class RebootService
    {

        private static System.Threading.Timer sqlTimer;
        private static System.Threading.Timer rebootTimer;



        static List<Chunk> chunks = new List<Chunk>();
        public void Start()
        {
            DoThings();
        }

        private static void DoThings()
        {
            //Create directories if they do not exist
            Directory.CreateDirectory("./log");
            Directory.CreateDirectory("./post/posted");
            Directory.CreateDirectory("./post/topost");



            //Provide feedback
            //  - Start of script
            Program.writeLog("Starting program");



            //Toolbox stores working information for the program as well as utility methods used by the main
            Toolbox magicBox = new Toolbox();

            //ConfStore object stores the configuration file
            ConfStore cStore = new ConfStore();


            //Read the file and store the data
            readConf(cStore, magicBox);

            //Generate the MD5 and store the MD5
            createHashFile(createHash());

            magicBox.setHash();

            //Create the inital SQL connection
            //  -By now the toolbox and configuration settings store is populated with data
            magicBox.connectSql();

            //update the last checkin time
            magicBox.updateLastCheckin();

            //Set up TimeSpan variables to store intervals the program will use for checking in
            TimeSpan sqlInterval = TimeSpan.FromMilliseconds(magicBox.getSqlInterval());
            TimeSpan rebootInterval = TimeSpan.FromMilliseconds(magicBox.getRebootDelay());

            delayWait(rebootInterval);

            //Clear the CSV file to start with a clean slate
            magicBox.clearCsv();

            //Execute programs to run at startup
            magicBox.runStart();

            //Verify integrity of the CSV files
            magicBox.checkCsv();

            //Upload the CSVs
            magicBox.uploadCsv();

            /* Every x minutes based on config file check in to the SQL server */
            //Spawn a thread for SQL check in timer
            sqlTimer = new System.Threading.Timer((e) =>
            {
                //magicBox.updateLastCheckin();                
                Console.WriteLine("SQL tick on " + DateTime.Now);

                // Check if a connection exists
                if (magicBox.checkConnection())
                {
                    //Update the last check in time
                    magicBox.updateLastCheckin();
                    createHashFile(createHash());
                    bool blnCheck = true;

                    try
                    {
                        //Check if Configured Reboot Time is the same between the SQL and the configuration file
                        blnCheck = magicBox.checkRt(cStore);

                        //if it isn't then rewrite the configuration file so it's Configured Reboot Time matches the SQL
                        if (!blnCheck)
                        {
                            //look for [configured reboot times] tag index
                            int intLength = cStore.getLength();
                            for (int i = 0; i < intLength; i++)
                            {
                                //if(cStore.getTag().Equals("[configured reboot times]"))
                                if (String.Compare(cStore.getTag()[i], "[configured reboot times]") == 0)
                                {

                                    Chunk cTemp = cStore.getTemp();
                                    cStore.setChunk(cTemp, i);
                                    cStore.writeConf();
                                    Program.writeLog("Updated Configured Reboot Time");
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }




                    //Verify the integrity of the MD5 and update it
                    if (!magicBox.compareHashWithSql())
                    {
                        //TODO: Change this so it just reads based off of chunks
                        //readConf(cStore, magicBox);
                        magicBox.updateConfInSql(cStore);
                    }

                    //Check for CSVs that haven't been uploaded and upload them
                    magicBox.checkPostQueue();
                }
                else
                {
                    Program.writeLog("Connection to SQL lost");
                }
            }, null, TimeSpan.Zero, sqlInterval);


            //Spawn a thread for checking if it's time to reboot
             rebootTimer = new System.Threading.Timer((f) =>
            {
                Program.writeLog("Reboot tick on " + DateTime.Now);

                DateTime happening = magicBox.getConfiguredRebootTime();
                TimeSpan interval = TimeSpan.FromMilliseconds(magicBox.getRebootInterval());

                while (magicBox.getLastRebootTime() >= happening)
                {
                    happening += interval;
                }


                if (DateTime.Now >= happening)
                {
                    if (magicBox.checkRebootTime())
                    {
                        Program.writeLog("Last reboot time is " + magicBox.getLastRebootTime());
                        Program.writeLog("Reboot interval is " + magicBox.getRebootInterval());
                        magicBox.runReb();
                    }
                }

            }, null, TimeSpan.Zero, rebootInterval);

            //This while loop exists to keep the timer threads from dying prematurely due to the end of the main program
            //while (true) { }
        }

        //====================================================
        // This function causes the program to wait min minutes
        //====================================================
        private static void delayWait(TimeSpan interval)
        {
            System.Threading.Thread.Sleep(interval);
        }

        //=======================================================
        // This function creates an MD5 hash of the Conf.cfg file.
        // Then passes it into createHashFile.
        //=======================================================
        private static string createHash()
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(Program.confUrl))
                {
                    string fileHash = Encoding.Default.GetString(md5.ComputeHash(stream));
                    createHashFile(fileHash);
                    return fileHash;
                }
            }
        }

        //================================================
        // Creates hash file of the Conf.cfg file.
        //  - Creates an MD5 file
        //================================================
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

        //=========================================
        //Read the Conf.cfg file
        //Chunks refers to each field in the file.
        //=========================================
        private static void readConf(ConfStore i_cs, Toolbox tIn)
        {
            //Check if file exists
            if (!File.Exists(Program.confUrl))
            {
                Program.writeLog("Conf file is missing");
                Program.exit(1);
            }

            StreamReader sr = new StreamReader(Program.confUrl, System.Text.Encoding.Default);

            string line = "";
            List<string> currentList = new List<string>();
            int currentChunk = 0;

            //Read until end of file
            while ((line = sr.ReadLine()) != null)
            {
                //Ensure the line is not empty before adding the line int the list that will form part of the settings chunk
                if (line != "")
                {
                    //Add the line to the chunk
                    currentList.Add(line);


                    //System.Console.WriteLine(line);
                }
                else
                {
                    //Make sure that the currentList is not empty
                    if (currentList.Count != 0)
                    {
                        //Add the chunk to the settings chunk
                        chunks.Add(new Chunk(currentList));
                        currentList = new List<string>();
                        currentChunk++;
                    }
                }
            }

            //Close the reader
            sr.Close();

            //Grab the final chunk that the loop did not grab
            if (currentList.Count != 0)
            {
                chunks.Add(new Chunk(currentList));
            }

            //External is a list of strings which will store the [Last Reboot Time] settings tag in a chunk
            List<string> external = new List<string>();

            if (!File.Exists("./lastreboottime.txt"))
            {
                //The [Last Reboot Time] tag exists in a seperate file 
                // - Create that file if it does not exist
                //   - Could be merged into configuration file but that adds more complexity
                StreamWriter writer = new StreamWriter("./lastreboottime.txt", false);
                writer.WriteLine("[Last Reboot Time]");

                PerformanceCounter uptime = new PerformanceCounter("System", "System Up Time");
                uptime.NextValue();
                TimeSpan t = TimeSpan.FromSeconds(uptime.NextValue());
                DateTime final = DateTime.Now.Subtract(t);
                //System.Console.WriteLine(final);
                //tools.setLastReboot(final);
                writer.WriteLine(final);
                writer.Close();
            }
           

            //
            StreamReader sr_b = new StreamReader("./lastreboottime.txt");
            external.Add(sr_b.ReadLine());
            external.Add(sr_b.ReadLine());

            sr_b.Close();
            //Send chunks to functions here

            Toolbox tb = tIn;
            ConfStore cStore = i_cs;

            foreach (Chunk c in chunks)
            {

                ParserChain ts = new ParserChain(c.getChunk(), tb, cStore);
            }

            ParserChain tc = new ParserChain(external, tb, cStore);


            //Debugging lines
            //string strTest = tb.ToString();
            //System.Console.WriteLine(tb.checkSql());
            //System.Console.WriteLine(startupChunk[1]);
            //Process.Start(startupChunk[1]);

            //ParserChain ts = new ParserChain(chunks[0].getChunk(), new Toolbox());
        }

        public void Stop()
        {
            Program.writeLog("Manually stopping service");
        }
    }
    

}
