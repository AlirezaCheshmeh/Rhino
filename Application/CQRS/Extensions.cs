using Application.Cqrs.Behaviors;
using Application.Cqrs.Commands;
using Application.Cqrs.Commands.Dispatcher;
using Application.Cqrs.Queris;
using Application.Cqrs.Queris.Dispatcher;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Windows.Input;

namespace Application.Cqrs
{
    public static class Extensions
    {
        public static IServiceCollection AddCqrs(this IServiceCollection services)
        {
            services.AddMediatR(c =>
            {
                c.RegisterServicesFromAssemblies(typeof(ICommand<>).Assembly);
                c.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });
            services.AddValidatorsFromAssembly(typeof(ICommand<>).Assembly);
            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            services.AddScoped<IQueryDispatcher, QueryDispatcher>();
            return services;
        }
    }
}
