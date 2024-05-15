using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions;
using System.Reflection;
using Application.Cqrs;
using Microsoft.Extensions.Configuration;
using Application.SiteSetting;
using Application.Extensions.Mapper;
using Domain.DTOs.Shared;
using Domain.Entities.BaseEntity;
using Application.BackgroundServices;
using Microsoft.Extensions.Caching.Distributed;
using Application.Services.AuthorizeServices;
using Application.Services.CacheServices;
using Application.Services.TelegramServices;
using Application.Services.TelegramServices.Interfaces;
using Application.Services.TelegramServices.BaseMethods;

namespace Application.Extensions
{
    public static class ApplicationLayerDependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection Services, IConfiguration configuration)
        {

            //Add AutoMapper Services
            Services.AddMapperServcies();

            // Add MediatR
            Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            //add scope transient singletone 
            var entitiesAssembly = typeof(IEntity).Assembly;
            var dataAssembly = typeof(AppContext).Assembly;
            var applicationAssmemly = typeof(IRequest<>).Assembly;
            Services.Scan(s =>
            s.FromAssemblies(entitiesAssembly, dataAssembly, applicationAssmemly)
            .AddClasses(c => c.AssignableTo(typeof(IScopedDependency))
            ).AsImplementedInterfaces()
            .WithScopedLifetime());

            Services.Scan(s =>
            s.FromAssemblies(entitiesAssembly, dataAssembly, applicationAssmemly)
            .AddClasses(c => c.AssignableTo(typeof(ITransientDependency))
            ).AsImplementedInterfaces()
            .WithScopedLifetime());

            Services.Scan(s =>
            s.FromAssemblies(entitiesAssembly, dataAssembly, applicationAssmemly)
            .AddClasses(c => c.AssignableTo(typeof(ISingletonDependency))
            ).AsImplementedInterfaces()
            .WithScopedLifetime());

            //add cqrs 
            Services.AddCqrs();
            //add service response
            Services.AddScoped<IServiceResponse, ServiceRespnse>();
            //add Token Services
            Services.AddScoped<IToken, Token>();

            




            return Services;
        }
    }
}
