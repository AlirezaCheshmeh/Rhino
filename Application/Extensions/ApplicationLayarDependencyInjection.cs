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

namespace Application.Extensions
{
    public static class ApplicationLayerDependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection Services, IConfiguration configuration)
        {
            //ad settings
            Services.AddSettings(configuration);

            //Add AutoMapper Services
            Services.AddMapperServcies();

            // Add MediatR
            Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            //add scope transient singletone 
            var commonAssembly = typeof(SiteSettings).Assembly;
            var entitiesAssembly = typeof(IEntity).Assembly;
            var dataAssembly = typeof(AppContext).Assembly;
            var applicationAssmemly = typeof(IRequest<>).Assembly;
            Services.Scan(s =>
            s.FromAssemblies(commonAssembly, entitiesAssembly, dataAssembly, applicationAssmemly)
            .AddClasses(c => c.AssignableTo(typeof(IScopedDependency))
            ).AsImplementedInterfaces()
            .WithScopedLifetime());

            Services.Scan(s =>
            s.FromAssemblies(commonAssembly, entitiesAssembly, dataAssembly, applicationAssmemly)
            .AddClasses(c => c.AssignableTo(typeof(ITransientDependency))
            ).AsImplementedInterfaces()
            .WithScopedLifetime());

            Services.Scan(s =>
            s.FromAssemblies(commonAssembly, entitiesAssembly, dataAssembly, applicationAssmemly)
            .AddClasses(c => c.AssignableTo(typeof(ISingletonDependency))
            ).AsImplementedInterfaces()
            .WithScopedLifetime());

            //add cqrs 
            Services.AddCqrs();

            //add service response
            Services.AddScoped<IServiceResponse, ServiceRespnse>();
            return Services.AddScoped(typeof(IPaymentZarinPalServiceResponse<>), typeof(PaymentZarinPalServiceResponse<>));

            return Services;
        }
    }
}
