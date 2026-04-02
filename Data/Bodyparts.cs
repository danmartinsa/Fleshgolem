using Fleshgolem.Ruleset;
using System.Data;
namespace Fleshgolem.Data
{

    public static class GameData
    {
        public static DataTable PopulateData()
        {
            DataTable BodyPartsDT = new("Bodyparts");
            DataColumn idColumn = new("Id", typeof(Guid));
            DataColumn animalColumn = new("Animal", typeof(string));
            DataColumn nameColumn = new("Name", typeof(string));
            DataColumn partColumn = new("Part", typeof(BodyParts));
            DataColumn strColumn = new("Strength", typeof(int));
            DataColumn szColumn = new("Size", typeof(int));
            DataColumn dexColumn = new("Dexterity", typeof(int));

            BodyPartsDT.Columns.Add(idColumn);
            BodyPartsDT.Columns.Add(animalColumn);
            BodyPartsDT.Columns.Add(nameColumn);
            BodyPartsDT.Columns.Add(partColumn);
            BodyPartsDT.Columns.Add(strColumn);
            BodyPartsDT.Columns.Add(szColumn);
            BodyPartsDT.Columns.Add(dexColumn);

            void AddRow(string animal, BodyParts part, int strength, int size, int dexterity)
            {
                DataRow row = BodyPartsDT.NewRow();
                row[idColumn] = Guid.NewGuid();
                row[animalColumn] = animal;
                row[nameColumn] = animal + "_" + part.ToString();
                row[partColumn] = part;
                row[strColumn] = strength;
                row[szColumn] = size;
                row[dexColumn] = dexterity;
                BodyPartsDT.Rows.Add(row);
            }


            AddRow("manticore", BodyParts.Head, 22, 25, 17);
            AddRow("manticore", BodyParts.Chest, 22, 25, 17);
            AddRow("manticore", BodyParts.Abdomen, 22, 25, 17);
            AddRow("manticore", BodyParts.LeftArm, 22, 25, 17);
            AddRow("manticore", BodyParts.RightArm, 22, 25, 17);
            AddRow("manticore", BodyParts.LeftLeg, 22, 25, 25);
            AddRow("manticore", BodyParts.RightLeg, 22, 25, 25);
            AddRow("manticore", BodyParts.Tail, 22, 25, 25);

            AddRow("human", BodyParts.Head, 12, 11, 11);
            AddRow("human", BodyParts.Chest, 12, 11, 11);
            AddRow("human", BodyParts.Abdomen, 12, 11, 11);
            AddRow("human", BodyParts.LeftArm, 12, 11, 11);
            AddRow("human", BodyParts.RightArm, 12, 11, 11);
            AddRow("human", BodyParts.LeftLeg, 12, 11, 11);
            AddRow("human", BodyParts.RightLeg, 12, 11, 11);

            return BodyPartsDT;
        }
    }
}
