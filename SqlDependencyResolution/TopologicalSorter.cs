using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SqlDependencyResolution
{
    public class TopologicalSorter
    {
        private static readonly ReferenceEqualityComparer<Action> actionComparer = new ReferenceEqualityComparer<Action>();
        private static readonly ReferenceEqualityComparer<Task> taskComparer = new ReferenceEqualityComparer<Task>();

        private class NodeRelationship
        {
            public HashSet<Action> Dependents = new HashSet<Action>(actionComparer);
            public HashSet<Action> Dependencies = new HashSet<Action>(actionComparer);
        }

        private class TaskRelationship
        {
            public Task Task;
            public HashSet<Task> TaskDependencies = new HashSet<Task>(taskComparer);
        }

        private readonly Dictionary<Action, NodeRelationship> nodes = new Dictionary<Action, NodeRelationship>(actionComparer);

        public void Add(Action nodeId)
        {
            this.nodes.TryAdd(nodeId, new NodeRelationship());
        }

        public void Add(Action nodeId, Action dependencyId)
        {
            Debug.Assert(!ReferenceEquals(nodeId, dependencyId));

            this.Add(nodeId);
            this.nodes[nodeId].Dependencies.Add(dependencyId);

            this.Add(dependencyId);
            this.nodes[dependencyId].Dependents.Add(nodeId);
        }

        public void Add(Action nodeId, IEnumerable<Action> dependencyIds)
        {
            foreach (Action dependencyId in dependencyIds)
            {
                this.Add(nodeId, dependencyId);
            }
        }

        public Task Sort()
        {
            var nodes = this.nodes.ToDictionary
            (
                kvp => kvp.Key,
                kvp =>  new NodeRelationship { Dependencies = kvp.Value.Dependencies.ToHashSet(actionComparer), Dependents = kvp.Value.Dependents.ToHashSet(actionComparer) },
                actionComparer
            );

            var taskDictionary = this.nodes.ToDictionary
            (
                kvp => kvp.Key,
                kvp => new TaskRelationship(),
                actionComparer
            );

            var sortedNodeIds = new List<Action>();

            // Add root nodes
            sortedNodeIds.AddRange(this.nodes.Where(kvp => kvp.Value.Dependencies.Count == 0).Select(kvp => kvp.Key));
            //foreach (Action nodeId in sortedNodeIds)
            //{
            //    var task = new Task(nodeId);
            //    taskDictionary[nodeId].Task = task;
            //}

            for (var index = 0; index < sortedNodeIds.Count; ++index)
            {
                Action nodeId = sortedNodeIds[index];
                Task task = Task.WhenAll(taskDictionary[nodeId].TaskDependencies).ContinueWith((Task t) => { nodeId.Invoke(); });

                taskDictionary[nodeId].Task = task;

                HashSet<Action> dependentIds = nodes[nodeId].Dependents;
                foreach (Action dependentId in dependentIds)
                {
                    NodeRelationship dependent = nodes[dependentId];

                    taskDictionary[dependentId].TaskDependencies.Add(task);

                    dependent.Dependencies.Remove(sortedNodeIds[index]);
                    if (dependent.Dependencies.Count == 0)
                    {
                        sortedNodeIds.Add(dependentId);
                    }
                }
            }

            return Task.WhenAll(taskDictionary.Values.Select(x => x.Task));
        }
    }
}
