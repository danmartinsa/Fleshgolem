using Fleshgolem.Entities;

namespace Fleshgolem.Ruleset;

// ── Position ──────────────────────────────────────────────────────────────────

/// <summary>2-D position in abstract combat space (units).</summary>
public record struct Position(float X, float Y)
{
    /// <summary>Euclidean distance to another position.</summary>
    public float DistanceTo(Position other) =>
        MathF.Sqrt(MathF.Pow(X - other.X, 2) + MathF.Pow(Y - other.Y, 2));
}

// ── Combat participant ────────────────────────────────────────────────────────

/// <summary>An <see cref="Entity"/> placed in the combat arena with a known position.</summary>
public class CombatParticipant(Entity entity, Position position)
{
    /// <summary>The entity taking part in this encounter.</summary>
    public Entity Entity { get; } = entity;

    /// <summary>Current position in abstract combat space.</summary>
    public Position Position { get; set; } = position;
}

// ── Wound states per limb ────────────────────────────────────────────────────

/// <summary>
/// Severity of damage sustained by a single hit location.
/// Thresholds are evaluated against <see cref="Limb.MaxHP"/> after each hit.
/// </summary>
public enum WoundLevel
{
    None,
    Wounded,
    Serious,
    Maimed,
}

// ── Outcome of a single opposed roll ────────────────────────────────────────

/// <summary>The degree of success (or failure) produced by an opposed attack roll.</summary>
public enum CombatResult
{
    Miss,
    Fumble,
    Hit,
    CriticalHit,
}

// ── Full result of one attack action ────────────────────────────────────────

/// <summary>Immutable summary of a single resolved attack action.</summary>
/// <param name="Result">Degree of success of the opposed roll.</param>
/// <param name="HitLocation">Body-part struck, or <see langword="null"/> on a miss/fumble.</param>
/// <param name="RawDamage">Damage rolled before armour mitigation.</param>
/// <param name="ArmorMitigation">Armour points subtracted from raw damage.</param>
/// <param name="FinalDamage">Damage actually applied to the hit location's HP.</param>
/// <param name="WoundCaused">Wound severity of the hit location after taking damage.</param>
/// <param name="LimbMaimed">True when a peripheral limb was severed this action.</param>
/// <param name="Fatal">True when a vital location (Head, Chest, Abdomen) reached 0 HP.</param>
public record AttackResult(
    CombatResult Result,
    BodyParts?   HitLocation,
    int          RawDamage,
    int          ArmorMitigation,
    int          FinalDamage,
    WoundLevel   WoundCaused,
    bool         LimbMaimed,
    bool         Fatal
);

/// <summary>
/// Stateless service class implementing all Mythras Imperative combat rules:
/// opposed skill rolls, hit location, wound evaluation, reach checks, and round sequencing.
/// </summary>
public static class Combat
{
    private static readonly Random _rng = new();

    // ── Derived stats ────────────────────────────────────────────────────────

    /// <summary>d100 target number for attack and defence rolls: <c>(STR + DEX) × 2</c>.</summary>
    public static int CombatSkill(Entity entity)
        => (entity.BaseAttribute.Strength + entity.BaseAttribute.Dexterity) * 2;

    /// <summary>
    /// Number of actions available per round: 1 base, +1 per 10 DEX above 10.
    /// A creature with DEX 20 has 2 AP; DEX 30 gives 3 AP.
    /// </summary>
    public static int ActionPoints(Entity entity)
        => 1 + Math.Max(0, (entity.BaseAttribute.Dexterity - 10) / 10);

    /// <summary>Rolls initiative for one entity: <c>DEX + 1d10</c>.</summary>
    public static int RollInitiative(Entity entity)
        => entity.BaseAttribute.Dexterity + _rng.Next(1, 11);

