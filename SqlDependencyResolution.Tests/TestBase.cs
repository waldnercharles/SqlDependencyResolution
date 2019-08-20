using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlDependencyResolution
{
    public class TestBase
    {
        private ServiceProvider serviceProvider;

        public IServiceProvider ServiceProvider
        {
            get { return this.serviceProvider ?? (this.serviceProvider = Startup.CreateServiceProvider()); }
        }

        [TestCleanup]
        public void TestBaseCleanup()
        {
            if (this.serviceProvider != null)
            {
                this.serviceProvider.Dispose();
                this.serviceProvider = null;
            }
        }
    }
}
