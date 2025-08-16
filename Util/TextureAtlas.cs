using OpenTK.Mathematics;

namespace VoxelGame;

public class TextureAtlas
{
    private Texture _texture;
    private Vector2 _tileSize;
    public int Id => _texture.Id;
    public TextureAtlas(string path, Vector2i tileSize)
    {
        _texture = new Texture(path);
        _tileSize = tileSize;
    }

    public TextureAtlas Generate()
    {
        _texture.Generate();
        return this;
    }

    public Vector2[] CoordinatesAt(Vector2i coordinate)
    {
        Vector2[] coordinates = new Vector2[4];
        Vector2 tileOffset = (1 / _tileSize.X, 1 / _tileSize.Y);
        
        // top left, bottom left, bottom right, top right
        coordinates[0] = (tileOffset.X * coordinate.X, 1.0f - (tileOffset.Y * coordinate.Y));
        coordinates[1] = (tileOffset.X * coordinate.X, 1.0f - tileOffset.Y - (tileOffset.Y * coordinate.Y));
        coordinates[2] = ((tileOffset.X * coordinate.X) + tileOffset.X, 1.0f - tileOffset.Y - (tileOffset.Y * coordinate.Y));
        coordinates[3] = ((tileOffset.X * coordinate.X) + tileOffset.X, 1.0f - (tileOffset.Y * coordinate.Y));
        
        return coordinates;
    }
}