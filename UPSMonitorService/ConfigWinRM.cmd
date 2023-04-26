@openfiles.exe 1>nul 2>&1
@if not %errorlevel% equ 0 goto :fail
winrm quickconfig
@goto:eof
:fail
@echo:
@echo Failed. Run as Administrator.
@echo:
:eof
