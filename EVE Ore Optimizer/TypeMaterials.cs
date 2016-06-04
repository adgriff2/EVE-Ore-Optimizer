using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVE_Ore_Optimizer
{
    class TypeMaterials
    {
        public long typeID;
        public long materialTypeID;
        public long quantity;

        public TypeMaterials(long _typeID,long _materialTypeID, long _quantity)
        {
            typeID = _typeID;
            materialTypeID = _materialTypeID;
            quantity = _quantity;
        }
    }
}
