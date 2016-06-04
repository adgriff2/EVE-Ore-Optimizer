using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVE_Ore_Optimizer
{
    class ManufacturableItem
    {
        public long typeID;
        public string name;
        public Dictionary<long, long> mats;

        public ManufacturableItem (long _typeID, string _name, Dictionary<long, long> _mats)
        {
            typeID = _typeID;
            name = _name;
            mats = _mats;
        }
    }
}
