using System;
using System.Collections.Generic;
using System.Threading;

namespace Lab_7_task_3
{
    class Operation
    {
        public int ThreadId { get; }
        public DateTime Timestamp { get; }
        public string Description { get; }

        public Operation(int threadId, string description)
        {
            ThreadId = threadId;
            Timestamp = DateTime.Now;
            Description = description;
        }
    }

    class ConflictLogger
    {
        private readonly List<Operation> operationLog = new List<Operation>();
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public void LogOperation(int threadId, string description)
        {
            semaphore.Wait();

            try
            {
                Operation operation = new Operation(threadId, description);
                operationLog.Add(operation);
                Console.WriteLine(
                    $"Operation logged: {operation.Timestamp} - Thread {operation.ThreadId}: {operation.Description}");
                
                Thread.Sleep(1000);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public void ResolveConflicts()
        {
            semaphore.Wait();

            try
            {
                Console.WriteLine("Checking for conflicts...");
                
                for (int i = 0; i < operationLog.Count - 1; i++)
                {
                    for (int j = i + 1; j < operationLog.Count; j++)
                    {
                        if (operationLog[i].Timestamp == operationLog[j].Timestamp)
                        {
                            Console.WriteLine(
                                $"Conflict detected between Thread {operationLog[i].ThreadId} and Thread {operationLog[j].ThreadId}");
                        }
                    }
                }

                Console.WriteLine("Conflicts resolved.");
            }
            finally
            {
                semaphore.Release();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ConflictLogger conflictLogger = new ConflictLogger();

            Thread thread1 = new Thread(() =>
            {
                while (true)
                {
                    conflictLogger.LogOperation(1, "Operation A");
                }
            });

            Thread thread2 = new Thread(() =>
            {
                while (true)
                {
                    conflictLogger.LogOperation(2, "Operation B");
                }
            });

            Thread resolutionThread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(5000);
                    conflictLogger.ResolveConflicts();
                }
            });

            thread1.Start();
            thread2.Start();
            resolutionThread.Start();

            Console.ReadLine();
        }
    }
}