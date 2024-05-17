using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Database;

namespace Infrastructure.Database.DependencyInjection
{
    public static class DatabaseDependencyInjection
    {
        //add generic repository Dependency Method
        public static IServiceCollection AddRepositoriesDependency(this IServiceCollection Services)
        {
            return Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        }

        //Add DbContext Dependency Method
        public static IServiceCollection AddDbContextServices(this IServiceCollection Services, IConfiguration configuration)
        {

            Services.AddScoped<IApplicationDataContext, ApplicationDataContext>();
            Services.AddDbContext<ApplicationDataContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("LocalConnection"));
            }, ServiceLifetime.Scoped);

            return Services;
        }


        //Add UnitOfWork Dependency Method
        public static IServiceCollection AddUnitOfWorkServices(this IServiceCollection Services)
        {
            Services.AddScoped<IUnitOfWork, UnitOfWork>();
            return Services;
        }
    }
}
