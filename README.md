# UPSMonitor

A work-in-progress, but it does work!

TODO: Write some docs...

Short version: I'm underwhelmed by the UPS support built into Windows.

The solution contains two projects that work together.

One is a Windows Service which does the following:
* finds the UPS
* detects disconnects / reconnects
* detects battery discharge / AC power states
* warns at various low-charge states
* logs this information to the Windows Event Log
* sends this information via email
* posts UI notifications to the System Tray app

The other is a Windows System Tray app which displays messages posted
by the Windows Service as notification pop-ups (aka "Toasts", which then
move to the Windows Action Center). It can also display a history of
the last 100 messages (since startup; these are not stored currently).
This is necessary because Windows Services run in a hidden session which
isn't supposed to have any interactivity (aka UI).

Eventually I'll compile and zip a downloadable release, but if you
want to compile it and try it out, have a look at appsettings.json for
the configuration options. They're reasonably self-explanatory for now.
