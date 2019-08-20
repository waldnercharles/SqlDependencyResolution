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

        protected T GetService<T>()
        {
            var serviceProvider = this.serviceProvider ?? (this.serviceProvider = Startup.CreateServiceProvider());
            return serviceProvider.GetRequiredService<T>();
        }
    }
}
