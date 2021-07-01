using Microsoft.Extensions.Logging;

namespace Tentakel.Extensions.Logging.Providers
{
    public class LoggerSinkProviderOptions
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }

   
    internal class LoggerSinkProviderOptionsSetup : ConfigureFromConfigurationOptions<LoggerSinkProviderOptions>
    {
      
        public LoggerSinkProviderOptionsSetup(ILoggerProviderConfiguration<LoggerSinkProvider> providerConfiguration)
            : base(providerConfiguration.Configuration)
        {
        }
    }
}