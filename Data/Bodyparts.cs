using Fleshgolem.Ruleset;
using System.Data;
namespace Fleshgolem.Data
{

    /// <summary>Static source of body-part data used to assemble creatures at startup.</summary>
    public static class GameData
    {
        /// <summary>
        /// Builds and returns an in-memory <see cref="DataTable"/> containing one row per
        /// body-part entry for every creature archetype (manticore, human, …).
        /// Columns: Id, Animal, Name, Part, Strength, Constitution, Size, Dexterity.
        /// </summary>
        public static DataTable PopulateData()
        {
            DataTable BodyPartsDT = new("Bodyparts");
            DataColumn idColumn = new("Id", typeof(Guid));
            DataColumn animalColumn = new("Animal", typeof(string));
            DataColumn nameColumn = new("Name", typeof(string));
            DataColumn partColumn = new("Part", typeof(BodyParts));
            DataColumn strColumn = new("Strength", typeof(int));
            DataColumn conColumn = new("Constitution", typeof(int));
            DataColumn szColumn = new("Size", typeof(int));
            DataColumn dexColumn = new("Dexterity", typeof(int));

            BodyPartsDT.Columns.Add(idColumn);
            BodyPartsDT.Columns.Add(animalColumn);
            BodyPartsDT.Columns.Add(nameColumn);
            BodyPartsDT.Columns.Add(partColumn);
            BodyPartsDT.Columns.Add(strColumn);
            BodyPartsDT.Columns.Add(conColumn);
            BodyPartsDT.Columns.Add(szColumn);
            BodyPartsDT.Columns.Add(dexColumn);

            // Helper that creates a fully populated row and appends it to the table.
            void AddRow(string animal, BodyParts part, int strength, int constitution, int size, int dexterity)
            {
                DataRow row = BodyPartsDT.NewRow();
                row[idColumn] = Guid.NewGuid();
                row[animalColumn] = animal;
                row[nameColumn] = animal + "_" + part.ToString();
                row[partColumn] = part;
                row[strColumn] = strength;
                row[conColumn] = constitution;
                row[szColumn] = size;
                row[dexColumn] = dexterity;
                BodyPartsDT.Rows.Add(row);
            }


            // ── Manticore — powerful leonine body with a spiked tail ───────────────
            AddRow("manticore", BodyParts.Head, 22, 20, 25, 17);
            AddRow("manticore", BodyParts.Chest, 22, 20, 25, 17);
            AddRow("manticore", BodyParts.Abdomen, 22, 20, 25, 17);
            AddRow("manticore", BodyParts.LeftArm, 22, 20, 25, 17);
            AddRow("manticore", BodyParts.RightArm, 22, 20, 25, 17);
            AddRow("manticore", BodyParts.LeftLeg, 22, 20, 25, 25);
            AddRow("manticore", BodyParts.RightLeg, 22, 20, 25, 25);
            AddRow("manticore", BodyParts.Tail, 22, 20, 25, 25);

            // ── Human — baseline humanoid without a tail ─────────────────────────
            AddRow("human", BodyParts.Head, 12, 11, 11, 11);
            AddRow("human", BodyParts.Chest, 12, 11, 11, 11);
            AddRow("human", BodyParts.Abdomen, 12, 11, 11, 11);
            AddRow("human", BodyParts.LeftArm, 12, 11, 11, 11);
            AddRow("human", BodyParts.RightArm, 12, 11, 11, 11);
            AddRow("human", BodyParts.LeftLeg, 12, 11, 11, 11);
            AddRow("human", BodyParts.RightLeg, 12, 11, 11, 11);

            return BodyPartsDT;
        }
    }
}
