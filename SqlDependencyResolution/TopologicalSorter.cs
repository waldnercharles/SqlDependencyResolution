using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SqlDependencyResolution
{
    public class Node
    {
        public string Name { get; set; }
        public Action Action { get; set; }
    }

    public class TopologicalSorter
    {
        private static readonly ReferenceEqualityComparer<Node> actionComparer = new ReferenceEqualityComparer<Node>();
        private static readonly ReferenceEqualityComparer<Task> taskComparer = new ReferenceEqualityComparer<Task>();

        private class NodeRelationship
        {
            public HashSet<Node> Dependents = new HashSet<Node>(actionComparer);
            public HashSet<Node> Dependencies = new HashSet<Node>(actionComparer);
        }

        private class TaskRelationship
        {
            public Task Task;
            public HashSet<Task> TaskDependencies = new HashSet<Task>(taskComparer);
        }

        private readonly Dictionary<Node, NodeRelationship> nodes = new Dictionary<Node, NodeRelationship>(actionComparer);

        public void Add(Node nodeId)
        {
            this.nodes.TryAdd(nodeId, new NodeRelationship());
        }

        public void Add(Node nodeId, Node dependencyId)
        {
            Debug.Assert(!ReferenceEquals(nodeId, dependencyId));

            this.Add(nodeId);
            this.nodes[nodeId].Dependencies.Add(dependencyId);

            this.Add(dependencyId);
            this.nodes[dependencyId].Dependents.Add(nodeId);
        }

        public void Add(Node nodeId, IEnumerable<Node> dependencyIds)
        {
            foreach (Node dependencyId in dependencyIds)
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

            var sortedNodeIds = new List<Node>();

            // Add root nodes
            sortedNodeIds.AddRange(this.nodes.Where(kvp => kvp.Value.Dependencies.Count == 0).Select(kvp => kvp.Key));

            var rootTasks = new List<Task>();

            foreach (Node nodeId in sortedNodeIds)
            {
                var task = new Task(nodeId.Action);
                taskDictionary[nodeId].Task = task;

                rootTasks.Add(task);
            }

            for (var index = 0; index < sortedNodeIds.Count; ++index)
            {
                Node nodeId = sortedNodeIds[index];
                Task task = taskDictionary[nodeId].Task ?? Task.WhenAll(taskDictionary[nodeId].TaskDependencies).ContinueWith((Task t) => { nodeId.Action.Invoke(); });

                taskDictionary[nodeId].Task = task;

                HashSet<Node> dependentIds = nodes[nodeId].Dependents;
                foreach (Node dependentId in dependentIds)
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

            var cycledNodes = nodes.Where(kvp => kvp.Value.Dependencies.Count != 0).ToArray();
            if (cycledNodes.Length > 0)
            {
                return Task.Run(() => Console.WriteLine("Failed to run. Cycle detected with the following nodes (" + string.Join(", ", cycledNodes.Select(kvp => kvp.Key.Name)) + ")"));
            }
            else
            {
                foreach (Task task in rootTasks)
                {
                    task.Start();
                }

                return Task.WhenAll(taskDictionary.Values.Select(x => x.Task)).ContinueWith((Task t) => Console.WriteLine("Complete!"));
            }

        }
    }
}
