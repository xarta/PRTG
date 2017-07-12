using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using System.ComponentModel;



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

            //WNetCancelConnection2("\\\\XWIFI02\\USBDisk1_Volume1", 0, true);
            Console.WriteLine($"{filesize}:{msg}");
            return returnVal;
        }

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string name, int flags,
            bool force);
    }

    public static class MyExtensions
    {
        public static Int64 ToKB(this Int64 byteValue)
        {
            return (Int64)Math.Floor(((double)byteValue / 1024));
        }
    }
}
