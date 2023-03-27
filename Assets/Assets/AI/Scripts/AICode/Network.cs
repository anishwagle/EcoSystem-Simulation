using System;
using System.Collections.Generic;
using System.Linq;

namespace FeedForwardWithGeneticAlgorithm
{
 
   
    public class NeuralNetwork
    {
        // A list of all the connections in the network.
        public List<Connection> Connections { get; set; }

        // A list of all the perceptions in the network.
        public List<Perception> Perceptions { get; set; }
        public int Generation =0;
        public Guid Id { get; set; }
        public double Fitness { get; set; }
        private Random random = new Random();
        public int inputLength;
        public int outputLength;

        // Initializes a new instance of the NeuralNetwork class.
        public NeuralNetwork() {
            Perceptions = new List<Perception>();
            Connections = new List<Connection>();
            Id= Guid.NewGuid();
        }
        public NeuralNetwork(int inputCount, int outputCount)
        {
            Perceptions = new List<Perception>();
            Connections = new List<Connection>();
            Id= Guid.NewGuid();
            inputLength = inputCount;
            outputLength = outputCount;

            // add input perceptions
            for (int i = 0; i < inputCount; i++)
            {
                Perception p = new Perception(i,  PerceptionType.Input);
                Perceptions.Add(p);
            }

            // add output perceptions
            for (int i = 0; i < outputCount; i++)
            {
                Perception p = new Perception(i + inputCount, PerceptionType.Output);
                Perceptions.Add(p);
            }

            for(var i = 0; i < inputCount; i++)
            {
                for(var j = inputCount; j < inputCount + outputCount; j++)
                {
                    Connections.Add(new Connection((random.NextDouble() * 2) - 1, $"{i}_{j}", i, j));
                }
            }


        }
        public void AddConnection(int inId, int outId, double weight = -1)
        {
            if(inId == outId)
            {
                return;
            }
            TopologicalSort();
            var iOrder = Perceptions.Find(x => x.Id == inId)?.Order;
            var oOrder = Perceptions.Find(x => x.Id == outId)?.Order;
            if (oOrder.Value < iOrder.Value)
            {
                var tem = inId;
                inId = outId;
                outId = tem;
            }
           
            if (Connections.Any(x => x.Innov == $"{inId}_{outId}"))
            {
                return;
            }

            if (weight == -1)
            {
                // assign random weight if not provided
                weight = (random.NextDouble() * 2) - 1;
            }
                  
            Connection c = new Connection(weight, $"{inId}_{outId}", inId, outId); ;

            Connections.Add(c);
        }

        public void RemoveConnection(string innov)
        {
            Connections = Connections.Where(x => x.Innov != innov).ToList();
            RemoveNodeWithNoConnection();
        }

        private void RemoveNodeWithNoConnection()
        {
            bool loopAgain = false;
            var per = Perceptions.Where(x => x.Type == PerceptionType.Hidden && !(Connections.Any(y => y.InId == x.Id ) && Connections.Any(y => y.OutId == x.Id)));
            foreach(var c in per)
            {
                Perceptions = Perceptions.Where(x => x.Id != c.Id).ToList();
                Connections = Connections.Where(x => x.InId != c.Id).ToList();
                Connections = Connections.Where(x => x.OutId != c.Id).ToList();
                loopAgain = true;
            }
            if (loopAgain)
            {
                RemoveNodeWithNoConnection();
            }
        }

        public void AddPerception(string innov)
        {
            var connection = Connections.First(x => x.Innov == innov);
            connection.Enabled = false;
            var newNode = new Perception(GetNewHiddenName(),  PerceptionType.Hidden);
            Perceptions.Add(newNode);

            Connections.Add(new Connection(1, $"{connection.InId}_{newNode.Id}", connection.InId, newNode.Id));
            Connections.Add(new Connection((random.NextDouble() * 2) - 1, $"{newNode.Id}_{connection.OutId}", newNode.Id, connection.OutId));
        }

