using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj.blocks
{
    internal class StoneBlock : Block
    {

        public StoneBlock()
        {

            float[] tx = {

                0.125f*3, 1-0.125f,
                0.125f*4, 1-0.125f,
                0.125f*4, 1,
                0.125f*4, 1,
                0.125f*3, 1,
                0.125f*3, 1-0.125f,
            };

            SetID(3);
            SetTexCoordinates(reffront, tx);
            SetTexCoordinates(refright, tx);
            SetTexCoordinates(refback, tx);
            SetTexCoordinates(refleft, tx);
            SetTexCoordinates(reftop, tx);
            SetTexCoordinates(refbottom, tx);

        }



    }
}
