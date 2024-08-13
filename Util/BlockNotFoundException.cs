using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Util
{
    internal class BlockNotFoundException : Exception
    {

        public BlockNotFoundException() { }

        public BlockNotFoundException(string message) : base(message) { }

        public BlockNotFoundException(string message, Exception innerException) : base(message, innerException) { }

    }
}
