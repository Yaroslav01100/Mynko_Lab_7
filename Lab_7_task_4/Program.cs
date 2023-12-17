using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lab_7_task_4
{
    public class LamportEvent
    {
        public int NodeId { get; }
        public int SequenceNumber { get; }

        public LamportEvent(int nodeId, int sequenceNumber)
        {
            NodeId = nodeId;
            SequenceNumber = sequenceNumber;
        }
    }

    public class LamportClock
    {
        private int nodeId;
        private int sequenceNumber;

        public LamportClock(int nodeId)
        {
            this.nodeId = nodeId;
            this.sequenceNumber = 0;
        }

        public LamportEvent GetNextEvent()
        {
            Interlocked.Increment(ref sequenceNumber);
            return new LamportEvent(nodeId, sequenceNumber);
        }

        public void Update(LamportEvent receivedEvent)
        {
            sequenceNumber = Math.Max(sequenceNumber, receivedEvent.SequenceNumber) + 1;
        }
    }

    public class EventSystem
    {
        private readonly ConcurrentDictionary<string, Action<LamportEvent>> eventSubscribers =
            new ConcurrentDictionary<string, Action<LamportEvent>>();

        private readonly Dictionary<int, LamportClock> nodeClocks = new Dictionary<int, LamportClock>();
        private readonly object lockObject = new object();

        public void Subscribe(string eventName, Action<LamportEvent> callback)
        {
            eventSubscribers.AddOrUpdate(eventName, callback, (_, existingCallback) => existingCallback + callback);
        }

        public void Unsubscribe(string eventName, Action<LamportEvent> callback)
        {
            eventSubscribers.AddOrUpdate(eventName, callback, (_, existingCallback) => existingCallback - callback);
        }

        public void Publish(string eventName, int nodeId)
        {
            LamportClock clock;
            lock (lockObject)
            {
                if (!nodeClocks.TryGetValue(nodeId, out clock))
                {
                    clock = new LamportClock(nodeId);
                    nodeClocks[nodeId] = clock;
                }
            }

            LamportEvent newEvent = clock.GetNextEvent();

            if (eventSubscribers.TryGetValue(eventName, out var callback))
            {
                callback(newEvent);
            }
        }

        public void ReceiveEvent(LamportEvent receivedEvent)
        {
            lock (lockObject)
            {
                if (nodeClocks.TryGetValue(receivedEvent.NodeId, out var clock))
                {
                    clock.Update(receivedEvent);
                }
                else
                {
                    nodeClocks[receivedEvent.NodeId] = new LamportClock(receivedEvent.NodeId);
                }
            }
        }
    }

    public class Node
    {
        private readonly int nodeId;
        private readonly EventSystem eventSystem;

        public Node(int nodeId, EventSystem eventSystem)
        {
            this.nodeId = nodeId;
            this.eventSystem = eventSystem;
        }

        public void Start()
        {
            Task.Run(() => ListenForEvents());
        }

        private void ListenForEvents()
        {
            Random random = new Random();

            while (true)
            {
                Thread.Sleep(random.Next(1000, 3000));
                string eventName = $"Event-{random.Next(1, 5)}";
                eventSystem.Publish(eventName, nodeId);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            EventSystem eventSystem = new EventSystem();

            Node node1 = new Node(1, eventSystem);
            Node node2 = new Node(2, eventSystem);
            node1.Start();
            node2.Start();
            
            eventSystem.Subscribe("Event-1",
                e => Console.WriteLine(
                    $"Received Event-1 from Node {e.NodeId} with sequence number {e.SequenceNumber}"));
            eventSystem.Subscribe("Event-2",
                e => Console.WriteLine(
                    $"Received Event-2 from Node {e.NodeId} with sequence number {e.SequenceNumber}"));
            eventSystem.Subscribe("Event-3",
                e => Console.WriteLine(
                    $"Received Event-3 from Node {e.NodeId} with sequence number {e.SequenceNumber}"));
            eventSystem.Subscribe("Event-4",
                e => Console.WriteLine(
                    $"Received Event-4 from Node {e.NodeId} with sequence number {e.SequenceNumber}"));

            Console.ReadLine();
        }
    }
}