    /// <summary>
    /// Rolls the damage modifier die for an entity using the Mythras Imperative STR+SIZ table.
    /// Negative results are subtracted from damage (total damage is floored at 0 elsewhere).
    /// </summary>
    /// <remarks>
    /// STR+SIZ table:
    /// 1–5 → –1d8 | 6–10 → –1d6 | 11–15 → –1d4 | 16–20 → –1d2 | 21–25 → 0 |
    /// 26–30 → +1d2 | 31–35 → +1d4 | 36–40 → +1d6 | 41–45 → +1d8 | 46–50 → +1d10 |
    /// 51–60 → +1d12 | 61–70 → +2d6 | 71–80 → +1d8+1d6 | 81–90 → +2d8 |
    /// 91–100 → +1d10+1d8 | 101–110 → +2d10 | 111–120 → +2d10+1d2 | each further +10 → +1d2 more
    /// </remarks>
    public static int DamageBonus(Entity entity)
    {
        int score = entity.BaseAttribute.Strength + entity.BaseAttribute.Size;

        // Helper: roll a die with the given number of faces (1-indexed).
        int Roll(int faces) => _rng.Next(1, faces + 1);

        return score switch
        {
            <= 5   => -Roll(8),
            <= 10  => -Roll(6),
            <= 15  => -Roll(4),
            <= 20  => -Roll(2),
            <= 25  => 0,
            <= 30  => Roll(2),
            <= 35  => Roll(4),
            <= 40  => Roll(6),
            <= 45  => Roll(8),
            <= 50  => Roll(10),
            <= 60  => Roll(12),
            <= 70  => Roll(6) + Roll(6),
            <= 80  => Roll(8) + Roll(6),
            <= 90  => Roll(8) + Roll(8),
            <= 100 => Roll(10) + Roll(8),
            <= 110 => Roll(10) + Roll(10),
            <= 120 => Roll(10) + Roll(10) + Roll(2),
            // Each additional +10 above 120 adds another +1d2
            _      => Roll(10) + Roll(10) + Roll(2) + Roll(2) * ((score - 121) / 10 + 1),
        };
    }

    // ── Reach ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Unarmed reach of <paramref name="attacker"/> in abstract units, derived from the
    /// highest SIZ among its arms and tail: <c>ceil(SIZ / 8)</c>, minimum 1.
    /// </summary>
    public static float AttackReach(Entity attacker)
    {
        int bestSiz = Math.Max(
            Math.Max(attacker.LeftArm.BodyPart.Attributes.Size,
                     attacker.RightArm.BodyPart.Attributes.Size),
            attacker.Tail.BodyPart.Attributes.Size);
        return Math.Max(1f, MathF.Ceiling(bestSiz / 8f));
    }

    /// <summary>Returns true when <paramref name="attacker"/> can physically reach <paramref name="targetPos"/>.</summary>
    public static bool CanReach(Entity attacker, Position attackerPos, Position targetPos)
        => attackerPos.DistanceTo(targetPos) <= AttackReach(attacker);

    /// <summary>
    /// Returns true when both participants are within each other's reach simultaneously,
    /// i.e. they are mutually engaged.
    /// </summary>
    public static bool MutuallyInReach(CombatParticipant a, CombatParticipant b)
        => CanReach(a.Entity, a.Position, b.Position)
        && CanReach(b.Entity, b.Position, a.Position);


    // ── Hit location (d20 table) ─────────────────────────────────────────────

    /// <summary>
    /// Rolls a d20 and maps the result to a hit location using the standard
    /// Mythras Imperative table. Roll 19 hits the Tail only when one is present
    /// (SIZ &gt; 0); otherwise it falls through to the Head.
    /// </summary>
    public static BodyParts RollHitLocation(Entity target)
    {
        int roll     = _rng.Next(1, 21);
        bool hasTail = target.Tail.BodyPart.Attributes.Size > 0;

        return roll switch
        {
            >= 1  and <= 3  => BodyParts.RightLeg,
            >= 4  and <= 6  => BodyParts.LeftLeg,
            >= 7  and <= 9  => BodyParts.Abdomen,
            >= 10 and <= 12 => BodyParts.Chest,
            >= 13 and <= 15 => BodyParts.RightArm,
            >= 16 and <= 18 => BodyParts.LeftArm,
            19 when hasTail => BodyParts.Tail,
            _               => BodyParts.Head,  // roll 20, or roll 19 without a tail
        };
    }

    // ── Opposed roll resolution (Mythras Imperative) ─────────────────────────
    //
    //  Critical  = roll ≤ skill/10
    //  Hit       = roll ≤ skill
    //  Miss      = roll > skill
    //  Fumble    = roll > 95
    //
    //  Both succeed: higher degree of success wins;
    //  same degree:  higher absolute roll wins (attacker advantage on exact tie)

