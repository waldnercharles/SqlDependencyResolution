using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlDependencyResolution
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                using (ServiceProvider serviceProvider = Startup.CreateServiceProvider())
                {
                    var dependencyService = serviceProvider.GetService(typeof(IDependencyService)) as IDependencyService;

                    HashSet<LogicRelationship> logicRelationships = await dependencyService.GetLogicRelationships();

                    var rand = new Random();
                    var writeLock = new object();

                    Console.WriteLine();

                    var nodeDictionary = logicRelationships.Select
                    (
                        l => KeyValuePair.Create<string, Action>
                        (
                            l.LogicNaturalKey,
                            () =>
                            {
                                int originalRow, originalColumn;
                                lock (writeLock)
                                {
                                    Console.Write("Running " + l.LogicNaturalKey + "...");

                                    originalRow = Console.CursorTop;
                                    originalColumn = Console.CursorLeft;

                                    Console.Write("\r\n");
                                }

                                //Thread.Sleep((rand.Next() % 2500) + 2500);
                                Thread.Sleep(10000);

                                int tempRow, tempColumn;
                                lock (writeLock)
                                {
                                    tempRow = Console.CursorTop;
                                    tempColumn = Console.CursorLeft;

                                    Console.SetCursorPosition(originalColumn, originalRow);
                                    Console.Write("Finished");
                                    Console.SetCursorPosition(tempColumn, tempRow);
                                }

                            }
                        )
                    ).ToDictionary(kvp => kvp.Key, kvp => new Node() { Name = kvp.Key, Action = kvp.Value });

                    var sorter = new TopologicalSorter();

                    foreach (LogicRelationship logic in logicRelationships)
                    {
                        sorter.Add(nodeDictionary[logic.LogicNaturalKey], logic.LogicNaturalKeyDependencies.Select(dependency => nodeDictionary[dependency]).ToArray());
                    }

                    await sorter.Sort();
                }
            }).Wait();

            Console.ReadKey();
        }
    }
}
