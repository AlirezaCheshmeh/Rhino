using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Extensions
{
    public static class SerilogConfig
    {
        public static Action<HostBuilderContext, LoggerConfiguration> ConfigureLogger =>
              (hostingContext, loggerConfiguration) =>
              {

                  var env = hostingContext.HostingEnvironment;
                  var applicationName = env.ApplicationName;
                  var environmentName = env.EnvironmentName;


                  loggerConfiguration.MinimumLevel.Information()
                      .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                      .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                      .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                      .Enrich.WithProperty("ApplicationName", applicationName)
                      .Enrich.WithProperty("EnvironmentName", environmentName)
                      .WriteTo.Console();

                 
                  var seqUrl = hostingContext.Configuration["SeqOption:Url"];

                  if (!string.IsNullOrEmpty(seqUrl))
                  {
                      loggerConfiguration.WriteTo.Seq(seqUrl);
                  }
              };
    }
}