        public List<double> Calculate(List<double>input)
        {
            TopologicalSort();
            Perceptions=Perceptions.OrderBy(x => x.Order).ToList();
            for (var i = 0;i< inputLength; i++)
            {
                Perceptions[i].Output = input[i];
            }

            for(var i = inputLength; i< Perceptions.Count(); i++)
            {
                double sum = 0;
                var connections = Connections.Where(y => y.OutId == Perceptions[i].Id);

                foreach (var connection in connections)
                {
                    if (connection.Enabled)
                    {
                        sum += Perceptions.First(x => x.Id == connection.InId).Output * connection.Weight;
                    }
                }
                Perceptions[i].Output = ActivationSigmoid( sum + Perceptions[i].Bias) ;
            }

            var result = Perceptions.Where(x => x.Type == PerceptionType.Output).Select(x =>x.Output ).ToList();
            return result;
        }

        double ActivationRelu(double x)
        {
            return x > 0 ? x : 0.01f * x;
        }

        double ActivationSigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }
        public void Mutation()
        {
            var condition = 0;
            if (Connections.Count() != 0)
            {
                condition = random.Next(0, 3);
            }

            switch (condition)
            {
                case 0:
                    var inputs = Perceptions.Where(x => x.Type == PerceptionType.Input || x.Type == PerceptionType.Hidden).ToList();
                    var outputs = Perceptions.Where(x => x.Type == PerceptionType.Output || x.Type == PerceptionType.Hidden).ToList();
                    var inputIndex = random.Next(0, inputs.Count());
                    var outputIndex = random.Next(0, outputs.Count());

                    AddConnection(inputs[inputIndex].Id, outputs[outputIndex].Id);
                    break;

                case 1:

                    RemoveConnection(Connections[random.Next(0, Connections.Count)].Innov);
                    break;
                case 2:
                    AddPerception(Connections[random.Next(0, Connections.Count)].Innov);
                    break;

            }
        }
        
        public void RemoveConnectWithNoPerception()
        {
            Connections = Connections.Where(x => Perceptions.Any(y => y.Id == x.InId) && Perceptions.Any(y => y.Id == x.OutId)).ToList();

        }
        //what is order of node between parents are different?? node 2 can have higher order in p1 and lower in p2
        //resuting in false nn connection
        public NeuralNetwork CrossOver( NeuralNetwork n2)
        {
            var child = new NeuralNetwork
            {
                inputLength = inputLength,
                outputLength = outputLength
            };

            for (var i=0; i<inputLength + outputLength; i++)
            {
                if (random.Next(0, 2) == 1)
                {
                    var per = Perceptions.First(x => x.Id == i);
                    child.Perceptions.Add(new Perception() { Id = per.Id, Bias = per.Bias,Type = per.Type});

                }
                else
                {
                    var per = n2.Perceptions.First(x => x.Id == i);
                    child.Perceptions.Add(new Perception() { Id = per.Id, Bias = per.Bias, Type = per.Type });
                }
            }

        

            var inFirst = Connections.Where(x => !n2.Connections.Any(y => y.Innov == x.Innov || y.Innov == $"{x.OutId}_{x.InId}"));
            var inSecond = n2.Connections.Where(x => !Connections.Any(y => y.Innov == x.Innov || y.Innov == $"{x.OutId}_{x.InId}"));
            var inBoth = n2.Connections.Where(x => Connections.Any(y => y.Innov == x.Innov || y.Innov == $"{x.OutId}_{x.InId}"));
            foreach (var f in inFirst)
            {
                if(!child.Connections.Any(y => y.Innov == f.Innov || y.Innov == $"{f.OutId}_{f.InId}"))
                {
                    child.Connections.Add(new Connection() { Enabled=f.Enabled,InId=f.InId,Innov=f.Innov,OutId=f.OutId,Weight=f.Weight});

                }
            }
            foreach (var f in inSecond)
            {
                if (!child.Connections.Any(y => y.Innov == f.Innov || y.Innov == $"{f.OutId}_{f.InId}"))
                {
                    child.Connections.Add(new Connection() { Enabled = f.Enabled, InId = f.InId, Innov = f.Innov, OutId = f.OutId, Weight = f.Weight });

                }
            }
            foreach (var b in inBoth)
            {
                if (random.Next(0, 2) == 1)
                {
                    var f = Connections.First(x => x.Innov == b.Innov || x.Innov == $"{b.OutId}_{b.InId}");
                    if (!child.Connections.Any(y => y.Innov == f.Innov || y.Innov == $"{f.OutId}_{f.InId}"))
                    {
                        child.Connections.Add(new Connection() { Enabled = f.Enabled, InId = f.InId, Innov = f.Innov, OutId = f.OutId, Weight = f.Weight });
                    }
                }
                else
                {
                    var f = n2.Connections.First(x => x.Innov == b.Innov || x.Innov == $"{b.OutId}_{b.InId}");
                    if (!child.Connections.Any(y => y.Innov == f.Innov || y.Innov == $"{f.OutId}_{f.InId}"))
                    {
                        child.Connections.Add(new Connection() { Enabled = f.Enabled, InId = f.InId, Innov = f.Innov, OutId = f.OutId, Weight = f.Weight });
                    }
                }
            }
            

            foreach (var c in child.Connections)
            {
                var c1In = Perceptions.FirstOrDefault(x => x.Id == c.InId);
                var c2In = n2.Perceptions.FirstOrDefault(x => x.Id == c.InId);
                if (c1In == null && c2In != null)
                {
                    if (!child.Perceptions.Any(x => x.Id == c2In.Id))
                    {
                        child.Perceptions.Add(new Perception() { Id = c2In.Id, Bias = c2In.Bias, Type = c2In.Type });
                    }
                }
                else if (c1In != null && c2In == null)
                {
                    if (!child.Perceptions.Any(x => x.Id == c1In.Id))
                    {
                        child.Perceptions.Add(new Perception() { Id = c1In.Id, Bias = c1In.Bias, Type = c1In.Type });
                    }
                }
                else if (c1In != null && c2In != null)
                {
                    if (random.Next(0, 2) == 1)
                    {
                        if (!child.Perceptions.Any(x => x.Id == c2In.Id))
                            child.Perceptions.Add(new Perception() { Id = c2In.Id, Bias = c2In.Bias, Type = c2In.Type });
                    }
                    else
                    {
                        if (!child.Perceptions.Any(x => x.Id == c1In.Id))
                            child.Perceptions.Add(new Perception() { Id = c1In.Id, Bias = c1In.Bias, Type = c1In.Type });
                    }
                }
            }
            return child;

        }

