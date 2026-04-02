using System.Data;
using System.Runtime.CompilerServices;
using Fleshgolem.Data;
using Fleshgolem.Entity;
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
                new BaseAttributes((int)row["Strength"], (int)row["Size"], (int)row["Dexterity"]),
                (Guid)row["Id"]
            );
            Limb limb = new(bodyPart);
            player.AttachLimb(limb);
        }

        return player;
    }

    static Entity CreateEnemy()
    {
        DataTable PartData = GameData.PopulateData();
        DataRow[] filteredRows = PartData.Select("Animal = 'human'");
        var enemy = new Entity("Human");

        foreach (DataRow row in filteredRows)
        {
            BodyPart bodyPart = new(
                (string)row["Name"],
                (BodyParts)row["Part"],
                new BaseAttributes((int)row["Strength"], (int)row["Size"], (int)row["Dexterity"]),
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
        Console.WriteLine($"Player Name: {player.Name}");
        Console.WriteLine($"Head: {player.Head.BodyPart.Name}, Strength: {player.Head.BodyPart.Attributes.Strength}, Size: {player.Head.BodyPart.Attributes.Size}, Dexterity: {player.Head.BodyPart.Attributes.Dexterity}");
        Console.WriteLine($"Chest: {player.Chest.BodyPart.Name}, Strength: {player.Chest.BodyPart.Attributes.Strength}, Size: {player.Chest.BodyPart.Attributes.Size}, Dexterity: {player.Chest.BodyPart.Attributes.Dexterity}");
        Console.WriteLine($"Abdomen: {player.Abdomen.BodyPart.Name}, Strength: {player.Abdomen.BodyPart.Attributes.Strength}, Size: {player.Abdomen.BodyPart.Attributes.Size}, Dexterity: {player.Abdomen.BodyPart.Attributes.Dexterity}");
        Console.WriteLine($"Left Arm: {player.LeftArm.BodyPart.Name}, Strength: {player.LeftArm.BodyPart.Attributes.Strength}, Size: {player.LeftArm.BodyPart.Attributes.Size}, Dexterity: {player.LeftArm.BodyPart.Attributes.Dexterity}");
        Console.WriteLine($"Right Arm: {player.RightArm.BodyPart.Name}, Strength: {player.RightArm.BodyPart.Attributes.Strength}, Size: {player.RightArm.BodyPart.Attributes.Size}, Dexterity: {player.RightArm.BodyPart.Attributes.Dexterity}");
        Console.WriteLine($"Left Leg: {player.LeftLeg.BodyPart.Name}, Strength: {player.LeftLeg.BodyPart.Attributes.Strength}, Size: {player.LeftLeg.BodyPart.Attributes.Size}, Dexterity: {player.LeftLeg.BodyPart.Attributes.Dexterity}");
        Console.WriteLine($"Right Leg: {player.RightLeg.BodyPart.Name}, Strength: {player.RightLeg.BodyPart.Attributes.Strength}, Size: {player.RightLeg.BodyPart.Attributes.Size}, Dexterity: {player.RightLeg.BodyPart.Attributes.Dexterity}");
        Console.WriteLine($"Tail: {player.Tail.BodyPart.Name}, Strength: {player.Tail.BodyPart.Attributes.Strength}, Size: {player.Tail.BodyPart.Attributes.Size}, Dexterity: {player.Tail.BodyPart.Attributes.Dexterity}");

        player.DetachLimb(BodyParts.Tail);
        Console.WriteLine($"Tail: {player.Tail.BodyPart.Name}, Strength: {player.Tail.BodyPart.Attributes.Strength}, Size: {player.Tail.BodyPart.Attributes.Size}, Dexterity: {player.Tail.BodyPart.Attributes.Dexterity}");

        Entity enemy = CreateEnemy();
        Console.WriteLine($"Enemy Name: {enemy.Name}");
        Console.WriteLine($"Head: {enemy.Head.BodyPart.Name}, Strength: {enemy.Head.BodyPart.Attributes.Strength}, Size: {enemy.Head.BodyPart.Attributes.Size}, Dexterity: {enemy.Head.BodyPart.Attributes.Dexterity}");
        Console.WriteLine($"Chest: {enemy.Chest.BodyPart.Name}, Strength: {enemy.Chest.BodyPart.Attributes.Strength}, Size: {enemy.Chest.BodyPart.Attributes.Size}, Dexterity: {enemy.Chest.BodyPart.Attributes.Dexterity}");
        Console.WriteLine($"Abdomen: {enemy.Abdomen.BodyPart.Name}, Strength: {enemy.Abdomen.BodyPart.Attributes.Strength}, Size: {enemy.Abdomen.BodyPart.Attributes.Size}, Dexterity: {enemy.Abdomen.BodyPart.Attributes.Dexterity}");
        Console.WriteLine($"Left Arm: {enemy.LeftArm.BodyPart.Name}, Strength: {enemy.LeftArm.BodyPart.Attributes.Strength}, Size: {enemy.LeftArm.BodyPart.Attributes.Size}, Dexterity: {enemy.LeftArm.BodyPart.Attributes.Dexterity}");
        Console.WriteLine($"Right Arm: {enemy.RightArm.BodyPart.Name}, Strength: {enemy.RightArm.BodyPart.Attributes.Strength}, Size: {enemy.RightArm.BodyPart.Attributes.Size}, Dexterity: {enemy.RightArm.BodyPart.Attributes.Dexterity}");
        Console.WriteLine($"Left Leg: {enemy.LeftLeg.BodyPart.Name}, Strength: {enemy.LeftLeg.BodyPart.Attributes.Strength}, Size: {enemy.LeftLeg.BodyPart.Attributes.Size}, Dexterity: {enemy.LeftLeg.BodyPart.Attributes.Dexterity}");
        Console.WriteLine($"Tail: {enemy.Tail.BodyPart.Name}, Strength: {enemy.Tail.BodyPart.Attributes.Strength}, Size: {enemy.Tail.BodyPart.Attributes.Size}, Dexterity: {enemy.Tail.BodyPart.Attributes.Dexterity}");

    }
}
