@echo off
@echo=
@echo  ********************************************
@echo  *                                          *
@echo  *      卸载：BingImageDownloadService      *
@echo  *                                          *
@echo  ********************************************
@echo=

@echo off
REM 检查是否已经以管理员权限运行
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo 请求管理员权限...
    PowerShell -Command "Start-Process -FilePath '%~0' -Verb RunAs"
    exit /b
)

REM 以下为正常脚本内容
cd /d %~dp0

REM 其余代码...

net stop BingImageDownloadService
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /u "BingImageDownloadServiceForWindows.exe"
sc delete BingImageDownloadService

@timeout /T 10 /NOBREAK
REM 若遇到脚本问题可以打开(添加)pause来暂停跟踪问题所在 ...
::@pause