        private int GetNewHiddenName()
        {
            if(Perceptions.Count(x=>x.Type==PerceptionType.Hidden) == 0)
            {
                return Perceptions.Count();
            }
            else
            {
                var name = 0;
                foreach(var c in Perceptions)
                {
                    if (c.Id > name)
                    {
                        name = c.Id;
                    }
                }
                return name+1;
            }
        }

        public void TopologicalSort()
        {
            RemoveConnectWithNoPerception();

            // Create a dictionary to store the in-degree of each node
            var inDegree = new Dictionary<int, int>();
            foreach (var node in Perceptions)
            {
                inDegree[node.Id] = 0;
            }

            // Create a dictionary to store the adjacency list of each node
            var adjacencyList = new Dictionary<int, List<int>>();
            foreach (var connection in Connections)
            {
                if (!adjacencyList.ContainsKey(connection.InId))
                {
                    adjacencyList[connection.InId] = new List<int>();
                }
                adjacencyList[connection.InId].Add(connection.OutId);

                // Increment the in-degree of the output node
                inDegree[connection.OutId]++;
            }

            // Create a queue to store the nodes with in-degree 0
            var queue = new Queue<Perception>();
            foreach (var node in Perceptions)
            {
                if (inDegree[node.Id] == 0)
                {
                    queue.Enqueue(node);
                }
            }

            // Initialize the sorted list
            var sorted = new List<Perception>();
            var order = 0;
            // Process the nodes in the queue
            while (queue.Count > 0)
            {
                // Get the next node in the queue
                var node = queue.Dequeue();
                Perceptions.First(x=>x.Id==node.Id).Order =  order++;

                sorted.Add(node);

                // Decrement the in-degree of the output nodes
                if (adjacencyList.ContainsKey(node.Id))
                {
                    foreach (var outputNodeId in adjacencyList[node.Id])
                    {
                        inDegree[outputNodeId]--;

                        // If the in-degree of the output node becomes 0, add it to the queue
                        if (inDegree[outputNodeId] == 0)
                        {
                            queue.Enqueue(Perceptions.Find(n => n.Id == outputNodeId));
                        }
                    }
                }
            }

           
        }
    }
}


