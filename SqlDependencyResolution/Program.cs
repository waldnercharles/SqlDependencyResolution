using System;

namespace SqlDependencyResolution
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var serviceProvider = Startup.CreateServiceProvider())
            {

            }

            Console.WriteLine("Hello World!");
        }
    }
}
