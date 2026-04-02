using System.Data;
using Fleshgolem.Ruleset;
using Fleshgolem.Data;

namespace Fleshgolem.Entities;

/// <summary>
/// Describes a single anatomical body part – its identity, hit-location slot,
/// and intrinsic characteristics. Body parts are the raw biological template;
/// attach them to a <see cref="Limb"/> to give them armour and hit points.
/// </summary>
public class BodyPart(string name, BodyParts part, BaseAttributes attr, Guid id = new Guid())
{
    /// <summary>Stable identifier, useful for serialisation and look-up.</summary>
    public Guid Id { get; protected set; } = id;

    /// <summary>Display name, e.g. "Manticore Head" or "Empty Socket".</summary>
    public string Name { get; set; } = name;

    /// <summary>Which socket on an <see cref="Entity"/> this part occupies.</summary>
    public BodyParts Part { get; set; } = part;

    /// <summary>Intrinsic characteristics of this part (STR, CON, SIZ, DEX).</summary>
    public BaseAttributes Attributes { get; set; } = attr;
}

/// <summary>
/// A <see cref="BodyPart"/> installed in an entity's socket, augmented with
/// combat state: current hit points and armour rating.
/// </summary>
public class Limb(BodyPart bodyPart)
{
    /// <summary>Instance identifier for this limb installation.</summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>The underlying body part providing name, location slot, and characteristics.</summary>
    public BodyPart BodyPart { get; } = bodyPart;

    /// <summary>Armour points absorbed before hit points are reduced.</summary>
    public int ArmorPoint { get; set; } = 0;

    /// <summary>Current hit points. Reaches 0 or below when the limb is maimed.</summary>
    public int HitPoint { get; set; } = 0;

    /// <summary>
    /// Maximum hit points for this location, calculated using the Mythras Imperative formula:
    /// <c>ceil((CON + SIZ) / 5)</c> plus a location modifier (+2 Chest, +1 Head/Abdomen, +0 others).
    /// </summary>
    public int MaxHP
    {
        get
        {
            int conPlusSiz = BodyPart.Attributes.Constitution + BodyPart.Attributes.Size;
            int @base = (int)Math.Ceiling(conPlusSiz / 5.0);

            // Vital locations receive bonus HP reflecting their importance.
            int modifier = BodyPart.Part switch
            {
                BodyParts.Chest   => 2,
                BodyParts.Head    => 1,
                BodyParts.Abdomen => 1,
                _                 => 0,
            };

            return Math.Max(1, @base + modifier);
        }
    }
}

/// <summary>
/// Singleton that provides default zero-stat limbs for every socket.
/// Used as the initial state of a new <see cref="Entity"/> and as the
/// replacement when a limb is detached.
/// </summary>
public sealed class EmptySocket
{
    private static readonly Lazy<EmptySocket> _lazy = new(() => new EmptySocket());
    /// <summary>Returns the shared <see cref="EmptySocket"/> instance.</summary>
    public static EmptySocket Instance() => _lazy.Value;

    private EmptySocket() { }

    public Limb Head { get; set; } = new(new BodyPart("Empty Socket", BodyParts.Head, new BaseAttributes(0, 0, 0, 0)));
    public Limb Chest { get; set; } = new(new BodyPart("Empty Chest", BodyParts.Chest, new BaseAttributes(0, 0, 0, 0)));
    public Limb Abdomen { get; set; } = new(new BodyPart("Empty Abdomen", BodyParts.Abdomen, new BaseAttributes(0, 0, 0, 0)));
    public Limb LeftArm { get; set; } = new(new BodyPart("Empty Left Arm", BodyParts.LeftArm, new BaseAttributes(0, 0, 0, 0)));
    public Limb RightArm { get; set; } = new(new BodyPart("Empty Right Arm", BodyParts.RightArm, new BaseAttributes(0, 0, 0, 0)));
    public Limb LeftLeg { get; set; } = new(new BodyPart("Empty Left Leg", BodyParts.LeftLeg, new BaseAttributes(0, 0, 0, 0)));
    public Limb RightLeg { get; set; } = new(new BodyPart("Empty Right Leg", BodyParts.RightLeg, new BaseAttributes(0, 0, 0, 0)));
    public Limb Tail { get; set; } = new(new BodyPart("Empty Tail", BodyParts.Tail, new BaseAttributes(0, 0, 0, 0)));
}


