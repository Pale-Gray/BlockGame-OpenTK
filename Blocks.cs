using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj
{
    internal class Blocks : Block
    {

        public static Block Air = new Block();
        public static GrassBlock Grass = new GrassBlock();

        public static Block GetBlockByID(int id)
        {

            switch(id)
            {

                case 0:
                    return Air;
                    break;
                case 1:
                    return Grass;
                    break;
                default:
                    return Air;
                    break;

            }

        } 

    }
}
