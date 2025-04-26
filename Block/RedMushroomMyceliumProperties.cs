using System.Collections.Generic;
using Game.Core.BlockStorage;
using Game.Util;
using OpenTK.Mathematics;

namespace Game.BlockUtil;

public class RedMushroomMyceliumProperties : IBlockProperties
{

    public int StemHeight;
    public int StemBendHeight;
    public int CapRadius;
    public Vector3i CurrentMainStemSampleOffset;
    public Vector3i StemOffsetDirection;
    public bool CapStarted;
    public bool IsGrowing;
    public Queue<Vector3i> CapGrowth = new();
    public RedMushroomMyceliumProperties() 
    {

        StemHeight = GlobalValues.RandomGenerator.Next(8, 13);
        StemBendHeight = GlobalValues.RandomGenerator.Next(4, 8);
        CapRadius = StemHeight / 2;
        CurrentMainStemSampleOffset = Vector3i.Zero;
        StemOffsetDirection = Vector3i.Zero;
        CapStarted = false;
        IsGrowing = true;

        StemOffsetDirection.X = GlobalValues.RandomGenerator.Next(-1, 2);
        if (StemOffsetDirection.X == 0)
        {

            StemOffsetDirection.Z = GlobalValues.RandomGenerator.Next(-1, 2);

        }

    }
    public IBlockProperties FromBytes(DataReader reader)
    {
        throw new System.NotImplementedException();
    }

    public void ToBytes(DataWriter writer)
    {
        throw new System.NotImplementedException();
    }
}