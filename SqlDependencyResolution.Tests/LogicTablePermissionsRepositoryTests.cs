using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlDependencyResolution
{
    [TestClass]
    public class LogicTablePermissionsRepositoryTests : TestBase
    {
        private ILogicTablePermissionRepository target;

        [TestInitialize]
        public void TestInitialize()
        {
            this.target = this.GetService<ILogicTablePermissionRepository>();
        }

        [TestMethod]
        public async Task GetLogicTablePermissions()
        {
            // Act
            var actual = await this.target.GetLogicTablePermissions().ToArrayAsync();
        }
    }
}
