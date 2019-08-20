using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SqlDependencyResolution
{
    public class TopologicalSorter<T> where T : IEquatable<T>
    {
        private class NodeRelationship
        {
            public readonly HashSet<T> Dependents = new HashSet<T>();
            public int DependenciesCount;
        }

        private Dictionary<T, NodeRelationship> nodes = new Dictionary<T, NodeRelationship>();

        public void Add(T nodeId)
        {
            this.nodes.TryAdd(nodeId, new NodeRelationship());
        }

        public void Add(T nodeId, T dependencyId)
        {
            Debug.Assert(!nodeId.Equals(dependencyId));

            this.Add(nodeId);
            this.nodes[nodeId].DependenciesCount += 1;

            this.Add(dependencyId);
            this.nodes[dependencyId].Dependents.Add(nodeId);
        }

        //public void Add(T node, IEnumerable<T> dependencies)
        //{

        //    foreach (var dependency in dependencies)
        //    {
        //        this.Add(node, dependency);
        //    }
        //}

        public IEnumerable<T> Sort()
        {
            var nodes = this.nodes.ToDictionary(kvp => kvp.Key, kvp =>  kvp.Value);
            var sortedNodeIds = new List<T>();

            // Add root nodes
            sortedNodeIds.AddRange(this.nodes.Where(kvp => kvp.Value.DependenciesCount == 0).Select(kvp => kvp.Key));

            foreach (var nodeId in sortedNodeIds)
            {
                var dependentIds = nodes[nodeId].Dependents;
                foreach (var dependentId in dependentIds)
                {
                    Debug.Assert(nodes[dependentId].DependenciesCount > 0);

                    nodes[dependentId].DependenciesCount -= 1;
                    if (nodes[dependentId].DependenciesCount == 0)
                    {
                        sortedNodeIds.Add(dependentId);
                    }
                }
            }

            return sortedNodeIds;
        }
    }
}