    /// <summary>
    /// Resolves one opposed roll between attacker and defender.
    /// Both sides roll d100 against their skill; the higher degree of success wins.
    /// On equal degrees the higher absolute roll wins; attacker wins exact ties.
    /// </summary>
    /// <returns>
    /// A tuple indicating whether the attacker succeeded and the resulting
    /// <see cref="CombatResult"/> (Miss, Fumble, Hit, or CriticalHit).
    /// </returns>
    private static (bool attackerWins, CombatResult result) ResolveOpposed(
        int attackSkill, int defendSkill)
    {
        int attackRoll = _rng.Next(1, 101);
        int defendRoll = _rng.Next(1, 101);

        // Critical threshold = skill / 10, floored but at least 1
        bool attackCrit    = attackRoll <= Math.Max(1, attackSkill / 10);
        bool attackSuccess = attackRoll <= attackSkill;
        bool attackFumble  = attackRoll > 95;  // automatic catastrophic failure

        bool defendCrit    = defendRoll <= Math.Max(1, defendSkill / 10);
        bool defendSuccess = defendRoll <= defendSkill;

        if (attackFumble)   return (false, CombatResult.Fumble);
        if (!attackSuccess) return (false, CombatResult.Miss);

        // Attacker succeeded; defender failed — attacker wins outright
        if (!defendSuccess)
            return (true, attackCrit ? CombatResult.CriticalHit : CombatResult.Hit);

        // Both succeeded — compare degrees; higher degree wins
        if (attackCrit && !defendCrit)
            return (true, CombatResult.CriticalHit);
        if (defendCrit && !attackCrit)
            return (false, CombatResult.Miss);

        // Same degree — higher absolute roll wins; attacker takes ties
        return attackRoll >= defendRoll
            ? (true,  attackCrit ? CombatResult.CriticalHit : CombatResult.Hit)
            : (false, CombatResult.Miss);
    }

    // ── Wound evaluation ─────────────────────────────────────────────────────

