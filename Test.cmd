cd /d "%~dp0"

ECHO OFF

SET CONFIG=%1
SET NETFX=%2
SET NET80=%3
SET NET90=%4
SET FORMAT=%5
SET EXTRA=%6

IF [%1] EQU [] (SET CONFIG=Debug)
IF [%2] EQU [] (SET NETFX=1)
IF [%3] EQU [] (SET NET80=1)
IF [%4] EQU [] (SET NET90=1)
IF [%5] EQU [] (SET FORMAT=html)

dotnet build linq2db.slnx -c %CONFIG% -v m

IF %NETFX% NEQ 0 (dotnet test .build/bin/Tests/%CONFIG%/net462/linq2db.Tests.dll -f net462 -l %FORMAT%;LogFileName=net462.%FORMAT% %EXTRA%)
IF %NET80%  NEQ 0 (dotnet test .build/bin/Tests/%CONFIG%/net8.0/linq2db.Tests.dll -f net8.0 -l %FORMAT%;LogFileName=net80.%FORMAT% %EXTRA%)
IF %NET90%  NEQ 0 (dotnet test .build/bin/Tests/%CONFIG%/net9.0/linq2db.Tests.dll -f net9.0 -l %FORMAT%;LogFileName=net90.%FORMAT% %EXTRA%)

IF %NETFX% NEQ 0 (dotnet test .build/bin/Tests.EntityFrameworkCore/%CONFIG%/net462/linq2db.EntityFrameworkCore.Tests.dll -f net462 -l %FORMAT%;LogFileName=net462.efcore.%FORMAT% %EXTRA%)
IF %NET80%  NEQ 0 (dotnet test .build/bin/Tests.EntityFrameworkCore/%CONFIG%/net8.0/linq2db.EntityFrameworkCore.Tests.dll -f net8.0 -l %FORMAT%;LogFileName=net80.efcore.%FORMAT% %EXTRA%)
IF %NET90%  NEQ 0 (dotnet test .build/bin/Tests.EntityFrameworkCore.STS/%CONFIG%/net9.0/linq2db.EntityFrameworkCore.Tests.dll -f net9.0 -l %FORMAT%;LogFileName=net90.efcore.%FORMAT% %EXTRA%)

