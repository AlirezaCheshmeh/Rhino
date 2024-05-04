using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Database;

namespace Infrastructure.Database.DependencyInjection
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDataContext>
    {
        public ApplicationDataContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDataContext>();
            //var connectionString = configuration.GetConnectionString("Default");


            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDataContext>();
            optionsBuilder.UseSqlServer("Server=.;Initial Catalog=Clean_ArchitectureDB;Integrated Security=true;TrustServerCertificate=True;");

            //return new ApplicationDataContext(optionsBuilder.Options);
            return null;
        }
    }
}