    /// <summary>
    /// Evaluates the current <see cref="WoundLevel"/> of a limb based on remaining HP
    /// relative to its maximum. Checked after every hit that reduces HP.
    /// </summary>
    private static WoundLevel GetWoundLevel(Limb limb)
    {
        int max = limb.MaxHP;
        if (limb.HitPoint <= 0)       return WoundLevel.Maimed;   // severed / destroyed
        if (limb.HitPoint <= max / 2) return WoundLevel.Serious;  // half HP or below
        if (limb.HitPoint < max)      return WoundLevel.Wounded;  // any damage taken
        return WoundLevel.None;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Returns the <see cref="Limb"/> corresponding to the given <paramref name="part"/> slot.</summary>
    public static Limb GetLimb(Entity entity, BodyParts part) => part switch
    {
        BodyParts.Head     => entity.Head,
        BodyParts.Chest    => entity.Chest,
        BodyParts.Abdomen  => entity.Abdomen,
        BodyParts.LeftArm  => entity.LeftArm,
        BodyParts.RightArm => entity.RightArm,
        BodyParts.LeftLeg  => entity.LeftLeg,
        BodyParts.RightLeg => entity.RightLeg,
        BodyParts.Tail     => entity.Tail,
        _                  => throw new ArgumentOutOfRangeException(nameof(part)),
    };

    /// <summary>Enumerates all eight limb slots of <paramref name="entity"/> in location-table order.</summary>
    private static IEnumerable<Limb> GetAllLimbs(Entity entity) =>
    [
        entity.Head, entity.Chest, entity.Abdomen,
        entity.LeftArm, entity.RightArm,
        entity.LeftLeg, entity.RightLeg, entity.Tail,
    ];

    /// <summary>
    /// Returns <see langword="true"/> for locations whose destruction is immediately fatal:
    /// Head, Chest, and Abdomen.
    /// </summary>
    public static bool IsVitalLocation(BodyParts part)
        => part is BodyParts.Head or BodyParts.Chest or BodyParts.Abdomen;

    /// <summary>
    /// Returns <see langword="true"/> when any vital location has reached 0 HP,
    /// rendering the entity dead or permanently incapacitated.
    /// </summary>
    public static bool IsIncapacitated(Entity entity)
        => entity.Head.HitPoint    <= 0
        || entity.Chest.HitPoint   <= 0
        || entity.Abdomen.HitPoint <= 0;

    /// <summary>Returns the peripheral (non-vital) limb slots that are currently maimed (HP ≤ 0).</summary>
    public static IEnumerable<BodyParts> MaimedLimbs(Entity entity)
    {
        (Limb limb, BodyParts part)[] peripheral =
        [
            (entity.LeftArm,  BodyParts.LeftArm),
            (entity.RightArm, BodyParts.RightArm),
            (entity.LeftLeg,  BodyParts.LeftLeg),
            (entity.RightLeg, BodyParts.RightLeg),
            (entity.Tail,     BodyParts.Tail),
        ];
        return peripheral.Where(l => l.limb.HitPoint <= 0).Select(l => l.part);
    }

    // ── Core attack action ───────────────────────────────────────────────────

    /// <summary>
    /// Resolves a single unarmed attack action between two entities.
    /// The full sequence is: opposed roll → location → damage roll → armour reduction
    /// → wound evaluation → limb detachment if maimed.
    /// </summary>
    /// <param name="attacker">The entity performing the attack.</param>
    /// <param name="defender">The entity receiving the attack.</param>
    /// <param name="targetLimb">If set, the attacker aims at a specific location at -20 to skill.</param>
    public static AttackResult PerformAttack(
        Entity attacker, Entity defender, BodyParts? targetLimb = null)
    {
        // Targeting a specific location incurs a -20 penalty to the attack skill.
        int attackSkill = CombatSkill(attacker) - (targetLimb.HasValue ? 20 : 0);
        int defendSkill = CombatSkill(defender);

        var (attackerWins, result) = ResolveOpposed(attackSkill, defendSkill);

        if (!attackerWins)
            return new AttackResult(result, null, 0, 0, 0, WoundLevel.None, false, false);

        BodyParts location = targetLimb ?? RollHitLocation(defender);
        Limb      hitLimb  = GetLimb(defender, location);

        // If the random location is already maimed, reroll once for a new location
        if (!targetLimb.HasValue && hitLimb.HitPoint <= 0)
        {
            location = RollHitLocation(defender);
            hitLimb  = GetLimb(defender, location);
        }

        int rawDamage = Math.Max(1, _rng.Next(1, 5) + DamageBonus(attacker)); // 1d4 unarmed base, minimum 1
        if (result == CombatResult.CriticalHit) rawDamage *= 2;   // criticals double damage

        int armor       = hitLimb.ArmorPoint;
        int finalDamage = Math.Max(0, rawDamage - armor); // armour cannot produce negative damage

        hitLimb.HitPoint -= finalDamage;

        WoundLevel wound = GetWoundLevel(hitLimb);
        bool limbMaimed  = wound == WoundLevel.Maimed && !IsVitalLocation(location);
        bool fatal       = wound == WoundLevel.Maimed &&  IsVitalLocation(location);

        // Peripheral limbs that reach 0 HP are physically severed.
        if (limbMaimed)
            defender.DetachLimb(location);

        return new AttackResult(result, location, rawDamage, armor, finalDamage,
                                wound, limbMaimed, fatal);
    }

    /// <summary>
    /// Resolves an attack action between two positioned participants.
    /// Returns <see langword="null"/> when the target is outside the attacker's reach.
    /// </summary>
    public static AttackResult? PerformAttack(
        CombatParticipant attacker, CombatParticipant target, BodyParts? targetLimb = null)
    {
        if (!CanReach(attacker.Entity, attacker.Position, target.Position))
            return null;

        return PerformAttack(attacker.Entity, target.Entity, targetLimb);
    }

    /// <summary>
    /// Runs one full round between two positioned participants, alternating action-by-action.
    /// An action is skipped (recorded as <see cref="CombatResult.Miss"/>) when the acting
    /// combatant is not in mutual reach of their opponent at that moment.
    /// Initiative (DEX + 1d10) determines who acts first; ties favour <paramref name="participantA"/>.
    /// The round ends early when either participant is incapacitated.
    /// </summary>
    public static IReadOnlyList<AttackResult> CombatRound(
        CombatParticipant participantA, CombatParticipant participantB)
    {
        int initA = RollInitiative(participantA.Entity);
        int initB = RollInitiative(participantB.Entity);

        // Higher initiative acts first; ties go to participantA.
        CombatParticipant first  = initA >= initB ? participantA : participantB;
        CombatParticipant second = initA >= initB ? participantB : participantA;

        int apFirst  = ActionPoints(first.Entity);
        int apSecond = ActionPoints(second.Entity);

        var results = new List<AttackResult>();
        int turn    = 0;

        while (apFirst > 0 || apSecond > 0)
        {
            bool firstActed = apFirst > 0 && (apSecond <= 0 || turn % 2 == 0);

            if (firstActed)
            {
                if (!IsIncapacitated(second.Entity))
                {
                    // Only attack if the acting combatant can reach their target.
                    AttackResult? r = PerformAttack(first, second);
                    if (r is not null) results.Add(r);
                }
                apFirst--;
            }
            else
            {
                if (!IsIncapacitated(first.Entity))
                {
                    AttackResult? r = PerformAttack(second, first);
                    if (r is not null) results.Add(r);
                }
                apSecond--;
            }

            turn++;
            if (IsIncapacitated(first.Entity) || IsIncapacitated(second.Entity)) break;
        }

        return results;
    }

    /// <summary>
    /// Runs one full round between two entities, alternating action-by-action.
    /// Initiative (DEX + 1d10) determines who acts first; ties favour <paramref name="entityA"/>.
    /// Both entities spend their Action Points interleaved — the entity with more AP
    /// gets to act again once the other's pool is exhausted.
    /// The round ends early when either entity is incapacitated.
    /// </summary>
    public static IReadOnlyList<AttackResult> CombatRound(Entity entityA, Entity entityB)
    {
        int initA = RollInitiative(entityA);
        int initB = RollInitiative(entityB);

        // Higher initiative acts first; ties go to entityA.
        Entity first  = initA >= initB ? entityA : entityB;
        Entity second = initA >= initB ? entityB : entityA;

        int apFirst  = ActionPoints(first);
        int apSecond = ActionPoints(second);

        var results = new List<AttackResult>();
        int turn    = 0; // alternation counter

        while (apFirst > 0 || apSecond > 0)
        {
            // Even turns: first entity acts (or whenever second has no AP left).
            bool firstActed = apFirst > 0 && (apSecond <= 0 || turn % 2 == 0);

            if (firstActed)
            {
                if (!IsIncapacitated(second))
                    results.Add(PerformAttack(first, second));
                apFirst--;
            }
            else
            {
                if (!IsIncapacitated(first))
                    results.Add(PerformAttack(second, first));
                apSecond--;
            }

            turn++;
            if (IsIncapacitated(first) || IsIncapacitated(second)) break;
        }

        return results;
    }

    /// <summary>
    /// Runs one full round for a single <paramref name="attacker"/> against multiple
    /// <paramref name="defenders"/>. Each action point is spent attacking the nearest
    /// reachable, living defender. Defenders do not counter-attack in this overload.
    /// </summary>
    /// <returns>
    /// A list of <c>(Target, Result)</c> pairs — one entry per action taken.
    /// Actions where no defender is in reach produce no entry.
    /// </returns>
    public static IReadOnlyList<(CombatParticipant Target, AttackResult Result)>
        CombatRound(CombatParticipant attacker, IReadOnlyList<CombatParticipant> defenders)
    {
        int ap      = ActionPoints(attacker.Entity);
        var results = new List<(CombatParticipant, AttackResult)>();

        for (int i = 0; i < ap; i++)
        {
            if (IsIncapacitated(attacker.Entity)) break;

            // Each AP: pick the nearest defender that is mutually in reach (both can reach each other).
            CombatParticipant? target = defenders
                .Where(d => !IsIncapacitated(d.Entity) &&
                             MutuallyInReach(attacker, d))
                .OrderBy(d => attacker.Position.DistanceTo(d.Position))
                .FirstOrDefault();

            if (target is null) break; // no reachable living targets remain

            AttackResult? result = PerformAttack(attacker, target);
            if (result is not null)
                results.Add((target, result));
        }

        return results;
    }
}
