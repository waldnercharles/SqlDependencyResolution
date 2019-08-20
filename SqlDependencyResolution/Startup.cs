using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SqlDependencyResolution
{
    public static class Startup
    {
        public static ServiceProvider CreateServiceProvider()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection()
                .AddLogging(options => options
                    .AddConfiguration(configuration)
                    .AddConsole()
                    .AddDebug());

            ConfigureServices(services, configuration);
            return services.BuildServiceProvider();
        }

        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationContext>(options => {
                var connectionString = configuration["ConnectionStrings:DefaultConnection"];
                if (connectionString.Contains("EFProviders.InMemory"))
                {
                    options.UseInMemoryDatabase("Test");
                }
                else
                {
                    options.UseSqlServer(connectionString);
                }
            });

            services.AddScoped<LogicTablePermissionRepository>();
        }
    }
}
