using OpenTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj.blocks
{
    internal class DirtBlock : Block
    {

        public DirtBlock()
        {

            float[] tx = {

                0, 1-0.125f,
                0.125f, 1-0.125f,
                0.125f, 1,
                0.125f, 1,
                0, 1,
                0, 1-0.125f

            };

            SetID(2);
            SetTexCoordinates(reffront, tx);
            SetTexCoordinates(refright, tx);
            SetTexCoordinates(refback, tx);
            SetTexCoordinates(refleft, tx);
            SetTexCoordinates(reftop, tx);
            SetTexCoordinates(refbottom, tx);

        }
    }
}
