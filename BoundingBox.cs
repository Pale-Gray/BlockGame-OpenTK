using OpenTK.Mathematics;

namespace Blockgame_OpenTK
{
    internal class BoundingBox
    {

        public Vector3 position, dimension, origin;
        Vector3 original_position;
        Vector3 origin_position;
        public Vector3 offset;

        public float[] triangles;
        public BoundingBox(Vector3 position, Vector3 dimension, Vector3 origin)
        {

            this.position = position;
            this.original_position = position;
            this.dimension = dimension;
            this.origin = origin;

            origin_position = position + origin;

            GenerateTriangles();

        }

        private void GenerateTriangles()
        {

            triangles = new float[] {

                //front
                position.X - origin.X, position.Y - origin.Y, (position.Z + dimension.Z) - origin.Z,
                (position.X + dimension.X) - origin.X, position.Y - origin.Y, (position.Z + dimension.Z) - origin.Z,
                (position.X + dimension.X) - origin.X, (position.Y + dimension.Y) - origin.X, (position.Z + dimension.Z) - origin.Z,

                (position.X + dimension.X) - origin.X, (position.Y + dimension.Y) - origin.X, (position.Z + dimension.Z) - origin.Z,
                position.X - origin.X, (position.Y + dimension.Y) - origin.Y, (position.Z + dimension.Z) - origin.Z,
                position.X - origin.X, position.Y - origin.Y, (position.Z + dimension.Z) - origin.Z,

                //right
                (position.X + dimension.X) - origin.X, position.Y - origin.Y, (position.Z + dimension.Z) - origin.Z,
                (position.X + dimension.X) - origin.X, position.Y - origin.Y, position.Z - origin.Z,
                (position.X + dimension.X) - origin.X, (position.Y + dimension.Y) - origin.Y, position.Z - origin.Z,

                (position.X + dimension.X) - origin.X, (position.Y + dimension.Y) - origin.Y, position.Z - origin.Z,
                (position.X + dimension.X) - origin.X, (position.Y + dimension.Y) - origin.Y, (position.Z + dimension.Z) - origin.Z,
                (position.X + dimension.X) - origin.X, position.Y - origin.Y, (position.Z + dimension.Z) - origin.Z,

                //back
                (position.X + dimension.X) - origin.X, position.Y - origin.Y, position.Z - origin.Z,
                position.X - origin.X, position.Y - origin.Y, position.Z - origin.Z,
                position.X - origin.X, (position.Y + dimension.Y) - origin.Y, position.Z - origin.Z,

                position.X - origin.X, (position.Y + dimension.Y) - origin.Y, position.Z - origin.Z,
                (position.X + dimension.X) - origin.X, (position.Y + dimension.Y) - origin.Y, position.Z - origin.Z,
                (position.X + dimension.X) - origin.X, position.Y - origin.Y, position.Z - origin.Z,

                //left
                position.X - origin.X, position.Y - origin.Y, position.Z - origin.Z,
                position.X - origin.X, position.Y - origin.Y, (position.Z + dimension.Z) - origin.Z,
                position.X - origin.X, (position.Y + dimension.Y) - origin.Y, (position.Z + dimension.Z) - origin.Z,

                position.X - origin.X, (position.Y + dimension.Y) - origin.Y, (position.Z + dimension.Z) - origin.Z,
                position.X - origin.X, (position.Y + dimension.Y) - origin.Y, position.Z - origin.Z,
                position.X - origin.X, position.Y - origin.Y, position.Z - origin.Z,

                //top
                position.X - origin.X, (position.Y + dimension.Y) - origin.Y, (position.Z + dimension.Z) - origin.Z,
                (position.X + dimension.X) - origin.X, (position.Y + dimension.Y) - origin.Y, (position.Z + dimension.Z) - origin.Z,
                (position.X + dimension.X) - origin.X, (position.Y + dimension.Y) - origin.Y, position.Z - origin.Z,

                (position.X + dimension.X) - origin.X, (position.Y + dimension.Y) - origin.Y, position.Z - origin.Z,
                position.X - origin.X, (position.Y + dimension.Y) - origin.Y, position.Z - origin.Z,
                position.X - origin.X, (position.Y + dimension.Y) - origin.Y, (position.Z + dimension.Z) - origin.Z,

                //bottom
                position.X - origin.X, position.Y - origin.Y, position.Z - origin.Z,
                (position.X + dimension.X) - origin.X, position.Y - origin.Y, position.Z - origin.Z,
                (position.X + dimension.X) - origin.X, position.Y - origin.Y, (position.Z + dimension.Z) - origin.Z,

                (position.X + dimension.X) - origin.X, position.Y - origin.Y, (position.Z + dimension.Z) - origin.Z,
                position.X - origin.X, position.Y - origin.Y, (position.Z + dimension.Z) - origin.Z,
                position.X - origin.X, position.Y - origin.Y, position.Z - origin.Z,

            };
            

        }

        public void SetOffset(float x, float y, float z)
        {

            position = original_position + new Vector3(x, y, z);
            origin_position = position + origin;
            offset = new Vector3(x, y, z);
            GenerateTriangles();

        }

    }
}
