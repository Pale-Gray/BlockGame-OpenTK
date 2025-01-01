using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.BlockProperty
{
    internal class NewProperties : IBlockProperties
    {
        public byte[] ToBytes()
        {
            Console.WriteLine("ToBytes from NewProperties is being called.");

            return Array.Empty<byte>();
        }

        public IBlockProperties FromBytes()
        {

            return new NewProperties();

        }
    }
}
