# UPSMonitor

Short version: I'm underwhelmed by the UPS support built into Windows.

A work-in-progress, but it does work!

TODO
* package for download
* write real documentation
* provide details in a blog article

The solution contains two projects that work together.

One is a Windows Service which does the following:
* finds the UPS
* detects disconnects / reconnects
* detects battery discharge / AC power states
* warns at various low-charge states
* optionally logs this information to the Windows Event Log
* optionally sends this information via email
* optionally posts UI notifications to the System Tray app

The other is a Windows System Tray app which displays messages posted
by the Windows Service as notification pop-ups (aka "Toasts", which then
move to the Windows Action Center). It can also display a history of
the last 100 messages, and these are saved and restored upon reload.
This app is necessary because Windows Services run in a hidden session
which doesn't allow UI interactivity.

If you want to try it before committing to setting it up "for real"
you can just run both programs directly. The Windows Service will show
console output if run interactively instead of being installed as a
service.

If you get an error, execute the `ConfigWinRM.cmd` batch file to enable
the Windows Remote Management protocol server. This lets the program
query battery status via CIM, which is really WMI under the hood.

Eventually I'll compile and zip a downloadable release, but if you
want to compile it and try it out, have a look at appsettings.json for
the configuration options. They're reasonably self-explanatory.
