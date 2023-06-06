using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace opentk_proj
{
    internal class GrassBlock : Block
    {
        public GrassBlock()
        {

            float tx = 0.125f;

            float[] texside =
            {

                tx, 1-tx,
                tx*2, 1-tx,
                tx*2, 1,
                tx*2, 1,
                tx, 1,
                tx, 1-tx

            };
            float[] texbottom =
            {

                0, 1-tx,
                tx, 1-tx,
                tx, 1,
                tx, 1,
                0, 1,
                0, 1-tx

            };
            float[] textop =
            {

                tx*2, 1-tx,
                tx*3, 1-tx,
                tx*3, 1,
                tx*3, 1,
                tx*2, 1,
                tx*2, 1-tx

            };

            SetBlockType(1);
            SetTexCoordinates(reffront, texside);
            SetTexCoordinates(refright, texside);
            SetTexCoordinates(refback, texside);
            SetTexCoordinates(refleft, texside);
            SetTexCoordinates(refbottom, texbottom);
            SetTexCoordinates(reftop, textop);


        }

    }
}
