# PRTG
My PRTG custom sensors

11th July 2017: just, just started project.  Set-up of Visual Studio 2017 on my laptop was a pain as the previous version wasn't removed fully after I ditched a partition in order to increase the C: drive leading to an "invalid drive" when trying to re-install.  Just getting used to Visual Studio again, and started a .Net Core app for the first time since December ... just getting my bearings.  Nothin' to see yet.

*Purpose*
I use PRTG in my home-network.  My email back-up scripts, for example:
https://github.com/xarta/hmailserver-backup-scripts
... create some back-up files with dates in the file-name.  My first custom PRTG sensor is to check that the back-up files exist, with reasonable attributes e.g. sizes, with dates valid for the previous 24-hours.  If the test fails, then my phone will be informed which in turn will alert me on my Pebble watch.

*How*
PRTG custom sensors live in custom sensor folders on the main PRTG install, and on the remote system if there is a remote probe.  (Alterantives to having a remote probe might be any app that could, for example, reply to a HTTP request with a value:message data.)  So they just have to print to console the value:message ... and the sensor's settings in PRTG calibrated accordingly.  E.g. see:
https://kb.paessler.com/en/topic/74655-how-to-monitor-mounted-windows-volumes
