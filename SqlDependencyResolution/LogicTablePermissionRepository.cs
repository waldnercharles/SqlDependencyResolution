using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SqlDependencyResolution
{
    public interface ILogicTablePermissionRepository
    {
        IQueryable<LogicTablePermission> GetLogicTablePermissions();
    }

    public class LogicTablePermissionRepository : ILogicTablePermissionRepository
    {
        private readonly ApplicationContext dbContext;

        public LogicTablePermissionRepository(ApplicationContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IQueryable<LogicTablePermission> GetLogicTablePermissions()
        {
            return this.dbContext.LogicTablePermissions.FromSql("SELECT * FROM (VALUES('CL01','Table',0),('CL01','OtherTable',1),('CL01','AnotherTable',2),('CL01','AndAnotherTable',3)) AS ltp(LogicNK,TableNM,Permissions)");
        }
    }
}
