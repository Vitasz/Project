using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

public class CellWithRoad : Cell
{
    private List<Vector3Int> roadsFromCell = new List<Vector3Int>();
    //private List<Vector3Int> CanMoveTo = new List<Vector3Int>();
    private int[] roadsfromCellOnIndex = new int[8];
    protected bool isEmpty = true;
    public CellWithRoad(GridFunc grid, HouseControlles houseControlles, Vector3Int position) : base(grid, houseControlles, position) { UpdateTile();}
    public void AddRoad(Vector3Int from, Vector3Int to, bool fromthissell)
    {
        if (!roadsFromCell.Contains(to) && from != to && GetIndexNearCell(from, to)!=-1)
        {
            if (fromthissell)
            {
                roadsFromCell.Add(to);
                //Debug.Log("Add");
            }
            roadsfromCellOnIndex[GetIndexNearCell(from, to)] = 1;
            UpdateTile();
            //CanMoveTo.Add(to);
            // grid.GetComponent<Tilemap>().SetTile(from, Resources.Load<Tile>("Tiles/Roads/basetile" + roadsnear));
            // grid.GetComponent<Tilemap>().SetTile(to, Resources.Load<Tile>("Tiles/Roads/basetile00000000"));
        }
    }
    protected override void UpdateTileWithNewNeighboors()
    {
        //SetTile();
    }

    public List<Vector3Int> GetNearRoadsWays()
    {
        return roadsFromCell;
    }
    protected override void UpdateTile()
    {
        string name = "";
        foreach (int a in roadsfromCellOnIndex) name += a==0?'0':'1';
        grid.tilemap.SetTile(positioninTileMap, Resources.Load<Tile>("Tiles/Roads/basetile" + name));
    }
    public bool CanMove() => isEmpty;
    public bool MoveToThis() => isEmpty = false;
    public bool MoveOutThis() => isEmpty = true;

}
