@echo off
@echo=
@echo  ******************************************
@echo  *                                        *
@echo  *      安装BingImageDownloadService      *
@echo  *                                        *
@echo  ******************************************
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

if exist "%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe" (
    %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe "BingImageDownloadServiceForWindows.exe"
    if %errorlevel% neq 0 (
        echo 安装 BingImageDownloadServiceForWindows 服务时出错，请查看相关日志。
        pause
        exit /B %errorlevel%
    )
    sc config BingImageDownloadService start= AUTO
    net Start BingImageDownloadService
    
    echo BingImageDownloadServiceForWindows 服务已成功安装并启动。
) else (
    echo 检测到系统中未安装 .NET Framework 4.0 或者 InstallUtil.exe 文件不存在，请确保正确安装 .NET Framework 4.0 后再运行此脚本。
    pause
    exit /B 1
)

@timeout /T 10 /NOBREAK
REM 若遇到脚本问题可以打开(添加)pause来暂停跟踪问题所在 ...
::@pause