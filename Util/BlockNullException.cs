using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Util
{
    public class BlockNullException : Exception
    {

        public BlockNullException() { }

        public BlockNullException(string message) : base(message) { }

        public BlockNullException(string message, Exception innerException) : base(message, innerException) { }

    }
}
