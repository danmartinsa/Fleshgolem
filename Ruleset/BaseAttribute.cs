namespace Fleshgolem.Ruleset;

public enum Attributes
{
    Strength,
    Constitution,
    Size,
    Dexterity,
    Intelligence,
    Power,
}

public enum BodyParts
{
    Head,
    LeftArm,
    RightArm,
    Abdomen,
    Chest,
    LeftLeg,
    RightLeg,
    Tail,
}

// public class Attribute(Attributes attr)
// {
//     public Attributes Type { get; set; } = attr;
//     public int Value { get; set; } =
// }

public class BaseAttributes(int str, int sz, int dex)
{
    public int Strength { get; set; } = str;
    public int Size { get; set; } = sz;
    public int Dexterity { get; set; } = dex;
}
