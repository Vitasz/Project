using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFunctionality : MonoBehaviour
{
    List<(int, int)> way;
    int nowposition = 1;
    GameObject end;
    GridFunc Grid;
    RoadsControlles Roads; 
    float speed = 2;
    public void StartGo(List<(int, int)> waytogo, GameObject EndHouse, GridFunc Grid, RoadsControlles roads)
    {
        this.Grid = Grid;
        Roads = roads;
        Roads.HumanInTile(waytogo[0]);
        transform.localPosition = GetEndPosition(GetIndex(waytogo[0], waytogo[1]), GetIndex(waytogo[0], waytogo[1]), Grid.PositionCell(waytogo[0]));
        way = waytogo;
        end = EndHouse;
        StartCoroutine("Go");
    }
    Vector2 GetEndPosition(int nowindex, int nextindex, Vector2 middle)
    {
        int SizeCell = Grid.SizeCell / 2 - 3;
        //Debug.Log(nowindex);
        if (nextindex == -1) return middle;
        else
        {
            if (nowindex == 1)//OK
            {
                //Debug.Log(nextindex);
                if (nextindex == 3) return new Vector2(middle.x + SizeCell, middle.y - SizeCell);
                else if (nextindex == 7) return new Vector2(middle.x + SizeCell, middle.y + SizeCell);
                else if (nextindex == 1) return new Vector2(middle.x + SizeCell, middle.y);
            }
            else if (nowindex == 3)//OK
            {
                if (nextindex == 1) return new Vector2(middle.x + SizeCell, middle.y - SizeCell);
                else if (nextindex == 5) return new Vector2(middle.x - SizeCell, middle.y - SizeCell);
                else if (nextindex == 3) return new Vector2(middle.x, middle.y - SizeCell);
            }
            else if (nowindex == 5)//OK
            {
                if (nextindex == 3) return new Vector2(middle.x - SizeCell, middle.y - SizeCell);
                else if (nextindex == 7) return new Vector2(middle.x - SizeCell, middle.y + SizeCell);
                else if (nextindex == 5) return new Vector2(middle.x - SizeCell, middle.y);
            }
            else//OK
            {
                if (nextindex == 1) return new Vector2(middle.x + SizeCell, middle.y + SizeCell);
                else if (nextindex == 7) return new Vector2(middle.x, middle.y + SizeCell);
                else return new Vector2(middle.x - SizeCell, middle.y + SizeCell);

            }
        }
        return new Vector2();
    }
    int GetIndex((int, int) from, (int, int) to)
    {
        if (from.Item1 - 1 == to.Item1 && from.Item2 + 1 == to.Item2) return 0;
        if (from.Item1 == to.Item1 && from.Item2 + 1 == to.Item2) return 1;
        if (from.Item1 + 1 == to.Item1 && from.Item2 + 1 == to.Item2) return 2;
        if (from.Item1 + 1 == to.Item1 && from.Item2 == to.Item2) return 3;
        if (from.Item1 + 1 == to.Item1 && from.Item2 - 1 == to.Item2) return 4;
        if (from.Item1 == to.Item1 && from.Item2 - 1 == to.Item2) return 5;
        if (from.Item1 - 1 == to.Item1 && from.Item2 - 1 == to.Item2) return 6;
        if (from.Item1 - 1 == to.Item1 && from.Item2 == to.Item2) return 7;
        return -1;
    }
    IEnumerator Go()
    {
        float progress = 0;
        
        Vector2 frompos = transform.localPosition, topos;
        int nowindexpos, nextindexpos = -1;
        nowindexpos = GetIndex(way[0], way[1]);
        if (way.Count > 2)
        {
            nextindexpos = GetIndex(way[1], way[2]);
            topos = GetEndPosition(nowindexpos, nextindexpos, Grid.PositionCell(way[nowposition]));
        }
        else topos = Grid.PositionCell(way[nowposition]);
        while (true)
        {
            progress += Time.deltaTime * speed;
            transform.localPosition = Vector3.Lerp(frompos, topos, progress);
            if (transform.localPosition.x == topos.x && transform.localPosition.y == topos.y)
            {
                Roads.HumanOutTile(way[nowposition-1]);
                nowposition++;   
                frompos = topos;
                if (nowposition < way.Count)
                {
                    Roads.HumanInTile(way[nowposition-1]);
                    nowindexpos = GetIndex(way[nowposition - 1], way[nowposition]);
                    if (nowposition < way.Count - 1)
                    {
                        nextindexpos = GetIndex(way[nowposition], way[nowposition + 1]);
                        topos = GetEndPosition(nowindexpos, nextindexpos, Grid.PositionCell(way[nowposition]));
                    }
                    else topos = Grid.PositionCell(way[nowposition]);
                }
                progress = 0;
            }
            if (nowposition == way.Count)
            {
                end.GetComponent<HouseFunctionality>().GetHuman();
                Destroy(gameObject);
                yield return null;
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
