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

public class BaseAttributes(int str, int sz, int dex)
{
    public int Strength { get; set; } = str;
    public int Size { get; set; } = sz;
    public int Dexterity { get; set; } = dex;
}
