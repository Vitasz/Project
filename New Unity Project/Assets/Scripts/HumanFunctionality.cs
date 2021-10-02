using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFunctionality : MonoBehaviour
{
    List<Vector3Int> way;
    CellWithHouse end;
    GridFunc grid;
    Clock clock;
    float speed = 4;
    public void StartGo(List<Vector3Int> waytogo, CellWithHouse EndHouse, GridFunc Grid, CellWithHouse FromHouse, Clock clock)
    {
        grid = Grid;
        way = waytogo;
        end = EndHouse;
        this.clock = clock;
        transform.localPosition = grid.tilemap.CellToWorld(FromHouse.GetCellPosition());
        StartCoroutine("Go");
    }
    IEnumerator Go()
    {
        int NowPositionInWay = -1;
        Vector3 from= transform.localPosition, to = transform.localPosition;
        float progress = 0;
        CellWithRoad nextCell = grid.GetCell(way[NowPositionInWay+1]) as CellWithRoad, prevCell = null;
        while (true)
        {
            if (NowPositionInWay >= way.Count-1 && from==to)
            {
                if (to != end.GetCellPosition())
                {
                    to = end.GetCellPosition();
                    prevCell.MoveOutThis();
                }
                else
                {
                    clock.totalHumans--;
                    clock.totalWays -= way.Count;
                    end.AddHuman();
                    Destroy(gameObject);
                    yield return null;
                }
            }
            else if (from==to)
            {
                if (nextCell.CanMove(this, NowPositionInWay == -1 ? nextCell.GetCellPosition() : way[NowPositionInWay]))
                {
                    transform.GetComponent<SpriteRenderer>().color = Color.red;
                    progress = 0;
                    nextCell.MoveToThis(this, way[NowPositionInWay + 1]);
                    if (prevCell != null) prevCell.MoveOutThis();
                    to = grid.tilemap.CellToWorld(nextCell.GetCellPosition());
                }
                else if (NowPositionInWay != -1 && prevCell.WaitTime>=10f)
                {
                    List<Vector3Int> newWay = grid.FindWay(new List<Vector3Int>() { prevCell.GetCellPosition() }, end.GetNearTiles());
                    if (newWay != null && nextCell.GetCellPosition() != newWay[1]) 
                    {
                        //newWay.Insert(0, prevCell.GetCellPosition());
                        NowPositionInWay = 0;
                        nextCell = grid.GetCell(newWay[NowPositionInWay+1]) as CellWithRoad;
                        clock.totalWays -= way.Count;
                        clock.totalWays += newWay.Count;
                        if (nextCell == null) Debug.LogError("ERROR");
                        way.Clear();
                        foreach (Vector3Int a in newWay) way.Add(a);
                        
                    }
                    else yield return new WaitForSeconds(1f);
                }
            }
            if (from != to)
            {
                progress += Time.deltaTime*speed;
                transform.localPosition = Vector3.Lerp(from, to, progress);
                if (progress >= 1f)
                {
                    NowPositionInWay++;
                    prevCell = nextCell;
                    if (NowPositionInWay < way.Count - 1)
                    {
                        nextCell = grid.GetCell(way[NowPositionInWay + 1]) as CellWithRoad;
                    }
                    from = to;
                    progress = 0;
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
