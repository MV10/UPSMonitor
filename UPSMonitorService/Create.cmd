@openfiles.exe 1>nul 2>&1
@if not %errorlevel% equ 0 goto :fail
sc create "UPSMonitorService" binpath= "C:\Program Files\UPSMonitor\UPSMonitorService.exe" start= delayed-auto displayname= "McGuireV10.com UPS monitoring service"
@goto:eof
:fail
@echo:
@echo Failed. Run as Administrator.
@echo:
:eof
