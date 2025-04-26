using System;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Game.Core.Worlds;
using Game.Util;
using OpenTK.Mathematics;

namespace Game.BlockUtil;

public class RedMushroomMyceliumBlock : Block
{

    public override void OnBlockPlace(World world, Vector3i globalBlockPosition, bool shouldUpdateMesh, bool hasPriority)
    {

        base.OnBlockPlace(world, globalBlockPosition, shouldUpdateMesh, hasPriority);

        world.AddBlockProperty(new RedMushroomMyceliumProperties(), globalBlockPosition);

    }

    private Vector3i[] _horizontalOffsets = { Vector3i.UnitX, -Vector3i.UnitX, Vector3i.UnitZ, -Vector3i.UnitZ };

    public override void OnRandomTick(World world, Vector3i globalBlockPosition, bool shouldUpdateMesh, bool hasPriority)
    {

        if (world.TryGetBlockProperty(globalBlockPosition, out RedMushroomMyceliumProperties properties) && properties.IsGrowing)
        {

            if (properties.CurrentMainStemSampleOffset.Y < properties.StemBendHeight)
            {

                properties.CurrentMainStemSampleOffset.Y++;
                GlobalValues.Register.GetBlockFromNamespace("Game.RedMushroomStem").OnBlockPlace(world, globalBlockPosition + properties.CurrentMainStemSampleOffset, shouldUpdateMesh, hasPriority);

            } else if (properties.CurrentMainStemSampleOffset.Y == properties.StemBendHeight)
            {

                properties.CurrentMainStemSampleOffset.Y++;
                GlobalValues.Register.GetBlockFromNamespace("Game.RedMushroomStem").OnBlockPlace(world, globalBlockPosition + properties.CurrentMainStemSampleOffset, shouldUpdateMesh, hasPriority);
                GlobalValues.Register.GetBlockFromNamespace("Game.RedMushroomStem").OnBlockPlace(world, globalBlockPosition + properties.CurrentMainStemSampleOffset + properties.StemOffsetDirection, shouldUpdateMesh, hasPriority);

            } else if (properties.CurrentMainStemSampleOffset.Y < properties.StemHeight)
            {

                properties.CurrentMainStemSampleOffset.Y++;
                GlobalValues.Register.GetBlockFromNamespace("Game.RedMushroomStem").OnBlockPlace(world, globalBlockPosition + properties.CurrentMainStemSampleOffset + properties.StemOffsetDirection, shouldUpdateMesh, hasPriority);

            } else if (properties.CurrentMainStemSampleOffset.Y == properties.StemHeight && properties.CapStarted == false)
            {

                properties.CapStarted = true;
                properties.CapGrowth.Enqueue((0, 0, 0));
                properties.CapGrowth.Enqueue((0, 1, 0));

            }

            if (properties.CapGrowth.TryDequeue(out Vector3i offset))
            {

                Vector3i stemPosition = globalBlockPosition + properties.CurrentMainStemSampleOffset + properties.StemOffsetDirection;
                Vector3i position = globalBlockPosition + properties.CurrentMainStemSampleOffset + properties.StemOffsetDirection + offset;

                if (offset.Y == 1)
                {

                    GlobalValues.Register.GetBlockFromNamespace("Game.RedMushroomCap").OnBlockPlace(world, position, shouldUpdateMesh, hasPriority);

                    for (int i = 0; i < _horizontalOffsets.Length; i++)
                    {

                        if (!world.GetSolidBlock(position + _horizontalOffsets[i]))
                        {

                            if (Maths.ChebyshevDistance3D(stemPosition + (0, 1, 0), position + _horizontalOffsets[i]) < properties.CapRadius &&
                                Maths.ChebyshevDistance3D(stemPosition + (0, 1, 0), position + _horizontalOffsets[i]) < properties.CapRadius + 3)
                            {

                                properties.CapGrowth.Enqueue(offset + _horizontalOffsets[i]);
                                
                            }

                            /*
                            if (Maths.ChebyshevDistance3D(stemPosition + (0, 1, 0), position + _horizontalOffsets[i]) < properties.CapRadius)
                            {

                                if (offset + _horizontalOffsets[i] != (properties.CapRadius - 1, 0, properties.CapRadius - 1) &&
                                    offset + _horizontalOffsets[i] != (properties.CapRadius - 1, 0, -properties.CapRadius - 1) &&
                                    offset + _horizontalOffsets[i] != (-properties.CapRadius - 1, 0, properties.CapRadius - 1) &&
                                    offset + _horizontalOffsets[i] != (-properties.CapRadius - 1, 0, -properties.CapRadius - 1)
                                    )
                                {

                                    properties.CapGrowth.Enqueue(offset + _horizontalOffsets[i]);

                                }

                            }
                            */

                        }

                    }

                } else
                {

                    for (int i = 0; i < _horizontalOffsets.Length; i++)
                    {

                        if (!world.GetSolidBlock(position + _horizontalOffsets[i]))
                        {

                            if (Maths.ChebyshevDistance3D(stemPosition, position + _horizontalOffsets[i]) <= properties.CapRadius &&
                                Maths.ChebyshevDistance3D(stemPosition, position + _horizontalOffsets[i]) <= properties.CapRadius + 3)
                            {

                                properties.CapGrowth.Enqueue(offset + _horizontalOffsets[i]);
                                GlobalValues.Register.GetBlockFromNamespace("Game.RedMushroomCap").OnBlockPlace(world, position + _horizontalOffsets[i], shouldUpdateMesh, hasPriority);
                                
                            }

                            /*
                            if (Maths.ChebyshevDistance3D(stemPosition, position + _horizontalOffsets[i]) <= properties.CapRadius)
                            {

                                if (offset + _horizontalOffsets[i] != (properties.CapRadius, 0, properties.CapRadius) &&
                                    offset + _horizontalOffsets[i] != (properties.CapRadius, 0, -properties.CapRadius) &&
                                    offset + _horizontalOffsets[i] != (-properties.CapRadius, 0, properties.CapRadius) &&
                                    offset + _horizontalOffsets[i] != (-properties.CapRadius, 0, -properties.CapRadius)
                                    )
                                {
                                    
                                    

                                }

                            }
                            */

                        }

                    }

                }

            }

            if (properties.CapStarted && properties.CapGrowth.Count == 0) properties.IsGrowing = false;

        }

    }

}