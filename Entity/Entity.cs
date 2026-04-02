using System.Data;
using Fleshgolem.Ruleset;
using Fleshgolem.Data;

namespace Fleshgolem.Entity;

public class BodyPart(string name, BodyParts part, BaseAttributes attr, Guid id = new Guid())
{
    public Guid Id { get; protected set; } = id;
    public string Name { get; set; } = name;
    public BodyParts Part { get; set; } = part;
    public BaseAttributes Attributes { get; set; } = attr;
}

public class Limb(BodyPart bodyPart)
{
    public Guid Id { get; } = Guid.NewGuid();
    public BodyPart BodyPart { get; } = bodyPart;
    public int ArmorPoint { get; set; } = 0;
    public int HitPoint { get; set; } = 0;
}

public sealed class EmptySocket
{
    private static readonly Lazy<EmptySocket> _lazy = new(() => new EmptySocket());
    public static EmptySocket Instance() => _lazy.Value;

    private EmptySocket() { }

    public Limb Head { get; set; } = new(new BodyPart("Empty Socket", BodyParts.Head, new BaseAttributes(0, 0, 0)));
    public Limb Chest { get; set; } = new(new BodyPart("Empty Chest", BodyParts.Chest, new BaseAttributes(0, 0, 0)));
    public Limb Abdomen { get; set; } = new(new BodyPart("Empty Abdomen", BodyParts.Abdomen, new BaseAttributes(0, 0, 0)));
    public Limb LeftArm { get; set; } = new(new BodyPart("Empty Left Arm", BodyParts.LeftArm, new BaseAttributes(0, 0, 0)));
    public Limb RightArm { get; set; } = new(new BodyPart("Empty Right Arm", BodyParts.RightArm, new BaseAttributes(0, 0, 0)));
    public Limb LeftLeg { get; set; } = new(new BodyPart("Empty Left Leg", BodyParts.LeftLeg, new BaseAttributes(0, 0, 0)));
    public Limb RightLeg { get; set; } = new(new BodyPart("Empty Right Leg", BodyParts.RightLeg, new BaseAttributes(0, 0, 0)));
    public Limb Tail { get; set; } = new(new BodyPart("Empty Tail", BodyParts.Tail, new BaseAttributes(0, 0, 0)));
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

    public BaseAttributes BaseAttribute { get; set; } = new(0, 0, 0);
    private static readonly EmptySocket emptySocket = EmptySocket.Instance();

    public Limb Head { get; set; } = emptySocket.Head;
    public Limb Chest { get; set; } = emptySocket.Chest;
    public Limb Abdomen { get; set; } = emptySocket.Abdomen;
    public Limb LeftArm { get; set; } = emptySocket.LeftArm;
    public Limb RightArm { get; set; } = emptySocket.RightArm;
    public Limb LeftLeg { get; set; } = emptySocket.LeftLeg;
    public Limb RightLeg { get; set; } = emptySocket.RightLeg;
    public Limb Tail { get; set; } = emptySocket.Tail;

    public void AttachLimb(Limb limb)
    {
        switch (limb.BodyPart.Part)
        {
            case BodyParts.Head: Head = limb; break;
            case BodyParts.Chest: Chest = limb; break;
            case BodyParts.Abdomen: Abdomen = limb; break;
            case BodyParts.LeftArm: LeftArm = limb; break;
            case BodyParts.RightArm: RightArm = limb; break;
            case BodyParts.LeftLeg: LeftLeg = limb; break;
            case BodyParts.RightLeg: RightLeg = limb; break;
            case BodyParts.Tail: Tail = limb; break;
        }
    }

    public void DetachLimb(BodyParts bodypart)
    {
        switch (bodypart)
        {
            case BodyParts.Head: Head = emptySocket.Head; break;
            case BodyParts.Chest: Chest = emptySocket.Chest; break;
            case BodyParts.Abdomen: Abdomen = emptySocket.Abdomen; break;
            case BodyParts.LeftArm: LeftArm = emptySocket.LeftArm; break;
            case BodyParts.RightArm: RightArm = emptySocket.RightArm; break;
            case BodyParts.LeftLeg: LeftLeg = emptySocket.LeftLeg; break;
            case BodyParts.RightLeg: RightLeg = emptySocket.RightLeg; break;
            case BodyParts.Tail: Tail = emptySocket.Tail; break;
        }
    }

    public void SetBaseStrength()
    {
        int baseMultiplier;
        baseMultiplier = 8;
        if (Tail.BodyPart.Attributes.Strength == 0)
        {
            baseMultiplier = 7;
        }

        int BaseAttributeSum = (Head?.BodyPart.Attributes.Strength ?? 0) +
        (Chest?.BodyPart.Attributes.Strength ?? 0) +
        (Abdomen?.BodyPart.Attributes.Strength ?? 0) +
        (LeftArm?.BodyPart.Attributes.Strength ?? 0) +
        (RightArm?.BodyPart.Attributes.Strength ?? 0) +
        (LeftLeg?.BodyPart.Attributes.Strength ?? 0) +
        (RightLeg?.BodyPart.Attributes.Strength ?? 0) +
        (Tail?.BodyPart.Attributes.Strength ?? 0);

        BaseAttribute.Strength = BaseAttributeSum / baseMultiplier;
    }

    public void SetBaseSize()
    {
        BaseAttribute.Size = (Head?.BodyPart.Attributes.Size ?? 0) +
        (Chest?.BodyPart.Attributes.Size ?? 0) +
        (Abdomen?.BodyPart.Attributes.Size ?? 0) +
        (LeftArm?.BodyPart.Attributes.Size ?? 0) +
        (RightArm?.BodyPart.Attributes.Size ?? 0) +
        (LeftLeg?.BodyPart.Attributes.Size ?? 0) +
        (RightLeg?.BodyPart.Attributes.Size ?? 0) +
        (Tail?.BodyPart.Attributes.Size ?? 0);
    }

    public void SetBaseDexterity()
    {
        BaseAttribute.Dexterity = (Head?.BodyPart.Attributes.Dexterity ?? 0) +
        (Chest?.BodyPart.Attributes.Dexterity ?? 0) +
        (Abdomen?.BodyPart.Attributes.Dexterity ?? 0) +
        (LeftArm?.BodyPart.Attributes.Dexterity ?? 0) +
        (RightArm?.BodyPart.Attributes.Dexterity ?? 0) +
        (LeftLeg?.BodyPart.Attributes.Dexterity ?? 0) +
        (RightLeg?.BodyPart.Attributes.Dexterity ?? 0) +
        (Tail?.BodyPart.Attributes.Dexterity ?? 0);
    }
}
