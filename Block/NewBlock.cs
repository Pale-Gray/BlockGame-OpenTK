namespace Blockgame_OpenTK.BlockUtil;

public struct Namespace
{

    public string Prefix;
    public string Suffix;

    public override string ToString()
    {
        return $"{Prefix}.{Suffix}";
    }

    public Namespace(string prefix, string suffix)
    {
        
        Prefix = prefix;
        Suffix = suffix;
        
    }

}
public class NewBlock
{

    public bool IsSolidBlock = true;
    public NewBlockModel BlockModel;
    public ushort Id;
    
    public NewBlock()
    {
        
    }

}