using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.SiteSetting
{
    public static class Container
    {
        public static void AddSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ICryptographySetting>(configuration.GetSection("CryptographySetting")
                .Get<CryptographySetting>());
        }
    }
}
