using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Util
{
    public class BlockNotFoundException : Exception
    {

        public BlockNotFoundException() { }

        public BlockNotFoundException(string message) : base(message) { }

        public BlockNotFoundException(string message, Exception innerException) : base(message, innerException) { }

    }
}
