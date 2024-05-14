using API.Extension.Interfaces;
using Application.Utility;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace API.Extension
{
    public static class InitisilizeExtention
    {

        public static IApplicationBuilder IntializeDatabase(this IApplicationBuilder app)
        {
            Assert.NotNull(app, nameof(app));


            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var dbContext = scope.ServiceProvider.GetService<ApplicationDataContext>(); //Service locator

            //Dos not use Migrations, just Create Database with latest changes
            dbContext.Database.Migrate();



            var dataInitializers = scope.ServiceProvider.GetServices<IDataInitializer>();
            foreach (var dataInitializer in dataInitializers)
                dataInitializer.InitializeData();

            return app;
        }
    }
}
