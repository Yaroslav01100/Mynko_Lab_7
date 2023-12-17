using System;
using System.Collections.Generic;
using System.Threading;

namespace Lab_7_task_2
{
    class Resource
    {
        public string Name { get; }
        private Semaphore semaphore;
        private Mutex mutex;

        public Resource(string name, int maxConcurrency)
        {
            Name = name;
            semaphore = new Semaphore(maxConcurrency, maxConcurrency);
            mutex = new Mutex();
        }

        public void Acquire()
        {
            semaphore.WaitOne();
            mutex.WaitOne();
        }

        public void Release()
        {
            semaphore.Release();
            mutex.ReleaseMutex();
        }
    }

    class ResourceSimulator
    {
        private static List<Resource> resources = new List<Resource>
        {
            new Resource("CPU", 2),
            new Resource("RAM", 3),
            new Resource("Disk", 1)
        };

        private static void UseResource(string threadName, Resource resource)
        {
            resource.Acquire();
            Console.WriteLine($"{threadName} is using {resource.Name}");
            Thread.Sleep(2000); // Simulate resource usage
            Console.WriteLine($"{threadName} released {resource.Name}");
            resource.Release();
        }

        static void Main(string[] args)
        {
            Thread[] threads = new Thread[5];

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    string threadName = Thread.CurrentThread.Name;
                    Random random = new Random();

                    while (true)
                    {
                        int resourceIndex = random.Next(resources.Count);
                        Resource resource = resources[resourceIndex];

                        UseResource(threadName, resource);
                        
                        Thread.Sleep(random.Next(1000, 3000));
                    }
                });

                threads[i].Name = $"Thread-{i + 1}";
                threads[i].Start();
            }

            Console.ReadLine();
        }
    }
}