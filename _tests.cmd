dotnet test "%~dp0Oleander.Extensions.Configuration\Oleander.Extensions.Configuration.sln" --configuration Release 
dotnet test "%~dp0Oleander.Extensions.Configuration.Json\Oleander.Extensions.Configuration.Json.sln" --configuration Release 
dotnet test "%~dp0Oleander.Extensions.DependencyInjection\Oleander.Extensions.DependencyInjection.sln" --configuration Release 
dotnet test "%~dp0Oleander.Extensions.Logging.Abstractions\Oleander.Extensions.Logging.Abstractions.sln" --configuration Release 
dotnet test "%~dp0Oleander.Extensions.Logging\Oleander.Extensions.Logging.sln" --configuration Release 
dotnet test "%~dp0Oleander.Extensions.Logging.TextFormatters.Abstractions\Oleander.Extensions.Logging.TextFormatters.Abstractions.sln" --configuration Release 
dotnet test "%~dp0Oleander.Extensions.Logging.TextFormatters\Oleander.Extensions.Logging.TextFormatters.sln" --configuration Release 
dotnet test "%~dp0Oleander.Extensions.Logging.Console\Oleander.Extensions.Logging.Console.sln" --configuration Release 
dotnet test "%~dp0Oleander.Extensions.Logging.File\Oleander.Extensions.Logging.File.sln" --configuration Release 
dotnet test "%~dp0Oleander.Extensions.Logging.File.Benchmarks\Oleander.Extensions.Logging.File.Benchmarks.sln" --configuration Release 
dotnet test "%~dp0Oleander.Extensions.Logging.ZipFile\Oleander.Extensions.Logging.ZipFile.sln" --configuration Release 

pause
