using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                bool loop = true;
                while (loop)
                {
                    var rand = new Random();

                    char keyPressed = Console.ReadLine()[0];

                    Action sleep;

                    switch(keyPressed)
                    {
                        case 's':
                            sleep = () => Thread.Sleep(5000);
                            break;
                        case 'r':
                            sleep = () => Thread.Sleep(rand.Next() % 2500 + 2500);
                            break;
                        case 'q':
                            loop = false;
                            continue;
                        default:
                            continue;
                    }

                    var stopwatch = new Stopwatch();

                    using (ServiceProvider serviceProvider = Startup.CreateServiceProvider())
                    {
                        var dependencyService = serviceProvider.GetService(typeof(IDependencyService)) as IDependencyService;

                        HashSet<LogicRelationship> logicRelationships = await dependencyService.GetLogicRelationships();

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

                                    sleep.Invoke();

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

                        stopwatch.Start();
                        await sorter.Sort();
                        stopwatch.Stop();

                        TimeSpan ts = stopwatch.Elapsed;

                        // Format and display the TimeSpan value.
                        var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                        Console.WriteLine("--------------------");
                        Console.WriteLine("Elapsed: " + elapsedTime);
                        Console.WriteLine("--------------------\r\n");
                    }
                }
            }).Wait();
        }
    }
}
