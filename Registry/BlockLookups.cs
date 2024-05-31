using System.Collections.Generic;

using Blockgame_OpenTK.BlockUtil;

namespace Blockgame_OpenTK.Registry
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
