using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class CellWithHouse : Cell
{
    ThingsInCell typeHouse;
    public CellWithHouse(GridFunc grid, HouseControlles houseControlles, Vector3Int position, ThingsInCell type) : base(grid, houseControlles, position) {
        type = typeHouse;
    }
    public int HumanInCellHouse = 0;
    protected override void UpdateTileWithNewNeighboors()
    {

    }
    public void UpdateTile()
    {
       /* foreach (KeyValuePair<int, int[]> a in whatNearCells)
        {
            string temp = "";
            foreach (int b in a.Value) temp += Convert.ToString(b);
            grid.tilemap.SetTile(new Vector3Int(positioninTileMap.x, positioninTileMap.y, a.Key), Resources.Load<Tile>(ResourcesLoadedPaths[a.Key] + temp));
        }*/
    }
    private IEnumerator SpawnHuman()
    {
        while (true)
        {
            if (HumanInCellHouse != 0)
            {
                Cell HouseTo = houseControlles.GetRandomHouseCell(ThingsInCell.HouseCom);
                if (HouseTo != null)
                {
                    Vector3Int positionTo = HouseTo.GetCellPosition();
                    List<Vector3Int> way = grid.FindWay(positioninTileMap, positionTo);
                    if (way != null)
                    {
                        //Create Human
                    }
                }
            }
            yield return new WaitForSeconds(10);
        }
    }
}
