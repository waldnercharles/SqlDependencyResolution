using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SqlDependencyResolution
{
    public class LogicRelationship
    {
        public string LogicNaturalKey { get; set; }
        public HashSet<string> LogicNaturalKeyDependencies { get; set; }
    }

    public interface IDependencyService
    {
        Task<HashSet<LogicRelationship>> GetLogicRelationships();
    }

    public class DependencyService : IDependencyService
    {
        public ILogicTablePermissionRepository logicTablePermissionRepository;

        public DependencyService(ILogicTablePermissionRepository logicTablePermissionRepository)
        {
            this.logicTablePermissionRepository = logicTablePermissionRepository;
        }

        public async Task<HashSet<LogicRelationship>> GetLogicRelationships()
        {
            var logicRelationships = new HashSet<LogicRelationship>();
            var logicTablePermissions = await this.logicTablePermissionRepository.GetLogicTablePermissions().ToArrayAsync();

            var groupedLogicTablePermissions =
                logicTablePermissions
                    .Distinct()
                    .GroupBy(l => l.LogicNaturalKey, l => new { l.TableName, l.ColumnName, l.Permissions })
                    .Select(grp => new
                    {
                        LogicNaturalKey = grp.Key,
                        Tables = grp.GroupBy(g => g.TableName, g => new { g.ColumnName, g.Permissions })
                                    .Select(t => new
                                    {
                                        TableName = t.Key,
                                        Columns = t.Select(c => new { c.ColumnName, c.Permissions }).ToArray()
                                    })
                    });

            foreach (var currentLogic in groupedLogicTablePermissions)
            {
                // Find logics writing to tables that the currentLogic is reading from
                var currentLogicColumnsBeingReadFrom =
                    currentLogic.Tables.SelectMany(t => t.Columns.Where(c => (c.Permissions & PermissionsType.Read) != 0).Select(c => $"{t.TableName}.{c.ColumnName}")).ToArray();

                var currentLogicDependencies =
                    groupedLogicTablePermissions
                        .Where(l => l.LogicNaturalKey != currentLogic.LogicNaturalKey)
                        .Where
                        (l => 
                            l.Tables.SelectMany(t => t.Columns.Where(c => !string.IsNullOrWhiteSpace(c.ColumnName) && ((c.Permissions & PermissionsType.Write) != 0)).Select(c => $"{t.TableName}.{c.ColumnName}")).Intersect(currentLogicColumnsBeingReadFrom).Any() ||
                            l.Tables.Where(t => t.Columns.Any(c => string.IsNullOrWhiteSpace(c.ColumnName) && ((c.Permissions & PermissionsType.Write) != 0))).Any(t => currentLogicColumnsBeingReadFrom.Any(r => r.StartsWith(t.TableName)))
                        )
                        .Select(l => l.LogicNaturalKey);


                logicRelationships.Add(new LogicRelationship()
                {
                    LogicNaturalKey = currentLogic.LogicNaturalKey,
                    LogicNaturalKeyDependencies = new HashSet<string>(currentLogicDependencies)
                });
            }

            return logicRelationships;
        }
    }
}
