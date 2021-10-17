using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptimizationAlgorithm:MonoBehaviour
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
    private List<Vector3Int> GetGrans(Vector3Int position)
    {
        List<Vector3Int> Grans = new List<Vector3Int>();
        if (!grid.Map.ContainsKey(new Vector3Int(position.x - 1, position.y, position.z))) Grans.Add(new Vector3Int(position.x - 1, position.y, position.z));
        if (!grid.Map.ContainsKey(new Vector3Int(position.x + 1, position.y, position.z))) Grans.Add(new Vector3Int(position.x + 1, position.y, position.z));
        if (!grid.Map.ContainsKey(new Vector3Int(position.x , position.y-1, position.z)) )Grans.Add(new Vector3Int(position.x, position.y - 1, position.z));
        if (!grid.Map.ContainsKey(new Vector3Int(position.x, position.y+1, position.z))) Grans.Add(new Vector3Int(position.x, position.y+1, position.z));
        return Grans;
    }
    public void Optimization(GridFunc grid)
    {
        Debug.Log("OPTIMIZE");
        this.grid = grid;
        if (grid.Map.Count == 0) grid.CreateNewTile(new Vector3Int(0, 0, 0), ThingsInCell.HousePeople);
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
        Debug.Log("Start optimization...");
        StartCoroutine("F");
    }
    IEnumerator F()
    {
        List<ThingsInCell> Variants = new List<ThingsInCell>() { ThingsInCell.HousePeople, ThingsInCell.HouseCom, ThingsInCell.HouseFact, ThingsInCell.RoadForCars };
        while (true)
        {
            foreach (CellWithHouse a in GranHouse)
            {
                foreach (Vector3Int b in GetGrans(a.GetCellPosition()))
                {
                    foreach (ThingsInCell c in Variants)
                    {
                        grid.CreateNewTile(b, c);
                        grid.StartSimmulation();
                        yield return new WaitForSeconds(2f);
                        grid.StopSimmulation();
                        grid.RemoveTileAt(b);
                    }
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
