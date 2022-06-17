dotnet build "%~dp0Tentakel.Extensions.Configuration\Tentakel.Extensions.Configuration.sln" --configuration Release --output "%~dp0out"
dotnet build "%~dp0Tentakel.Extensions.Configuration.Json\Tentakel.Extensions.Configuration.Json.sln" --configuration Release --output "%~dp0out"
dotnet build "%~dp0Tentakel.Extensions.DependencyInjection\Tentakel.Extensions.DependencyInjection.sln" --configuration Release --output "%~dp0out"
dotnet build "%~dp0Tentakel.Extensions.Logging.Abstractions\Tentakel.Extensions.Logging.Abstractions.sln" --configuration Release --output "%~dp0out"
dotnet build "%~dp0Tentakel.Extensions.Logging\Tentakel.Extensions.Logging.sln" --configuration Release --output "%~dp0out"
dotnet build "%~dp0Tentakel.Extensions.Logging.TextFormatters.Abstractions\Tentakel.Extensions.Logging.TextFormatters.Abstractions.sln" --configuration Release --output "%~dp0out"
dotnet build "%~dp0Tentakel.Extensions.Logging.TextFormatters\Tentakel.Extensions.Logging.TextFormatters.sln" --configuration Release --output "%~dp0out"
dotnet build "%~dp0Tentakel.Extensions.Logging.Console\Tentakel.Extensions.Logging.Console.sln" --configuration Release --output "%~dp0out"
dotnet build "%~dp0Tentakel.Extensions.Logging.File\Tentakel.Extensions.Logging.File.sln" --configuration Release --output "%~dp0out"
dotnet build "%~dp0Tentakel.Extensions.Logging.File.Benchmarks\Tentakel.Extensions.Logging.File.Benchmarks.sln" --configuration Release --output "%~dp0out"
dotnet build "%~dp0Tentakel.Extensions.Logging.ZipFile\Tentakel.Extensions.Logging.ZipFile.sln" --configuration Release --output "%~dp0out"

pause
