using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Game.BlockProperty
{
    public interface IBlockProperties
    {

        public abstract void ToBytes(DataWriter writer);

        public abstract IBlockProperties FromBytes(DataReader reader);

    }
}
