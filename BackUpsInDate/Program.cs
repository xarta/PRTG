using System;

/**
 * Using as a reference:
 * https://kb.paessler.com/en/topic/69346-how-can-i-monitor-the-number-of-users-logged-in-to-windows
 * 
 * This custom sensor is to run on my dedicated email-server where
 * vbscript/bat/PowerShell, via windows scheduler, together generate
 * 3x back-up files for the email installation:
 * 
 * - settings
 * - MySQL dump
 * - 7z of eml files etc.
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
