using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

/**
 * Using as a reference:
 * https://kb.paessler.com/en/topic/74481-can-the-customer-sensor-exe-be-a-ui-application
 * https://kb.paessler.com/en/topic/15813-custom-parameters-in-custom-sensors
 * https://www.paessler.com/manuals/prtg/exe_script_advanced_sensor
 * 
 * ... etc. ,and, detailed reference available in web interface for installation
 * 
 * THIS IS NOT AN ADVANCED SENSOR e.g.
 * https://www.paessler.com/manuals/prtg/exe_script_advanced_sensor
 * 
 * ... THIS IS A STANDARD SENSOR.
 * 
 * Extract (modified by me) from linked pages:
 * ******************************************
 * 
 * 
 * COMMAND LINE: test of sensor: sensorexe parameter > result.txt
 * ************
 * 
 * 
 * PARAMETER:
 * *********
 * 
 * In the "parameter" field you can use the following placeholders:
 *
 * Placeholder      |       Description
 * -----------------------------------------------------------------------------
 * %sensorid        |       The ID of the EXE/Script sensor                     |
 * %deviceid        |       The ID of the device the sensor is created on       |
 * %groupid         |       The ID of the group the sensor is created in        |
 * %probeid         |       The ID of the probe the sensor is created on        |
 * %host            |       The IP address/DNS name entry of the device the     |
 *                  |       sensor is created on                                |
 * %device          |       The name of the device the sensor is created on     |
 * %group           |       The name of the group the sensor is created in      |
 * %probe           |       The name of the probe the sensor is created on      |
 * %name or %sensor |       The name of the EXE/Script sensor                   |
 *                  |                                                           |
 *                  |       May be inherited by parent:                         |
 *                  |       ---------------------------                         |
 * %windowsdomain   |       The domain for Windows access                       |
 * %windowsuser     |       The user name for Windows access                    |
 * %windowspassword |       The password for Windows access                     |
 * %linuxuser       |       The user name for Linux access                      |
 * %linuxpassword   |       The password for Linux access                       | 
 * %snmpcommunity   |       The community string for SNMP v1 or v2              |
 * -----------------------------------------------------------------------------
 * 
 * Please make sure you write the placeholders in quotes to ensure that they are 
 * working properly if their values contain blanks. Use single quotation marks ' ' 
 * with PowerShell scripts, and double quotes " " with all others.
 * 
 * 
 * RETURN VALUE (standard out)
 * ************
 *  ---------------
 * | value:message |
 *  ---------------
 * 
 * Value has to be a 64-bit integer or float. 
 * It will be used as the resulting value for this sensor 
 * (for example, bytes, milliseconds) and stored in the database. 
 * The message can be any string (maximum length: 2000 characters).
 * "Last Msg" is at the top of the Overview screen, in PRTG, for the sensor
 * 
 * 
 * EXIT CODE of the EXE has to be one of the following values:
 * *********
 * 
 * Value | Description
 * -----------------------------------------------------------------------------
 *   0   | OK                                                                   |
 *   1   | WARNING                                                              |
 *   2   | System Error (e.g. a network/socket error)                           |
 *   3   | Protocol Error (e.g. web server returns a 404)                       |
 *   4   | Content Error (e.g. a web page does not contain a required word)     |
 * -----------------------------------------------------------------------------
 * 
 * This custom sensor is to run on my dedicated email-server where
 * vbscript/bat/PowerShell, via windows scheduler, together generate
 * 3x back-up files for the email installation:
 * https://github.com/xarta/hmailserver-backup-scripts
 * 
 * - settings                   e.g. 20170711-0500HMsettings.7z     127 KB 
 * - MySQL dump                 e.g. 20170711hmdump.sql.7z       10,858 KB
 * - 7z of eml files etc.       e.g. 20170711hmdata.7z          981,900 KB
 * 
 * I want this sensor to alert me if it can't find a backup file with the
 * current date. My back-up scripts run at 5am, so I could run this check
 * once per day, later on.  8am maybe (don't want to be too early in the
 * morning, but not too late either).
 * 
 * After further research and thought, it seems prudent to make a general
 * purpose sensor, where I can pass the filename to the sensor from PRTG.
 * Assumption: the date part will always be in the same format - YYYYMMDD
 * at the very start of the filename. With optional time-part.
 * 
 * The file size could be returned as the "value" so can be graphed in PRTG,
 * maybe with bounds set to alert "unusual" values.
 * 
 * So there will be 1:1 sensors:files  (3x instances of the sensor) for my 3 files.
 * An advanced sensor could do multichannel but keeping it simple for now.
 * 
 */

