﻿using System;
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
            return this.dbContext.LogicTablePermissions.FromSql
            (
                "exec sp_loopthrough"
                //"SELECT * FROM" + Environment.NewLine +
                //"(" + Environment.NewLine +
                //"   VALUES" + Environment.NewLine +
                //    string.Join
                //    (
                //        "," + Environment.NewLine,
                //        new[]
                //        {
                //            "   ('CL1','Table A', 'Banana', 3)",
                //            "   ('CL1','Table B', 'Banana', 2)",
                //            "   ('CL2','Table A', 'Banana', 2)",
                //            "   ('CL3','Table B', 'Banana', 3)",
                //            "   ('CL4','Table B', 'Banana', 2)",
                //            "   ('CL5','Table C', 'Banana', 2)",
                //            "   ('CL6','Table C', 'Banana', 3)",
                //            "   ('CL3','Table A', 'Banana2', 2)", // This line creates a cycle between CL1 and CL3.
                //                                                  // CL3 needs to read from B after CL1 writes to it, and CL1 needs to read from A after CL3 writes to it.
                //        }
                //    ) + Environment.NewLine +
                //") AS ltp(LogicNK, TableNM, ColumnNM, PermissionFLG)"
            );
        }
    }
}
