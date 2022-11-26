using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Inventory
    {
        public Dictionary<int, Item> _items = new Dictionary<int, Item>();

        public void Add(Item item)
        {
            _items.Add(item.ItemDbId, item);
        }

        public Item Get(int itemDbId)
        {
            Item item = null;
            _items.TryGetValue(itemDbId, out item);
            return item;
        }

        public Item Find(Func<Item, bool> condition)
        {
            foreach(Item item in _items.Values)
            {
                if (condition.Invoke(item))
                    return item;
            }
            return null;
        }
    }
}
