using System.Collections.Generic;
using OpenTK.Mathematics;

namespace Game.Core.BlockStorage
{
    public class NewProperties : IBlockProperties
    {

        public bool MyBool = false;
        public string MyString = "Hello World!";
        public int MyInt = 12934;
        public long MyLong = 1902480914891;
        public List<int> MyList = [ 1, 2, 3, 4, 5, 6 ];
        public Vector3 MyFloatVector = (4002.001f, 50.041f, 0.0442f);
        public Vector3i MyIntVector = (1, 50, 0);
        public void ToBytes(DataWriter writer)
        {

            writer.WriteBool(MyBool);
            writer.WriteString(MyString);
            writer.WriteInt(MyInt);
            writer.WriteLong(MyLong);
            writer.WriteList(MyList);
            writer.WriteVector3(MyFloatVector);
            writer.WriteVector3i(MyIntVector);

        }

        public IBlockProperties FromBytes(DataReader reader)
        {

            NewProperties prop = new NewProperties();
            prop.MyBool = reader.GetBool();
            prop.MyString = reader.GetString();
            prop.MyInt = reader.GetInt();
            prop.MyLong = reader.GetLong();
            prop.MyList = reader.GetIntList();
            prop.MyFloatVector = reader.GetVector3();
            prop.MyIntVector = reader.GetVector3i();

            return prop;

        }
    }
}
