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

// https://github.com/xarta/PRTG    for README.md

namespace PRTGSensors
{
    class Program
    {
        static int Main(string[] args)
        {
            int returnVal = 0;      // 0 = OK, 2 = ERROR
            Int64 filesize = 0;     // in KB
            string msg = "none";    // Use "file-found", "file-missing" etc.

            /**
             * args
             * args[0] == filename
             * args[1] == unc path for folder to search
             * args[2] == unc resource password nb: not sure if can use SecureString for params?
             * args[3] == unc resource username
             * 
             * e.g. hMailServer back-ups
             *      YYYYMMDD-HHMMHMsettings.7z     (ignore -HHMM)
             *      YYYYMMDDhmdump.sql.7z 
             *      YYYYMMDDhmdata.7z
             */

            // to help with debug - output args to readable text file
            // WARNING: will output password to file, if included in parameters
            if (Directory.Exists(Properties.Settings.Default.PRTGcustSenseExtraFolder))
            {
                string PRTGcustSenseExtra = Properties.Settings.Default.PRTGcustSenseExtraFolder +
                   Properties.Settings.Default.PRTGcustSenseExtraFile;

                // test/debug: check args - output to file
                if (args.Length > 0)
                {
                    System.IO.File.WriteAllLines(PRTGcustSenseExtra, args);
                }
                else
                {
                    string[] noArgs = { "No args found" };
                    System.IO.File.WriteAllLines(PRTGcustSenseExtra, noArgs);
                }
            }

            // primitive sanity check - bail-out early if not enough parameters
            if (args.Length < 4)
            {
                returnVal = 2;
                msg = "Expected sensor parameters not found. e.g. filename, unc-path, password, username";
                Console.WriteLine($"{filesize}:{msg}");
                return returnVal;
            }

            string uncPath = args[1];
            Uri uncPathUri = new Uri(uncPath);
            string uncPathEsc = uncPath.Replace(@"\", @"\\");
            // https://gist.github.com/AlanBarber/92db36339a129b94b7dd
            // I've put class from the gist in a class library:
            // referencing class library project "WinNetConnectUnc"
            var credentials = new NetworkCredential(args[3], args[2]);

            using (new NetworkConnection(uncPath, credentials))
            {
                // ASSUMES this sensor is looking for back-up made in same 24-hour period (earlier)
                string today = DateTime.Now.ToString("yyyyMMdd");
                string fileName = args[0].Replace("YYYYMMDD", today);
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