/// <summary>
/// A creature composed of interchangeable <see cref="Limb"/> parts.
/// Base characteristics are recalculated automatically whenever a limb
/// is attached or detached, averaging the characteristics across all
/// occupied sockets (7 divisor when no tail is present, 8 otherwise).
/// </summary>
public class Entity
{
    /// <summary>Creates a nameless entity with all sockets empty.</summary>
    public Entity(string name)
    {
        Name = name;
        SetBaseAttributes();
    }

    /// <summary>Display name of the creature.</summary>
    public string Name { get; set; }

    /// <summary>Averaged characteristics derived from all attached limbs.</summary>
    public BaseAttributes BaseAttribute { get; set; } = new(0, 0, 0, 0);
    private static readonly EmptySocket emptySocket = EmptySocket.Instance();

    public Limb Head { get; set; } = emptySocket.Head;
    public Limb Chest { get; set; } = emptySocket.Chest;
    public Limb Abdomen { get; set; } = emptySocket.Abdomen;
    public Limb LeftArm { get; set; } = emptySocket.LeftArm;
    public Limb RightArm { get; set; } = emptySocket.RightArm;
    public Limb LeftLeg { get; set; } = emptySocket.LeftLeg;
    public Limb RightLeg { get; set; } = emptySocket.RightLeg;
    public Limb Tail { get; set; } = emptySocket.Tail;

    /// <summary>
    /// Installs <paramref name="limb"/> in the matching socket and recalculates base attributes.
    /// Replaces whatever was previously in that slot.
    /// </summary>
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

        SetBaseAttributes();
    }

    /// <summary>
    /// Removes the limb at <paramref name="bodypart"/>, replacing it with the empty socket,
    /// and recalculates base attributes. Called automatically when a limb is maimed in combat.
    /// </summary>
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

        SetBaseAttributes();
    }

    /// <summary>
    /// Averages a single characteristic across all sockets using the supplied selector.
    /// The divisor is 7 when the tail socket is empty (zero value), 8 when a tail is attached.
    /// </summary>
    private int CalculateBaseAttribute(Func<BaseAttributes, int> selector)
    {
        // Tail is optional: exclude it from the average when its slot is empty.
        int baseMultiplier = selector(Tail.BodyPart.Attributes) == 0 ? 7 : 8;

        int sum = selector(Head.BodyPart.Attributes) +
                  selector(Chest.BodyPart.Attributes) +
                  selector(Abdomen.BodyPart.Attributes) +
                  selector(LeftArm.BodyPart.Attributes) +
                  selector(RightArm.BodyPart.Attributes) +
                  selector(LeftLeg.BodyPart.Attributes) +
                  selector(RightLeg.BodyPart.Attributes) +
                  selector(Tail.BodyPart.Attributes);

        return sum / baseMultiplier;
    }

    /// <summary>Recalculates all base characteristics from the currently attached limbs.</summary>
    public void SetBaseAttributes()
    {
        BaseAttribute.Strength = CalculateBaseAttribute(a => a.Strength);
        BaseAttribute.Constitution = CalculateBaseAttribute(a => a.Constitution);
        BaseAttribute.Size = CalculateBaseAttribute(a => a.Size);
        BaseAttribute.Dexterity = CalculateBaseAttribute(a => a.Dexterity);
    }

    /// <summary>
    /// Sets every limb's <see cref="Limb.HitPoint"/> to its <see cref="Limb.MaxHP"/>.
    /// Call this after all limbs have been attached and before the first combat round.
    /// </summary>
    public void InitializeHP()
    {
        foreach (Limb limb in new[] { Head, Chest, Abdomen, LeftArm, RightArm, LeftLeg, RightLeg, Tail })
            limb.HitPoint = limb.MaxHP;
    }
}
