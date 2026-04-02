using System.Data;
using System.Runtime.CompilerServices;
using Fleshgolem.Data;
using Fleshgolem.Entities;
using Fleshgolem.Ruleset;

class Program
{
    static Entity CreatePlayer()
    {

        DataTable PartData = GameData.PopulateData();
        DataRow[] filteredRows = PartData.Select("Animal = 'manticore'");
        var player = new Entity("Homunculus");

        foreach (DataRow row in filteredRows)
        {
            BodyPart bodyPart = new(
                (string)row["Name"],
                (BodyParts)row["Part"],
                new BaseAttributes((int)row["Strength"], (int)row["Constitution"], (int)row["Size"], (int)row["Dexterity"]),
                (Guid)row["Id"]
            );
            Limb limb = new(bodyPart);
            player.AttachLimb(limb);
        }

        return player;
    }

    static Entity CreateEnemy(string name)
    {
        DataTable PartData = GameData.PopulateData();
        DataRow[] filteredRows = PartData.Select("Animal = 'human'");
        var enemy = new Entity(name);

        foreach (DataRow row in filteredRows)
        {
            BodyPart bodyPart = new(
                (string)row["Name"],
                (BodyParts)row["Part"],
                new BaseAttributes((int)row["Strength"], (int)row["Constitution"], (int)row["Size"], (int)row["Dexterity"]),
                (Guid)row["Id"]
            );
            Limb limb = new(bodyPart);
            enemy.AttachLimb(limb);
        }

        return enemy;
    }
    static void Main(string[] args)
    {
        Entity player = CreatePlayer();
        Entity enemy1 = CreateEnemy("Human 1");
        Entity enemy2 = CreateEnemy("Human 2");

        player.InitializeHP();
        enemy1.InitializeHP();
        enemy2.InitializeHP();

        // Place combatants: player at origin, enemy1 close (within reach), enemy2 further away.
        var playerP = new CombatParticipant(player, new Position(0, 0));
        var enemy1P = new CombatParticipant(enemy1, new Position(2, 0));
        var enemy2P = new CombatParticipant(enemy2, new Position(6, 0));

        float reach = Combat.AttackReach(player);
        Console.WriteLine("=== Combat Setup ===");
        Console.WriteLine($"{player.Name} — reach: {reach} units | Skill:{Combat.CombatSkill(player)} AP:{Combat.ActionPoints(player)}");
        Console.WriteLine($"  {enemy1.Name} at distance {playerP.Position.DistanceTo(enemy1P.Position):F1} — {(reach >= playerP.Position.DistanceTo(enemy1P.Position) ? "IN REACH" : "OUT OF REACH")}");
        Console.WriteLine($"  {enemy2.Name} at distance {playerP.Position.DistanceTo(enemy2P.Position):F1} — {(reach >= playerP.Position.DistanceTo(enemy2P.Position) ? "IN REACH" : "OUT OF REACH")}");
        Console.WriteLine();

        var results = Combat.CombatRound(playerP, [enemy1P, enemy2P]);

        Console.WriteLine("=== Combat Round ===");
        foreach (var (target, r) in results)
        {
            if (r.Result is CombatResult.Miss or CombatResult.Fumble)
            {
                Console.WriteLine($"  vs {target.Entity.Name}: [{r.Result}] — no damage");
                continue;
            }
            Console.WriteLine($"  vs {target.Entity.Name}: [{r.Result}] Hit:{r.HitLocation,-10} | Raw:{r.RawDamage,3}  Armor:{r.ArmorMitigation,2}  Final:{r.FinalDamage,3}  Wound:{r.WoundCaused}{(r.LimbMaimed ? " — LIMB MAIMED" : "")}{(r.Fatal ? " — FATAL" : "")}");
        }

        Console.WriteLine();
        Console.WriteLine("=== Round End ===");
        Console.WriteLine($"{player.Name} incapacitated: {Combat.IsIncapacitated(player)}");
        Console.WriteLine($"{enemy1.Name} incapacitated: {Combat.IsIncapacitated(enemy1)}");
        Console.WriteLine($"{enemy2.Name} incapacitated: {Combat.IsIncapacitated(enemy2)}");

        foreach (var (entity, name) in new[] { (player, player.Name), (enemy1, enemy1.Name), (enemy2, enemy2.Name) })
        {
            var maimed = Combat.MaimedLimbs(entity).ToList();
            if (maimed.Count > 0)
                Console.WriteLine($"{name} maimed limbs: {string.Join(", ", maimed)}");

        }
    }
}
