using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trees
{
    public class Service
    {
        private readonly Collection<Items> itemList;

        public IEnumerable<Items> ItemList
        {
            get { return itemList; }
        }

        public Service(string path)
        {
            itemList = new Collection<Items> { new Items(path, true) };
        }
    }
}
