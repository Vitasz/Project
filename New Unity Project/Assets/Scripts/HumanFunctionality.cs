using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFunctionality : MonoBehaviour
{
    List<Vector3Int> way;
    CellWithHouse end;
    GridFunc grid;
    float speed = 4;
    public void StartGo(List<Vector3Int> waytogo, CellWithHouse EndHouse, GridFunc Grid, CellWithHouse FromHouse)
    {
        grid = Grid;
        way = waytogo;
        end = EndHouse;
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
            if (NowPositionInWay == way.Count-1)
            {

                prevCell.MoveOutThis();
                end.AddHuman();
                Destroy(gameObject);
                yield return null;
            }
            else if (from==to)
            {
                if (nextCell.CanMove())
                {
                    transform.GetComponent<SpriteRenderer>().color = Color.red;
                    progress = 0;
                    nextCell.MoveToThis();
                    if (prevCell != null) prevCell.MoveOutThis();
                    to = grid.tilemap.CellToWorld(nextCell.GetCellPosition());
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
                    if (NowPositionInWay != way.Count - 1)
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
