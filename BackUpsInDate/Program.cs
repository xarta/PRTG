using System;

/**
 * Using as a reference:
 * https://kb.paessler.com/en/topic/74481-can-the-customer-sensor-exe-be-a-ui-application
 * 
 * Extract from linked page:
 * ____________________
 * | value  :  message |
 *  --------------------
 * 
 * Value has to be a 64-bit integer or float. 
 * It will be used as the resulting value for this sensor 
 * (for example, bytes, milliseconds) and stored in the database. 
 * The message can be any string (maximum length: 2000 characters).
 * 
 * 
 * The EXIT CODE of the EXE has to be one of the following values:
 * 
 * Value | Description
 *   0   | OK
 *   1   | WARNING
 *   2   | System Error (e.g. a network/socket error)
 *   3   | Protocol Error (e.g. web server returns a 404)
 *   4   | Content Error (e.g. a web page does not contain a required word)

 * This custom sensor is to run on my dedicated email-server where
 * vbscript/bat/PowerShell, via windows scheduler, together generate
 * 3x back-up files for the email installation:
 * https://github.com/xarta/hmailserver-backup-scripts
 * 
 * - settings                   e.g. 20170711-0500HMsettings.7z     127 KB 
 * - MySQL dump                 e.g. 20170711hmdump.sql.7z       10,858 KB
 * - 7z of eml files etc.       e.g. 20170711hmdata.7z          981,900 KB
 * 
 * I want this sensor to alert me if the last back-ups are more than 
 * 24-hours old.
 * 
 */

/* Initially this will be crude. Just installed Vis Studio 2017 on my laptop.
 * .Net Core
 * As I revise .Net which I learned 11.5 to over 7-months ago, I'll iteratively
 * re-factor and improve this tiny app.
 */

namespace BackUpsInDate
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