/* Initially this will be crude. Just installed Vis Studio 2017 on my laptop.
 * .Net Core
 * As I revise .Net which I learned 11.5 to over 7-months ago, I'll iteratively
 * re-factor and improve this tiny app.
 */


namespace BackUpsInDateNetFramework
{
    class Program
    {
        static int Main(string[] args)
        {


            int returnVal = 0;      // 0 = OK, 2 = ERROR
            Int64 filesize = 0;     // in KB
            string msg = "none";    // Use "file-found", "file-missing" etc.

            if (Directory.Exists(Properties.Settings.Default.PRTGcustSenseExtra))
            {
                // EXPECTING:
                // args[0] == filename
                
                // E.G. 3 sensors:

                //      YYYYMMDD-0500HMsettings.7z     (ignore -0500)
                //      YYYYMMDDhmdump.sql.7z     
                //      YYYYMMDDhmdata.7z         

                // args[1] == unc path for search folder e.g.       \\XWIFI02\USBDisk1_Volume1  for me
                // ... a string for net use can be generated e.g. \\\\XWIFI02\\USBDisk1_Volume1 for me
                // args[2] == unc net use password
                // args[3] == unc net use username
                
                // e.g. for me:  \\\\XWIFI02\\USBDisk1_Volume1 $password /USER:$user
                // TODO: look at SecureString options for this scenario - relevant?


                // expecting filename to look for in args[0]
                // Date placeholder: YYYYMMDD
                // Time placeholder: -HHMM ... ignored.
                // test/debug: check args - output to file
                if (args.Length > 0)
                {
                    // WARNING: will output password to file, if included in parameters
                    System.IO.File.WriteAllLines(Properties.Settings.Default.PRTGcustSenseExtra, args);
                }
                else
                {
                    string[] noArgs = { "No args found" };
                    System.IO.File.WriteAllLines(Properties.Settings.Default.PRTGcustSenseExtra, noArgs);
                }
            }

            if(args.Length < 4)
            {
                returnVal = 2;
                msg = "Expected sensor parameters not found. e.g. filename, unc-path, password, username";
                Console.WriteLine($"{filesize}:{msg}");
                return returnVal;
            }

            string uncPath = args[1];
            Uri uncPathUri = new Uri(uncPath);
            string uncPathEsc = uncPath.Replace(@"\", @"\\");

            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = $"/C net use {uncPathEsc} {args[2]} /USER:{args[3]}";
                process.StartInfo = startInfo;
                Process.Start(startInfo);
            }
            catch (Exception)
            {
                returnVal = 2;
                throw; // TODO: Custom-Outer-Throw-Message/Inner (existing) exception
            }

            // ASSUMES this sensor is looking for back-up made in same 24-hour period (earlier)
            string today = DateTime.Now.ToString("yyyyMMdd");
            string fileName = args[0].Replace("YYYYMMDD",today);
            msg = $"Looking for {fileName}: ";

            //TODO: wildcard for times in format -0500 (regex maybe)???

            if (File.Exists($"{uncPathUri.LocalPath}/{fileName}"))
            {
                filesize = new FileInfo($"{uncPathUri.LocalPath}/{fileName}").Length.ToKB();
                msg += $"FOUND! {DateTime.Now.ToString("h:mm tt")}";
                returnVal = 0;
            }
            else
            {
                filesize = 0;
                msg += $"NOT found! {DateTime.Now.ToString("h: mm tt")}";
                returnVal = 2;
            }
                
            Console.WriteLine($"{filesize}:{msg}");
            return returnVal;
        }
    }

    public static class MyExtensions
    {
        public static Int64 ToKB(this Int64 byteValue)
        {
            return (Int64)Math.Floor(((double)byteValue / 1024));
        }
    }
}
