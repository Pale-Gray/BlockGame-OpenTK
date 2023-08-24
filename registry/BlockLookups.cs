using opentk_proj.block;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj.registry
{

    public enum LookupType
    {

        Default,
        Additional

    };
    internal class BlockLookups
    {

        public static Dictionary<int, Block> DefaultLookup = new Dictionary<int, Block>();

    }
}
