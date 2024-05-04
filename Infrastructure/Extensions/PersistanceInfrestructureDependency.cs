
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    public static class PersistanceInfrestructureDependency
    {
        public static IServiceCollection AddPersistanceInfrestructurelayarServcies(this IServiceCollection Servcies, IConfiguration configuration)
        {
            //Add DbContext Services
            Database.DependencyInjection.DatabaseDependencyInjection.AddDbContextServices(Servcies, configuration);

            //Add UnitOfWork Services
            Database.DependencyInjection.DatabaseDependencyInjection.AddUnitOfWorkServices(Servcies);


            return Servcies;
        }
    }
}
