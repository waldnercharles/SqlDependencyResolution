using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlDependencyResolution
{
    [Flags]
    public enum PermissionsType : byte
    {
        None = 0,
        Read = 1,
        Write = 2
    }

    public class LogicTablePermissions
    {
        [Column("LogicNM")]
        public string LogicName { get; set; }

        [Column("TableNM")]
        public string TableName { get; set; }

        [Column("Permissions")]
        public PermissionsType Permissions { get; set; }
    }
}
