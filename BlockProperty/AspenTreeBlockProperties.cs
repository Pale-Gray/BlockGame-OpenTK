using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.BlockProperty
{

    public enum BranchDirection
    {

        Up,
        Down,
        Left,
        Right,
        Forward,
        Backward

    }
    public class AspenTreeBlockProperties : IBlockProperties
    {

        public bool IsGrowing = true;
        public int Thickness = 16;
        public BranchDirection GrowingDirection = BranchDirection.Up;
        public int MaxHeight = new Random().Next(15, 19);
        public int StumpHeight = (int)(new Random().Next(15, 19) / 2.0) + new Random().Next(-2, 2);
        public int BranchHeight = (int)(new Random().Next(15, 19) / 2.0) + new Random().Next(5, 9);

        public int BranchLength = 0;

        public bool CanConnectUpwards = true;
        public bool CanConnectDownwards = true;
        public bool CanConnectLeft = false;
        public bool CanConnectRight = false;
        public bool CanConnectForwards = false;
        public bool CanConnectBackwards = false;

        public void ToBytes(DataWriter writer)
        {

            

        }

        public IBlockProperties FromBytes(DataReader reader)
        {

            AspenTreeBlockProperties prop = new AspenTreeBlockProperties();

            // somewhere in here, deserialize the bits and set them in the prop fields

            return prop;
            
        }

    }
}
