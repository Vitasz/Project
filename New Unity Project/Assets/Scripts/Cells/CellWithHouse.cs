using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
public class CellWithHouse:Cell
{
    public ThingsInCell typeHouse;
    //public int HumanInCellHouse = 0;
    private int[] housefromCellOnIndex = new int[8];
    //private List<Vector3Int> thisHouse = new List<Vector3Int>();
    public CellWithHouse(GridFunc grid, HouseControlles houseControlles, Vector3Int position, ThingsInCell type) : base(grid, houseControlles, position,type) {
        houseControlles.AddHouse(position, type, this);
        typeHouse = type;
        UpdateTile();
        if (type == ThingsInCell.HousePeople)
        {
            //HumanInCellHouse = 2;
            houseControlles.AddCellWithHumans(this);
            //Debug.Log("add");
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
}
