using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
public class CellWithHouse:Cell
{
    public ThingsInCell typeHouse;
    public int HumanInCellHouse = 0;
    private int[] housefromCellOnIndex = new int[8];
    private List<Vector3Int> thisHouse = new List<Vector3Int>();
    public CellWithHouse(GridFunc grid, HouseControlles houseControlles, Vector3Int position, ThingsInCell type, bool ForOA) : base(grid, houseControlles, position) {
        houseControlles.AddHouse(position, type, this);
        typeHouse = type;
       if (!ForOA)UpdateTile();
        if (type == ThingsInCell.HousePeople)
        {
            HumanInCellHouse = 10;
            houseControlles.AddCellWithHumans(this);
        }
    }
    public void UniteHouse(Vector3Int from, Vector3Int to, bool fromthis)
    {

    }
    protected override void UpdateTile()
    {
        string name = "";
        foreach (int a in housefromCellOnIndex) name += a == 0 ? '0' : '1';
        grid.tilemap.SetTile(positioninTileMap, Resources.Load<Tile>("Tiles/Houses/basetile" + name));
        grid.tilemap.SetTileFlags(positioninTileMap, TileFlags.None);;
        if (typeHouse == ThingsInCell.HousePeople) grid.tilemap.SetColor(positioninTileMap, Color.green);
        if (typeHouse == ThingsInCell.HouseFact) grid.tilemap.SetColor(positioninTileMap, Color.yellow);
        if (typeHouse == ThingsInCell.HouseCom) grid.tilemap.SetColor(positioninTileMap, Color.blue);
    }
    public void AddHuman()
    {
        HumanInCellHouse++;
        if (HumanInCellHouse == 1) houseControlles.AddCellWithHumans(this);
    }
}
