
using System;

namespace FeedForwardWithGeneticAlgorithm
{
    public class Connection
    {

        private Random rand = new Random();

        // The weight of the connection
        public double Weight { get; set; }

        // The innovation number of the connection
        public string Innov { get; set; }

        // The ID of the input perception
        public int InId { get; set; }

        // The ID of the output perception
        public int OutId { get; set; }

        // Whether the connection is enabled or not
        public bool Enabled { get; set; }

        // Constructor for creating a new connection
        public Connection() { }
        public Connection(double weight, string innov, int inId, int outId)
        {
            this.Weight = weight;
            this.Innov = innov;
            this.InId = inId;
            this.OutId = outId;
            this.Enabled = true;
        }
    }
}
