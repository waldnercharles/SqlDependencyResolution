using System;

namespace SqlDependencyResolution
{
    [Flags]
    public enum PermissionsType
    {
        None = 0,
        Read = 1,
        Write = 2
    }

    public class LogicTablePermission
    {
        public string LogicNaturalKey { get; set; }
        public string TableName { get; set; }
        public PermissionsType Permissions { get; set; }
    }
}
