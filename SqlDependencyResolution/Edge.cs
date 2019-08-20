using System;
using System.Collections.Generic;
using System.Text;

namespace SqlDependencyResolution
{
    public class Edge
    {
        public int Id { get; set; }

        public int? DependsOn { get; set; }
    }
}
