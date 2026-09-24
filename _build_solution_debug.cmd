@echo off

rem dotnet build "%~dp0extensions.sln" --configuration Debug --property:VersionDevSuffix=dev --property:GeneratePackageOnBuild=false --property:versioningTask-disabled=true

dotnet test --solution "%~dp0extensions.sln" --configuration Debug --ignore-exit-code 8

rem dotnet pack "%~dp0extensions.sln" --configuration Debug --output "%~dp0packoutput" --no-restore --property:versioningTask-disabled=true --property:VersionDevSuffix=dev
pause


