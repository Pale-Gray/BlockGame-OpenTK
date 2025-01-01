using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.BlockProperty
{
    public interface IBlockProperties
    {

        public abstract byte[] ToBytes();

        public abstract IBlockProperties FromBytes();

    }
}
