using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace YT2AudioConverter
{
    public static class ServiceProvider
    {
        public static IServiceProvider BuildDi(IConfiguration config)
        {
            return new ServiceCollection()
            .AddLogging(loggingBuilder =>
            {
                // configure Logging with NLog
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                loggingBuilder.AddNLog(config);
            })
            .BuildServiceProvider();
        }
    }
}

