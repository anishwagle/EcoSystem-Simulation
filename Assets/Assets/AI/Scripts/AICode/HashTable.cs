
using System.Collections.Generic;

namespace FeedForwardWithGeneticAlgorithm
{
    public class HashTable
    {
        readonly IDictionary<int, int> Items;
        public HashTable()
        {
            Items = new Dictionary<int, int>();

        }
        public void AddToDic(int id, int output)
        {
            Items[id]=output;
        }

        public int? GetOutputForId(int id)
        {
            int? output= null;
            if (Items.ContainsKey(id))
            {
                output = Items[id];
            }
            return output;
        }
    }
}
