using System.Collections.Generic;
using RT.BigInteger;
using UnityEngine;

public class Board
{
    public readonly string id;
    public readonly BoardSpace[] spaces;
    public readonly List<Structure> structures;
    public Board(string ModuleName, int ModuleId)
    {
        string cols = "ABCDEFGHIJKL";
        List<MapTile> tiles = new List<MapTile> { new MapTile1(), new MapTile2(), new MapTile3(), new MapTile4(), new MapTile5(), new MapTile6() };
        spaces = new BoardSpace[108];
        BigInt code = 0;
        id = "";

        List<int> spaceIDs = new List<int>();
        structures = new List<Structure>();
        for (int i = 0; i < 6; i++)
        {
            int startCol = (i % 2) * 6, startRow = (i / 2) * 3;
            int mapTileIx = Random.Range(0, tiles.Count);
            code = (code * tiles.Count) + mapTileIx;
            BoardTile[] bt = tiles[mapTileIx].spaces;
            
            var randVal = Random.Range(0, 2);
            code = (code * 2) + randVal;
            switch (tiles[mapTileIx].id)
            {
                case 1:
                    id += "001";
                    break;
                case 2:
                    id += "010";
                    break;
                case 3:
                    id += "011";
                    break;
                case 4:
                    id += "100";
                    break;
                case 5:
                    id += "101";
                    break;
                case 6:
                    id += "110";
                    break;
            }
            tiles.RemoveAt(mapTileIx);
            id += randVal;
            if (randVal == 0)
            {
                for (int j = 0; j < bt.Length; j++)
                {
                    int col = startCol + (j % 6);
                    int row = startRow + (j / 6);
                    spaces[(row * 12) + col] = new BoardSpace(cols[col] + "" + (row + 1), bt[j].type, bt[j].territory);
                    spaceIDs.Add((row * 12) + col);
                }
            }
            else
            {
                for (int j = 0; j < bt.Length; j++)
                {
                    int col = startCol + (j % 6);
                    int row = startRow + (j / 6);
                    spaces[(row * 12) + col] = new BoardSpace(cols[col] + "" + (row + 1), bt[bt.Length - j - 1].type, bt[bt.Length - j - 1].territory);
                    spaceIDs.Add((row * 12) + col);
                }
            }
        }
        id = binToHex(id);

        spaceIDs.Shuffle();
        structures.Add(new Structure(StructureType.AbandonedShack, StructureColor.Red, spaces[spaceIDs[0]].id));
        structures.Add(new Structure(StructureType.StandingStone, StructureColor.Red, spaces[spaceIDs[1]].id));
        structures.Add(new Structure(StructureType.AbandonedShack, StructureColor.Yellow, spaces[spaceIDs[2]].id));
        structures.Add(new Structure(StructureType.StandingStone, StructureColor.Yellow, spaces[spaceIDs[3]].id));
        structures.Add(new Structure(StructureType.AbandonedShack, StructureColor.Blue, spaces[spaceIDs[4]].id));
        structures.Add(new Structure(StructureType.StandingStone, StructureColor.Blue, spaces[spaceIDs[5]].id));
        structures.Add(new Structure(StructureType.AbandonedShack, StructureColor.White, spaces[spaceIDs[6]].id));
        structures.Add(new Structure(StructureType.StandingStone, StructureColor.White, spaces[spaceIDs[7]].id));
        for (var i = 0; i < 8; i++)
        {
            id += structures[i].spaceName;
            code = (code * 108) + spaceIDs[i];
        }
        Debug.LogFormat("[{0} #{1}] Old Map Seed: {2}", ModuleName, ModuleId, id);
        id = "";
        var alphabet = "!#$%&*+-/123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ\\abcdefghijklmnopqrstuvwxyz|~";
        while (code > 0)
        {
            var q = code.DivideModulo(alphabet.Length);
            id += alphabet[(int) q.Remainder];
            code = q.Quotient;
        }
    }
    private string binToHex(string bin)
    {
        string hex = "";
        for (int i = 0; i < bin.Length; i += 4)
        {
            switch (bin.Substring(i, 4))
            {
                case "0000":
                    hex += "0";
                    break;
                case "0001":
                    hex += "1";
                    break;
                case "0010":
                    hex += "2";
                    break;
                case "0011":
                    hex += "3";
                    break;
                case "0100":
                    hex += "4";
                    break;
                case "0101":
                    hex += "5";
                    break;
                case "0110":
                    hex += "6";
                    break;
                case "0111":
                    hex += "7";
                    break;
                case "1000":
                    hex += "8";
                    break;
                case "1001":
                    hex += "9";
                    break;
                case "1010":
                    hex += "A";
                    break;
                case "1011":
                    hex += "B";
                    break;
                case "1100":
                    hex += "C";
                    break;
                case "1101":
                    hex += "D";
                    break;
                case "1110":
                    hex += "E";
                    break;
                case "1111":
                    hex += "F";
                    break;
            }
        }
        return hex;
    }
}
