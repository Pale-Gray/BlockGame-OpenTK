namespace Blockgame_OpenTK.Util
{
    public class ArrayUtils
    {

        public static float[] BlockFaceShift(float[] array, float x, float y, float z)
        {

            float[] newArray = new float[array.Length];

            float[] offsets =
            {

                0, x, y, z, 0, 0, 0, 0, 0,
                0, x, y, z, 0, 0, 0, 0, 0,
                0, x, y, z, 0, 0, 0, 0, 0,
                0, x, y, z, 0, 0, 0, 0, 0,
                0, x, y, z, 0, 0, 0, 0, 0,
                0, x, y, z, 0, 0, 0, 0, 0,

            };

            for (int i = 0; i < array.Length; i++)
            {

                newArray[i] = array[i] + offsets[i];

            }

            return newArray;

        }

    }
}
