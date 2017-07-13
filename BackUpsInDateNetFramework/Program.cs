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

namespace BackUpsInDateNetFramework
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

            // primative sanity check - bail-out early if not enough parameters
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

            // TODO: look at using this instead: https://gist.github.com/AlanBarber/92db36339a129b94b7dd
            // PRTG monitor / this sensor likely running under SYSTEM ... connect unc resource
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = $"/C net use {uncPath} {args[2]} /USER:{args[3]}";
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

            WNetCancelConnection2(uncPathEsc, 0, true);
            Console.WriteLine($"{filesize}:{msg}");
            return returnVal;
        }

        // using a bit of code from: https://gist.github.com/AlanBarber/92db36339a129b94b7dd
        // to close the unc connection (credentials might persist in cache for a little bit I think)
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


    /**
     * https://gist.github.com/AlanBarber/92db36339a129b94b7dd
     * Was looking at Git subtrees then realised this was a gist
     * rather than a repository.  Thought about having a shared library
     * and a vendor folder etc. but in the end, for these tiny "sensors"
     * and this one class, and for my needs, it seems easier to just
     * duplicate this class and add it directly into my Program.cs
     * file.  I think sometimes doing things "right" just makes life
     * too complicated.  If Windows changes such that this doesn't work
     * anymore for me, then I might have to think about other things anyway
     * and a bit of copying and pasting won't take too long in this case,
     * and it will be easier to "think about" when all the eggs are in one
     * immediately apparent, obvious, visible, basket.
     */
    public class NetworkConnection : IDisposable
    {
        readonly string _networkName;

        public NetworkConnection(string networkName, NetworkCredential credentials)
        {
            _networkName = networkName;

            var netResource = new NetResource
            {
                Scope = ResourceScope.GlobalNetwork,
                ResourceType = ResourceType.Disk,
                DisplayType = ResourceDisplaytype.Share,
                RemoteName = networkName
            };

            var userName = string.IsNullOrEmpty(credentials.Domain)
                ? credentials.UserName
                : string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);

            var result = WNetAddConnection2(
                netResource,
                credentials.Password,
                userName,
                0);

            if (result != 0)
            {
                throw new Win32Exception(result, "Error connecting to remote share");
            }
        }

        ~NetworkConnection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            WNetCancelConnection2(_networkName, 0, true);
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NetResource netResource,
            string password, string username, int flags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string name, int flags,
            bool force);

        [StructLayout(LayoutKind.Sequential)]
        public class NetResource
        {
            public ResourceScope Scope;
            public ResourceType ResourceType;
            public ResourceDisplaytype DisplayType;
            public int Usage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }

        public enum ResourceScope : int
        {
            Connected = 1,
            GlobalNetwork,
            Remembered,
            Recent,
            Context
        };

        public enum ResourceType : int
        {
            Any = 0,
            Disk = 1,
            Print = 2,
            Reserved = 8,
        }

        public enum ResourceDisplaytype : int
        {
            Generic = 0x0,
            Domain = 0x01,
            Server = 0x02,
            Share = 0x03,
            File = 0x04,
            Group = 0x05,
            Network = 0x06,
            Root = 0x07,
            Shareadmin = 0x08,
            Directory = 0x09,
            Tree = 0x0a,
            Ndscontainer = 0x0b
        }
    }
}
