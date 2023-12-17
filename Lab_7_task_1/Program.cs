using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab_7_task_1
{
    class DistributedSystemNode
    {
        private readonly List<DistributedSystemNode> connectedNodes;
        private bool isActive;

        public string NodeId { get; }

        public DistributedSystemNode(string nodeId)
        {
            NodeId = nodeId;
            connectedNodes = new List<DistributedSystemNode>();
            isActive = true;
        }

        public async Task SendMessageAsync(string message, DistributedSystemNode targetNode)
        {
            await Task.Delay(100);

            Console.WriteLine($"Node {NodeId} sent message: '{message}' to Node {targetNode.NodeId}");
            await targetNode.ReceiveMessageAsync(message, this);
        }

        public async Task ReceiveMessageAsync(string message, DistributedSystemNode senderNode)
        {
            await Task.Delay(100);

            Console.WriteLine($"Node {NodeId} received message: '{message}' from Node {senderNode.NodeId}");
        }

        public void ConnectNode(DistributedSystemNode node)
        {
            connectedNodes.Add(node);
            Console.WriteLine($"Node {NodeId} connected to Node {node.NodeId}");
        }

        public async Task NotifyStatusAsync()
        {
            while (true)
            {
                await Task.Delay(1000);
                
                isActive = !isActive;

                Console.WriteLine($"Node {NodeId} is {(isActive ? "active" : "inactive")}");
                
                foreach (var connectedNode in connectedNodes)
                {
                    await connectedNode.ReceiveStatusChangeAsync(isActive, this);
                }
            }
        }

        public async Task ReceiveStatusChangeAsync(bool newStatus, DistributedSystemNode senderNode)
        {
            await Task.Delay(100);

            Console.WriteLine(
                $"Node {NodeId} received status change: {(newStatus ? "active" : "inactive")} from Node {senderNode.NodeId}");
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            DistributedSystemNode node1 = new DistributedSystemNode("Node1");
            DistributedSystemNode node2 = new DistributedSystemNode("Node2");

            node1.ConnectNode(node2);
            node2.ConnectNode(node1);
            
            var statusTask1 = node1.NotifyStatusAsync();
            var statusTask2 = node2.NotifyStatusAsync();
            
            await node1.SendMessageAsync("Hello!", node2);
            await node2.SendMessageAsync("Hi there!", node1);
            
            await Task.WhenAll(statusTask1, statusTask2);
        }
    }
}