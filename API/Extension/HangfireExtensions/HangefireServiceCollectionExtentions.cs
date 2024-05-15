using Hangfire.Dashboard;
using Hangfire;
using System.Diagnostics.CodeAnalysis;
using Hangfire.SqlServer;
using Application.BackgroundServices;

namespace API.Extension.HangfireExtensions
{

    public class AllowAnnoymusFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            return true;
        }
    }
    public static class HangefireServiceCollectionExtentions
    {
        public static IServiceCollection AddCustomHangFireServer(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Hangfire services.
            services.AddHangfire(c => c
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration["HangfireOptions:DbConnection"], new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            // Add the processing server as IHostedService
            services.AddHangfireServer();
            //add job for TelegramBot
            services.AddHostedService<TelegramJobSchedule>();
            //add job for remiders
            services.AddHostedService<ConfigJobSchedule>();
            return services;
        }

        public static IApplicationBuilder UseCustomHangfireDashbord(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new IDashboardAuthorizationFilter[] { new AllowAnnoymusFilter() }
                //    Authorization = new[] { new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                //    {
                //        RequireSsl = true,
                //        LoginCaseSensitive = true,
                //        Users = new []
                //        {
                //            new BasicAuthAuthorizationUser
                //            {
                //                Login = configuration["HangfireOptions:DashbordUserName"],
                //                PasswordClear = configuration["HangfireOptions:DashbordPassword"],
                //            }
                //        }

                //    })}
            });

            return app;
        }
    }
}
