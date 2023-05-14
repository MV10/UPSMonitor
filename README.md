# UPSMonitor

A simple service that notifies and logs when a UPS backup switches between
AC and battery power, and can log a few other status events.

Later I will link to a technical article about this on my blog, [mcguirev10](https://mcguirev10.com).

## First-Time Installation

* Download the [v1.0.0](https://github.com/MV10/UPSMonitor/releases/tag/v1.0.0) archive file.
* Create a directory: `C:\Program Files\UPSMonitor`
* Unzip the archive into that directory.
* Open an administrator console window and CD to that directory.
* Run the `ConfigWinRM` batch file to start the WinRM service.
* **Optional:** Edit `appsettings.json` to enable Event Log output and/or email
notifications. See the _Configuration Options_ section below for details.
* **Optional:** Before creating / running the monitor as a Windows Service, you can
run `UPSMonitorService` interactively from the console window to see how it works.
* Add `UPSMonitor.exe` to your Startup group, and execute it now.
* Run the `Create` batch file to create the Windows Service entry.
* Run the `Run` batch file to start the Windows Service

## Removal

* Close the `UPSMonitor` System Tray program (right click the tray icon).
* Remove `UPSMonitor` from your Startup group.
* Open an administrator console window and CD to the `C:\Program Files\UPSMonitor` directory.
* Run the `Stop` batch file.
* Run the `Delete` batch file.
* Remove the directory.
* **Optional:** Remove the `HKCU\SOFTWARE\mcguirev10\UPSMonitor` registry key and contents.
This key is just a history of the last 100 events logged by the monitor.

## Features

* Windows pop-up ("Toast") notifications via System Tray app
* Logging of basic UPS details upon discovery
* Logging of AC / battery power state changes
* Logging of UPS connect / disconnect events
* Logging of various low-charge states
* Logging of changes in reported UPS health status
* Optional ability to log details to the Windows Event Log
* Optional ability to send details via email
* Event history via Windows System Tray app (right-click the tray icon)

## Configuration (appsettings.json)

By default, the program is configured to only provide pop-up notifications
with a 1-second polling interval.

### _Settings_ section

**PollingSeconds**: How often the service queries for UPS status, expressed
in seconds. Default is 1.

**BatteryName**: Which battery to monitor if the system might be connected
to more than one (for example, a laptop with an internal battery but also
sometimes connected to a larger stand-alone UPS). Default is blank, which
monitors the first battery reported.

**NotificationEmails**: Whether to send events via email. Default is false.
See the _Email section_ settings below for more options.

**NotificationEventLog**: Whether to write events to the Windows Application
Event Log. To simplify setup, the program does not use a custom log provider.
If true, events can be found under Event ID 9001.

**NotificationPopups**: Whether to display events as a Windows desktop pop-up,
sometimes called "Toasts". Under Windows 10 and newer, these will appear on-screen
briefly, then move to the "Application Notification" area. These only work if the
`UPSMonitor` System Tray application is running. Default is true.

**RemotePopupOnly**: Whether the `UPSMonitorService` Windows Service can directly
trigger Windows "Toast" pop-up messages when running interactively, or only via
messages to the `UPSMonitor` System Tray application. This is primarily a debugging
aid. Default is false, which allows direct display of pop-up messages.

### _BatteryLevels_ section

These are the percentages at which low-battery warning notifications occur. Generally
you should configure these to 1% higher than the Windows Battery settings. If Windows
is configured for an action like Hibernate or Shutdown and this service targets the
same battery percentage, you probably won't get any notification from this service
because the configured action will happen first.

UPS Monitor doesn't (currently) take any action itself, it only provides warnings.

**Advisory**: The "early warning" point that the battery is starting to get low. There
is no Windows Power Configuration equivalent to this setting. Default is 30.

**Low**: The first of three low-battery levels. Windows Power Configuration is usually
set to 10%. UPS Monitor's default is 11.

**Reserve**: The second of three low-battery levels. Windows Power Configuration is
usually set to 7%. UPS Monitor's default is 8.

**Critical**: The last of the three low-battery levels, and the one most likely to be
configured in Windows Battery settings to take some kind of system shutdown activity.
The Windows Power COnfiguration is usually set to 5%. UPS Monitor's default is 6.

### _Email_ section

These define all the values needed to send SMTP email warnings. Although email is
disabled by default, the first few settings are configured for GMail usage. See the
_GMail Notes_ section later in the README for more details.

**MailServerDomain**: The FQDN for the mail server. Default is `smtp.gmail.com`.

**MailServerPort**: The IP port number for the mail server. Default is 587.

**UseTLS**: Whether to use SSL/TLS. Default is true.

**Subject**: The email Subject line. An asterisk (`*`) will be auto-replaced with
the computer's name. Default is `UPS Notification from *`. 

**Username**: The login username for the mail sender. GMail requires this to be a
GMail account and it must match _SenderName_. Default is `usersecrets` which is simply
a developer reminder that this value is only stored/obtained on the local machine during
development.

**Password**:  The login password for the mail sender. See the _Gmail Notes_ section
later in the README for more details. Default is `usersecrets` which is simply
a developer reminder that this value is only stored/obtained on the local machine during
development.

**SenderName**: The display name for the mail sender. GMail requires this to be a
GMail account and it must match _Username_. Default is `usersecrets` which is simply
a developer reminder that this value is only stored/obtained on the local machine during
development.

**RecipientList**: A comma-delimited list of email addresses to receive the notification.
Note most mobile phone providers also allow email SMS messaging (for example, T-Mobile uses
`##########@tmomail.net`). Default is `usersecrets` which is simply a developer reminder
that this value is only stored/obtained on the local machine during development.

### _Logging_ section

These are the default .NET Logger configuration options. Minimum log levels are set to `Warning`.
No special logging configuration is used, so any log output will be found in the Windows
Event Log. Generally you can ignore this section.

## GMail Notes ##

Google only allows SMTP email under certain conditions. Most importantly, you have to configure
the sending GMail account to allow "less secure" access, then configure a password that is specific
to the sending system. For this reason, you should probably create a dedicated GMail account for
this kind of messaging.

The relevant settings are under the general account settings (not within GMail itself), but Google
keeps changing how it works, so you will have to search to find out how to set this up, I won't be
able to track that and keep this document up to date. (The good news is that older authorizations
continue to work even when they change the UI. I have used something similar to allow my security 
system and network storage devices to email notifications for several years and haven't had to change
their configurations.)

