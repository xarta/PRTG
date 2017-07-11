using System;
using System.Collections.Generic;
using System.Linq;
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
 * I want this sensor to alert me if the last back-ups are more than 
 * 24-hours old.
 * 
 * After further research and thought, it seems prudent to make a general
 * purpose sensor, where I can pass the filename to the sensor from PRTG.
 * Assumption: the date part will always be in the same format - not
 * part of the filename. With optional time-part.
 * 
 * The file size could be returned as the "value" so can be graphed in PRTG,
 * maybe with bounds set to alert "unusual" values.
 * 
 * So there will be 1:1 sensors:files  (3x instances of the sensor) for my 3 files.
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
            // testing
            Console.WriteLine($"0:{args[0]}");
            return 0;
        }
    }
}
