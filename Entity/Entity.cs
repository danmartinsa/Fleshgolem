using Fleshgolem.Ruleset;
namespace Fleshgolem.Entity;


public class Limb(string name, BodyParts bodyPart, BaseAttributes attr)
{
    public string Name { get; set; } = name;
    public BodyParts BodyPart { get; set; } = bodyPart;
    public BaseAttributes Attributes { get; set; } = attr;
}

// Head,
// LeftArm,
// RightArm,
// Abdomen,
// Chest,
// LeftLeg,
// RightLeg,
// Tail,

public class Entity(string name)
{
    public string Name { get; set; } = name;
    public Limb? Head { get; set; }
    public Limb? Chest { get; set; }
    public Limb? Abdomen { get; set; }
    public Limb? LeftArm { get; set; }
    public Limb? RightArm { get; set; }
    public Limb? LeftLeg { get; set; }
    public Limb? RightLeg { get; set; }
    public Limb? Tail { get; set; }

    public void AttachLimb()
    {

    }
}
