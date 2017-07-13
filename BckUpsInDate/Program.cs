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
            int returnVal = 2;      // 0 = OK, 2 = ERROR
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
                msg = "Expected sensor parameters not found. e.g. filename, unc-path, password, username";
                Console.WriteLine($"{filesize}:{msg}");
                return returnVal;
            }

            string uncPath = args[1];
            Uri uncPathUri = new Uri(uncPath);
            // https://gist.github.com/AlanBarber/92db36339a129b94b7dd
            // I've put the class from the gist in a class library:
            // referencing class library project "WinNetConnectUnc"
            // The class will raise an error on connection failure ...
            // and if this sensor exe is being called by PRTG, then PRTG
            // will capture and display the standard-error
            var credentials = new NetworkCredential(args[3], args[2]);

            using (new NetworkConnection(uncPath, credentials))
            {
                // ASSUMES this sensor is looking for back-up made in same 24-hour period (earlier)
                string today = DateTime.Now.ToString("yyyyMMdd");
                string fileName = args[0].Replace("YYYYMMDD", today);
                fileName = fileName.Replace("-HHMM", "*");
                string path = $"{uncPathUri.LocalPath}/";
                bool fileNameExists = false;
                msg = $"Looking for {fileName}: ";

                if(fileName.Contains("*"))
                {
                    string[] files = Directory.GetFiles(uncPathUri.LocalPath, fileName, System.IO.SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        fileNameExists = true;
                        fileName = files[0];
                        path = "";
                    }
                }

                if (fileNameExists || File.Exists($"{path}{fileName}"))
                {
                    filesize = new FileInfo($"{path}{fileName}").Length.ToKB();
                    msg += $"FOUND! {DateTime.Now.ToString("h:mm tt")}";
                    returnVal = 0;
                }
                else
                {
                    msg += $"NOT found! {DateTime.Now.ToString("h: mm tt")}";
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