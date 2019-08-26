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
        static private void Help()
        {
            Console.WriteLine();
            Console.WriteLine("Gadget LogicSQL Dependency Resolver (Patent Pending)");
            Console.WriteLine();
            Console.WriteLine("\tGo Go Gadget Batch\t(b)\t Run jobs in batches that take 5s to complete.");
            Console.WriteLine("\tGo Go Gadget Random\t(r)\t Run jobs that take between 1s and 5s to complete.");
            Console.WriteLine("\tGo Go Gadget Graph\t(g)\t Graph the job dependency tree.");
            Console.WriteLine("\tGo Go Gadget Ski Shoes\t(q)\t Run away, defeated by Doctor Claw.");
            Console.WriteLine();
            Console.WriteLine("\tHelp\t\t\t(h)\t Display this message.");
            Console.WriteLine();
        }

        static object writeLock = new object();

        static private void WriteLine(string message, Action action, bool logTime = false)
        {
            int originalRow, originalColumn;
            lock (writeLock)
            {
                Console.Write(message);

                originalRow = Console.CursorTop;
                originalColumn = Console.CursorLeft;

                Console.Write("\r\n");
            }

            var jobStopwatch = new Stopwatch();
            jobStopwatch.Start();
            action.Invoke();
            jobStopwatch.Stop();

            int tempRow, tempColumn;
            lock (writeLock)
            {
                tempRow = Console.CursorTop;
                tempColumn = Console.CursorLeft;

                
                TimeSpan ts = jobStopwatch.Elapsed;
                var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                var timeMessage = logTime ? $" ({elapsedTime})" : "";

                Console.SetCursorPosition(originalColumn, originalRow);
                Console.Write($"Finished{timeMessage}");
                Console.SetCursorPosition(tempColumn, tempRow);
            }
            
        }

        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                bool loop = true;
                while (loop)
                {
                    var rand = new Random();

                    string line = Console.ReadLine().Replace(" ", "").ToLowerInvariant();
                    bool shouldGraph = false;

                    Action sleep;

                    switch (line)
                    {
                        case "b":
                        case "gogogadgetbatch":
                            sleep = () => Thread.Sleep(5000);
                            break;
                        case "r":
                        case "gogogadgetrandom":
                            sleep = () => Thread.Sleep(rand.Next() % 4000 + 1000);
                            break;
                        case "g":
                        case "gogogadgetgraph":
                            sleep = () => { };
                            shouldGraph = true;
                            break;
                        case "q":
                        case "gogogadgetskishoes":
                            loop = false;
                            continue;
                        case "h":
                        case "help":
                            Help();
                            continue;
                        default:
                            Console.WriteLine("Invalid command. Type 'help' to see available commands.");
                            continue;
                    }

                    var stopwatch = new Stopwatch();

                    using (ServiceProvider serviceProvider = Startup.CreateServiceProvider())
                    {
                        HashSet<LogicRelationship> logicRelationships = null;

                        WriteLine("\r\nRetrieving logic read/write permissions from database...", () =>
                        {
                            var dependencyService = serviceProvider.GetService(typeof(IDependencyService)) as IDependencyService;
                            logicRelationships = dependencyService.GetLogicRelationships().GetAwaiter().GetResult();
                        }, true);

                        Console.WriteLine();

                        var nodeDictionary = logicRelationships.Select
                        (
                            l => KeyValuePair.Create<string, Action>
                            (
                                l.LogicNaturalKey,
                                () =>
                                {
                                    WriteLine("Running " + l.LogicNaturalKey + "...", sleep, true);
                                }
                            )
                        ).ToDictionary(kvp => kvp.Key, kvp => new Node() { Name = kvp.Key, Action = kvp.Value });

                        var sorter = new TopologicalSorter();

                        foreach (LogicRelationship logic in logicRelationships)
                        {
                            sorter.Add(nodeDictionary[logic.LogicNaturalKey], logic.LogicNaturalKeyDependencies.Select(dependency => nodeDictionary[dependency]).ToArray());
                        }

                        if (shouldGraph)
                        {
                            sorter.Print();
                        }
                        else
                        {
                            stopwatch.Start();
                            await sorter.Sort();
                            stopwatch.Stop();

                            TimeSpan ts = stopwatch.Elapsed;

                            // Format and display the TimeSpan value.
                            var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                            Console.WriteLine("------------------------");
                            Console.WriteLine("  Elapsed: " + elapsedTime);
                            Console.WriteLine("------------------------\r\n");
                        }
                    }
                }
            }).Wait();
        }
    }
}
