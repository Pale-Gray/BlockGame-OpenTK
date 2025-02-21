using Blockgame_OpenTK.Core.Worlds;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Chunks;

public class ChunkColumn
{

    public Vector2i Position;
    public PackedChunk[] Chunks = new PackedChunk[PackedWorldGenerator.WorldGenerationHeight];
    
}