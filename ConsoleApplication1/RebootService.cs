using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Websdepot
{
    public class RebootService
    {
        public void Start()
        {
            RebootConfigService.Configure();
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
            Program.readConf(cStore, magicBox);

            //Generate the MD5 and store the MD5
            Program.createHashFile(Program.createHash());

            magicBox.setHash();

            //Create the inital SQL connection
            //  -By now the toolbox and configuration settings store is populated with data
            magicBox.connectSql();

            //update the last checkin time
            magicBox.updateLastCheckin();

            //Set up TimeSpan variables to store intervals the program will use for checking in
            TimeSpan sqlInterval = TimeSpan.FromMilliseconds(magicBox.getSqlInterval());
            TimeSpan rebootInterval = TimeSpan.FromMilliseconds(magicBox.getRebootDelay());

            Program.delayWait(rebootInterval);

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
            var sqlTimer = new System.Threading.Timer((e) =>
            {
                //magicBox.updateLastCheckin();                
                Console.WriteLine(sqlInterval.TotalMinutes.ToString() + " minutes has passed");

                // Check if a connection exists
                if (magicBox.checkConnection())
                {
                    //Update the last check in time
                    magicBox.updateLastCheckin();
                    Program.createHashFile(Program.createHash());
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
                        Program.readConf(cStore, magicBox);
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
            var rebootTimer = new System.Threading.Timer((f) =>
            {
                Console.WriteLine(rebootInterval.TotalMinutes.ToString() + " minutes has passed");

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
                        magicBox.runReb();
                    }
                }

            }, null, TimeSpan.Zero, rebootInterval);

            //This while loop exists to keep the timer threads from dying prematurely due to the end of the main program
            //while (true) {}
        }
        public void Stop()
        {

        }
    }
    

}
