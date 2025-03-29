namespace Game.BlockProperty;
public interface IBlockProperties
{

    public abstract void ToBytes(DataWriter writer);

    public abstract IBlockProperties FromBytes(DataReader reader);

}
