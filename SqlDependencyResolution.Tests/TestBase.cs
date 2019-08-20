using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlDependencyResolution
{
    public class TestBase
    {
        private ServiceProvider serviceProvider;

        [TestCleanup]
        public void TestBaseCleanup()
        {
            if (this.serviceProvider != null)
            {
                this.serviceProvider.Dispose();
                this.serviceProvider = null;
            }
        }

        protected void ConfigureServices(Action<IServiceCollection, IConfiguration> setup)
        {
            if (this.serviceProvider != null)
            {
                throw new ApplicationException("Unable to configure services. The service provider has already been initialized.");
            }

            this.serviceProvider = Startup.CreateServiceProvider(setup);
        }

        protected T GetService<T>()
        {
            var serviceProvider = this.serviceProvider ?? (this.serviceProvider = Startup.CreateServiceProvider());
            return serviceProvider.GetRequiredService<T>();
        }
    }
}
