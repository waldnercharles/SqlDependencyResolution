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
                    .GroupBy(l => l.LogicNaturalKey, l => new { l.TableName, l.Permissions })
                    .Select(grp => new { LogicNaturalKey = grp.Key, Tables = grp.Select(t => new { t.TableName, t.Permissions }) });

            foreach (var currentLogic in groupedLogicTablePermissions)
            {
                // Find logics writing to tables that the currentLogic is reading from
                var currentLogicTablesBeingReadFrom = currentLogic.Tables.Where(t => (t.Permissions & PermissionsType.Read) != 0).Select(t => t.TableName).ToArray();

                var currentLogicDependencies =
                    groupedLogicTablePermissions
                        .Where(l => l.LogicNaturalKey != currentLogic.LogicNaturalKey)
                        .Where(l => l.Tables.Where(t => (t.Permissions & PermissionsType.Write) != 0).Select(t => t.TableName).Intersect(currentLogicTablesBeingReadFrom).Any())
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
