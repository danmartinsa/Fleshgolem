namespace Fleshgolem.Ruleset;

/// <summary>All Mythras Imperative characteristics that can appear on a creature or body part.</summary>
public enum Attributes
{
    Strength,
    Constitution,
    Size,
    Dexterity,
    Intelligence,
    Power,
}

/// <summary>Discrete hit locations used by the d20 location table and the body-part socket system.</summary>
public enum BodyParts
{
    Head,
    LeftArm,
    RightArm,
    Abdomen,
    Chest,
    LeftLeg,
    RightLeg,
    /// <summary>Optional location; treated as absent when its Size is 0.</summary>
    Tail,
}

/// <summary>
/// Holds the four characteristics tracked per body part and, at the entity level,
/// the derived average across all attached limbs.
/// </summary>
/// <param name="str">Strength – governs damage bonus and combat skill.</param>
/// <param name="con">Constitution – contributes to per-location hit points.</param>
/// <param name="sz">Size – contributes to per-location hit points and the hit-location roll.</param>
/// <param name="dex">Dexterity – governs initiative, action points, and combat skill.</param>
public class BaseAttributes(int str, int con, int sz, int dex)
{
    /// <summary>Raw muscular power. Feeds damage bonus and combat skill.</summary>
    public int Strength { get; set; } = str;

    /// <summary>Toughness and vitality. Used in the Mythras HP formula: ceil((CON + SIZ) / 5).</summary>
    public int Constitution { get; set; } = con;

    /// <summary>Physical bulk. Affects hit points and the hit-location probability table.</summary>
    public int Size { get; set; } = sz;

    /// <summary>Speed and coordination. Feeds initiative, action points, and combat skill.</summary>
    public int Dexterity { get; set; } = dex;
}
