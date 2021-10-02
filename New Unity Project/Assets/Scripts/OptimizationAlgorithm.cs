using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptimizationAlgorithm
{
    List<CellWithHouse> GranHouse = new List<CellWithHouse>();
    List<CellWithRoad> GranRoad = new List<CellWithRoad>();
    GridFunc grid;
    private bool IsGran(Vector3Int position)
    {
        return !(grid.Map.ContainsKey(new Vector3Int(position.x - 1, position.y, position.z)) &&
            grid.Map.ContainsKey(new Vector3Int(position.x, position.y - 1, position.z)) &&
            grid.Map.ContainsKey(new Vector3Int(position.x + 1, position.y, position.z)) &&
            grid.Map.ContainsKey(new Vector3Int(position.x, position.y + 1, position.z)));
    }
    public void Optimization(GridFunc grid)
    {
        Debug.Log("OPTIMIZE");
        this.grid = grid;
        foreach(Vector3Int a in grid.Map.Keys)
        {
            if (IsGran(a))
            {
                if (grid.Map[a] is CellWithHouse) GranHouse.Add(grid.Map[a] as CellWithHouse);
                else if (grid.Map[a] is CellWithRoad) GranRoad.Add(grid.Map[a] as CellWithRoad);
            }
        }
        Debug.Log(GranHouse.Count);
        Debug.Log(GranRoad.Count);
    }

}
