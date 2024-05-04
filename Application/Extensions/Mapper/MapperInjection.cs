using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Application.Extensions.Mapper
{
    public static class MapperInjection
    {
        public static IServiceCollection AddMapperServcies(this IServiceCollection Services)
        {
            // Set Mapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MapperProfile.MapperProfile());
            });
            var mapper = config.CreateMapper();
            Services.AddSingleton(mapper);
            return Services;
        }
    }
}
