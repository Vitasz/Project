using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFunctionality : MonoBehaviour
{
    List<(int, int)> way;
    int nowpositionInWay = 1, nowpositionInCell = 0;
    GameObject end;
    GridFunc Grid;
    RoadsControlles Roads; 
    float speed = 4;
    public void StartGo(List<(int, int)> waytogo, GameObject EndHouse, GridFunc Grid, RoadsControlles roads)
    {
        this.Grid = Grid;
        Roads = roads;
        //Roads.Roads[waytogo[0]].HumanInTile();
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
    Vector3 PositionInCelltoCoords((int, int) position)
    {
        int SizeCell = Grid.SizeCell / 2 - 3;
        Vector3 ans = new Vector3();
        if (position.Item1 == 0)
        {
            if (position.Item2 < 2)
                ans.y = SizeCell * 2;
            else ans.y = SizeCell;
            if (position.Item2 % 2 == 0) ans.x = -SizeCell;
            else ans.x = SizeCell;
        }
        else if (position.Item1 == 1)
        {
            if (position.Item2 < 2)
                ans.x = SizeCell * 2;
            else ans.x = SizeCell;
            if (position.Item2 % 2 == 1) ans.y = -SizeCell;
            else ans.y = SizeCell;
        }
        else if (position.Item1 == 2)
        {
            if (position.Item2 < 2)
                ans.y = -SizeCell * 2;
            else ans.y = -SizeCell;
            if (position.Item2 % 2 == 1) ans.x = -SizeCell;
            else ans.x = SizeCell;
        }
        else
        {
            if (position.Item2 < 2)
                ans.x = -SizeCell * 2;
            else ans.x = -SizeCell;
            if (position.Item2 % 2 == 0) ans.y = -SizeCell;
            else ans.y = SizeCell;
        }
        return ans;
    }
    List<(int, int)> GetWayInCell(int previndex, int nextindex)
    {
        List<(int, int)> ans = new List<(int, int)>();
        ans.Add(((previndex / 2 + 2)%4, 0));
        ans.Add(((previndex / 2 + 2) % 4, 2));
        if (!ans.Contains((nextindex/2, 3)))ans.Add((nextindex / 2, 3));
        ans.Add((nextindex / 2, 1));
        return ans;
    }
    IEnumerator Go()
    {
        int previndex = -1, nowindex = GetIndex(way[0], way[1]), nextindex = -1;
        (int, int) prevposition = (-1, -1);
        int prevpositioninway = 0;
        if (way.Count > 2) nextindex = GetIndex(way[1], way[2]);
        List<(int, int)> WayInCell = new List<(int, int)>();
        bool canMove = false;
        Vector3 from = transform.localPosition, to = new Vector3();
        float progress = 0;
        while (true)
        {
            if (!canMove)
            {
                if (nowpositionInCell == WayInCell.Count)
                {
                    nowpositionInWay++;
                    nowpositionInCell = 0;
                    previndex = nowindex;
                    if (nowpositionInWay != way.Count) nowindex = GetIndex(way[nowpositionInWay - 1], way[nowpositionInWay]);
                    if (way.Count > nowpositionInWay)
                    {
                        //foreach ((int, int) a in WayInCell) Roads.Roads[way[nowpositionInWay - 2]].SetHumanInCell(a, false);
                        WayInCell = GetWayInCell(previndex, nowindex);
                    }
                }
                if (nowpositionInWay == way.Count)
                {
                    if (prevposition != (-1, -1)) Roads.Roads[way[prevpositioninway]].SetHumanInCell(prevposition, false);
                    end.GetComponent<HouseFunctionality>().GetHuman();
                    Destroy(gameObject);
                    yield return null;
                }
                if (Roads.Roads[way[nowpositionInWay-1]].CanMove(WayInCell[nowpositionInCell])) {
                    to = Grid.PositionCell(way[nowpositionInWay - 1]);
                    if (prevposition!=(-1,-1))Roads.Roads[way[prevpositioninway]].SetHumanInCell(prevposition, false);
                    Roads.Roads[way[nowpositionInWay-1]].SetHumanInCell(WayInCell[nowpositionInCell], true);
                    if (WayInCell.Count > nowpositionInCell)
                        to += PositionInCelltoCoords(WayInCell[nowpositionInCell]);
                    canMove = true;
                }
            }
            else{
                progress += Time.deltaTime * speed;
                transform.localPosition = Vector3.Lerp(from, to, progress);
                if (transform.localPosition == to)
                {
                    from = to;
                    prevpositioninway = nowpositionInWay-1;
                    prevposition = WayInCell[nowpositionInCell];
                    nowpositionInCell++;
                    canMove = false;
                    progress = 0;
                    continue;
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
