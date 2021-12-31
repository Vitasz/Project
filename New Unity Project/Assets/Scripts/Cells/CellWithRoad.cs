using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;
using COLORS_CONST;
using System.Linq;
public class CellWithRoad : Cell
{
    private List<(int, int)> roadsFromCell = new List<(int, int)>();
    private int[] roadsfromCellOnIndex = new int[4];
    private string name = "0000";
    protected bool isEmpty = true;
    List<float> EmptyLastFrames = new List<float>();
    public float WaitTime = 1f;
    private float humansInCelllast100frames = 0;
    private float humansNow = 0;
    public int throughput = 1;
    Dictionary<(float, float), List<(float, float)>> RoadsInCell = new Dictionary<(float, float), List<(float, float)>>();
    Dictionary<(float,float), HumanFunctionality> humansInCell = new Dictionary<(float, float), HumanFunctionality>();
    public CellWithRoad(GridFunc grid, (int, int) position, ThingsInCell type) : base(grid, position, type) {

        GetCellPosition();
        UpdateThroughPut(2);
    }
    private void UpdateThroughPut(int throughput)
    {
        this.throughput = throughput;
        RoadsInCell.Clear();
        float min = 0.25f, max = 0.75f;
        List<float> roadspositions = new List<float>();
        float delta = (max - min) / (2 * throughput);
        float now = 0;
        for (int i = 0; now<1f; i++)
        {
            roadspositions.Add(now + delta / 2);
            now += delta;
        }
        //Горизонтальное направление
        //НАЛЕВО
        for (int i = 2*throughput; i < 3*throughput; i++)
        {
            RoadsInCell.Add((roadspositions[0], roadspositions[i]), new List<(float, float)>() { (-1f, -1f) });
        }
        for (int i = 1; i < 4 * throughput; i++)
        {
            for (int j = 2*throughput; j < 3 * throughput; j++)
            {
                if (!RoadsInCell.ContainsKey((roadspositions[i], roadspositions[j]))) RoadsInCell.Add((roadspositions[i], roadspositions[j]), new List<(float, float)>());
                //ПЕРЕМЕЩЕНИЕ СПРАВА НАЛЕВО
                RoadsInCell[(roadspositions[i], roadspositions[j])].Add((roadspositions[i-1], roadspositions[j]));
                //ПЕРЕМЕЩЕНИЕ МЕЖДУ ПОЛОСАМИ
                if (j!=2*throughput) RoadsInCell[(roadspositions[i], roadspositions[j])].Add((roadspositions[i-1], roadspositions[j-1]));
                if (j+1!=3*throughput) RoadsInCell[(roadspositions[i], roadspositions[j])].Add((roadspositions[i-1], roadspositions[j+1]));
            }

        }
        //НАПРАВО
        for (int i = throughput; i < 2*throughput; i++)
        {
            RoadsInCell.Add((roadspositions[4*throughput-1], roadspositions[i]), new List<(float, float)>() { (-1f, -1f) });
        }
        for (int i = 4 * throughput-2; i >= 0; i--)
        {
            for (int j = throughput; j < 2 * throughput; j++)
            {
                if (!RoadsInCell.ContainsKey((roadspositions[i], roadspositions[j]))) RoadsInCell.Add((roadspositions[i], roadspositions[j]), new List<(float, float)>());
                //ПЕРЕМЕЩЕНИЕ СЛЕВА НАПРАВО
                RoadsInCell[(roadspositions[i], roadspositions[j])].Add((roadspositions[i + 1], roadspositions[j]));
                //ПЕРЕМЕЩЕНИЕ МЕЖДУ ПОЛОСАМИ
                if (j != throughput) RoadsInCell[(roadspositions[i], roadspositions[j])].Add((roadspositions[i+1], roadspositions[j-1]));
                if (j + 1 != 2 * throughput) RoadsInCell[(roadspositions[i], roadspositions[j])].Add((roadspositions[i+1], roadspositions[j+1]));
            }
        }
        //Вертикальное направление
        //НАВЕРХ
        for (int i = 2 * throughput; i < 3 * throughput; i++)
        {
            RoadsInCell.Add((roadspositions[i], roadspositions[4*throughput-1]), new List<(float, float)>() { (-1f, -1f) });
        }
        for (int i = 0; i < 4 * throughput-1; i++)
        {
            for (int j = 2 * throughput; j < 3 * throughput; j++)
            {
                if (!RoadsInCell.ContainsKey((roadspositions[j], roadspositions[i]))) RoadsInCell.Add((roadspositions[j], roadspositions[i]), new List<(float, float)>());
                //ПЕРЕМЕЩЕНИЕ СНИЗУ ВВЕРХ
                RoadsInCell[(roadspositions[j], roadspositions[i])].Add((roadspositions[j], roadspositions[i+1]));
                //ПЕРЕМЕЩЕНИЕ МЕЖДУ ПОЛОСАМИ
                if (j != 2 * throughput) RoadsInCell[(roadspositions[j], roadspositions[i])].Add((roadspositions[j-1], roadspositions[i+1]));
                if (j + 1 != 3 * throughput) RoadsInCell[(roadspositions[j], roadspositions[i])].Add((roadspositions[j+1], roadspositions[i+1]));
            }

        }
        //ВНИЗ
        for (int i = throughput; i < 2 * throughput; i++)
        {
            RoadsInCell.Add((roadspositions[i], roadspositions[0]), new List<(float, float)>() { (-1f, -1f) });
        }
        for (int i = 4 * throughput - 1; i >= 1; i--)
        {
            for (int j = throughput; j < 2 * throughput; j++)
            {
                if (!RoadsInCell.ContainsKey((roadspositions[j], roadspositions[i]))) RoadsInCell.Add((roadspositions[j], roadspositions[i]), new List<(float, float)>());
                //ПЕРЕМЕЩЕНИЕ СЛЕВА НАПРАВО
                RoadsInCell[(roadspositions[j], roadspositions[i])].Add((roadspositions[j], roadspositions[i-1]));
                //ПЕРЕМЕЩЕНИЕ МЕЖДУ ПОЛОСАМИ
                if (j != throughput) RoadsInCell[(roadspositions[j], roadspositions[i])].Add((roadspositions[j-1], roadspositions[i-1]));
                if (j + 1 != 2 * throughput) RoadsInCell[(roadspositions[j], roadspositions[i])].Add((roadspositions[j+1], roadspositions[i-1]));
            }
        }
        humansInCell.Clear();
        foreach((float,float) a in RoadsInCell.Keys)
        {
            humansInCell.Add((a.Item1 + positioninTileMap.Item1, a.Item2 + positioninTileMap.Item2), null);
        }
    }
    public void RemoveRoad((int, int) to)
    {
        if (roadsFromCell.Contains(to))
        {
            roadsFromCell.Remove(to);
            roadsfromCellOnIndex[GetIndexNearCell(to)] = 0;
            name = "";
            for (int i = 0; i < 4; i++)
            {
                if (roadsfromCellOnIndex[i] == 1) name += '1';
                else name += '0';
            }
        }
        UpdateTile();
    }
    public void AddRoad(List<(int, int)> roads)
    {
        foreach ((int, int) to in roads)
        {
            if (!roadsFromCell.Contains(to) && positioninTileMap != to && GetIndexNearCell(to) != -1)
            {
                roadsFromCell.Add(to);
                name = "";
                roadsfromCellOnIndex[GetIndexNearCell(to)] = 1;
                for (int i = 0; i < 4; i++)
                {
                    if (roadsfromCellOnIndex[i] == 1) name += '1';
                    else name += '0';
                }
            }
        }
        UpdateTile();
    }
    public List<Vector3> GetWayInTheCell((int,int) from, (int, int) to)
    {
        HashSet<(float, float)>CanBePositions(int index, bool start)
        {
            HashSet<(float, float)> ans = new HashSet<(float, float)>();
            float delta = 0.5f / (2 * throughput);
            if (index == 0)
            {
                if (start) {
                    for (int i = 1; i <= throughput; i++)
                        ans.Add((0.25f + delta / 2 + delta * (i - 1), 1f - delta / 2));
                }
                else
                {
                    for (int i = 1; i <= throughput; i++)
                        ans.Add((0.5f + delta / 2 + delta * (i - 1), 1f - delta / 2));
                }
            }
            else if (index == 1)
            {
                if (start)
                {
                    for (int i = 1; i <= throughput; i++)
                        ans.Add((1f - delta / 2, 0.5f + delta / 2 + delta * (i - 1) ));
                }
                else
                {
                    for (int i = 1; i <= throughput; i++)
                        ans.Add((1f - delta / 2, 0.25f + delta / 2 + delta * (i - 1)));
                }
            }
            else if (index == 2)
            {
                if (start)
                {
                    for (int i = 1; i <= throughput; i++)
                        ans.Add((0.5f + delta / 2+delta*(i-1), delta / 2));
                }
                else
                {
                    for (int i = 1; i <= throughput; i++)
                        ans.Add((0.25f + delta / 2 + delta * (i - 1), delta / 2));
                }
            }
            else if (index == 3)
            {
                if (start)
                {
                    for (int i = 1; i <= throughput; i++)
                        ans.Add((delta / 2, 0.25f + +delta / 2 + delta * (i - 1)));
                }
                else
                {
                    for (int i = 1; i <= throughput; i++)
                        ans.Add((delta / 2, 0.5f + delta / 2 + delta * (i - 1)));
                }
            }
            return ans;
        }
        int startindex = GetIndexNearCell(from), endindex = GetIndexNearCell(to);
        HashSet<(float, float)> CanBeStart = CanBePositions(startindex, true);
        HashSet<(float, float)> CanBeEnd = CanBePositions(endindex, false);
        Dictionary<(float, float),(float,float)> usedpositions = new Dictionary<(float, float), (float, float)>();
        HashSet<(float, float)> nowpositions = new HashSet<(float, float)>(), newpositionsPriority = new HashSet<(float, float)>(), newpositionsNoPriority=new HashSet<(float, float)>();
        foreach ((float, float) a in CanBeStart)
        {

            usedpositions.Add(a, (-1f, -1f));
            if (humansInCell[(a.Item1+positioninTileMap.Item1, a.Item2+positioninTileMap.Item2)]==null) nowpositions.Add(a);
            //Debug.Log(a);
        }
        if (nowpositions.Count == 0) nowpositions.Add(CanBeStart.First());
        //foreach ((float, float) a in CanBeEnd) Debug.Log(end0)
        (float, float) end = (-1f, -1f);
        while (nowpositions.Count != 0)
        {
            foreach((float, float)a in nowpositions)
            {
                if (CanBeEnd.Contains(a))
                {
                    end = a;
                    newpositionsPriority.Clear();
                    newpositionsNoPriority.Clear();
                    break;
                }
                foreach((float,float) b in RoadsInCell[a])
                {
                    if (!usedpositions.ContainsKey(b)&&b!=(-1f, -1f))
                    {
                        usedpositions.Add(b, a);
                        if (humansInCell[(b.Item1+positioninTileMap.Item1, b.Item2 + positioninTileMap.Item2)]==null)newpositionsPriority.Add(b);
                        else newpositionsNoPriority.Add(b);
                    }
                }
            }
            nowpositions.Clear();
            foreach ((float, float) a in newpositionsPriority) nowpositions.Add(a);
            foreach ((float, float) a in newpositionsNoPriority) nowpositions.Add(a);
            newpositionsPriority.Clear();
            newpositionsNoPriority.Clear();
        }
        //Debug.Log(end);
        List<Vector3> ans = new List<Vector3>();
        while (!CanBeStart.Contains(end))
        {
            ans.Add(new Vector3(positioninTileMap.Item1+end.Item1, positioninTileMap.Item2+end.Item2, 0));
            end = usedpositions[end];
        }
        ans.Add(new Vector3(positioninTileMap.Item1 + end.Item1, positioninTileMap.Item2 + end.Item2, 0));
        ans.Reverse();
        return ans;
    }
    public List<Vector3> GetWayFromPositionInTheCell((float, float)from, (int, int) to)
    {
        HashSet<(float, float)> CanBePositions(int index, bool start)
        {
            HashSet<(float, float)> ans = new HashSet<(float, float)>();
            float delta = 0.5f / (2 * throughput);
            if (index == 0)
            {
                if (start)
                {
                    for (int i = 1; i <= throughput; i++)
                        ans.Add((0.25f + delta / 2 + delta * (i - 1), 1f - delta / 2));
                }
                else
                {
                    for (int i = 1; i <= throughput; i++)
                        ans.Add((0.5f + delta / 2 + delta * (i - 1), 1f - delta / 2));
                }
            }
            else if (index == 1)
            {
                if (start)
                {
                    for (int i = 1; i <= throughput; i++)
                        ans.Add((1f - delta / 2, 0.5f + delta / 2 + delta * (i - 1)));
                }
                else
                {
                    for (int i = 1; i <= throughput; i++)
                        ans.Add((1f - delta / 2, 0.25f + delta / 2 + delta * (i - 1)));
                }
            }
            else if (index == 2)
            {
                if (start)
                {
                    for (int i = 1; i <= throughput; i++)
                        ans.Add((0.5f + delta / 2 + delta * (i - 1), delta / 2));
                }
                else
                {
                    for (int i = 1; i <= throughput; i++)
                        ans.Add((0.25f + delta / 2 + delta * (i - 1), delta / 2));
                }
            }
            else if (index == 3)
            {
                if (start)
                {
                    for (int i = 1; i <= throughput; i++)
                        ans.Add((delta / 2, 0.25f + +delta / 2 + delta * (i - 1)));
                }
                else
                {
                    for (int i = 1; i <= throughput; i++)
                        ans.Add((delta / 2, 0.5f + delta / 2 + delta * (i - 1)));
                }
            }
            return ans;
        }
        int endindex = GetIndexNearCell(to);
        HashSet<(float, float)> CanBeStart = new HashSet<(float, float)>();
        HashSet<(float, float)> CanBeEnd = CanBePositions(endindex, false);
        Dictionary<(float, float), (float, float)> usedpositions = new Dictionary<(float, float), (float, float)>();
        HashSet<(float, float)> nowpositions = new HashSet<(float, float)>(), newpositionsPriority = new HashSet<(float, float)>(), newpositionsNoPriority = new HashSet<(float, float)>();
        foreach ((float, float) a in RoadsInCell[(from.Item1 - positioninTileMap.Item1, from.Item2 - positioninTileMap.Item2)]) if (a!=(-1f,-1f))CanBeStart.Add(a);
        foreach ((float, float) a in CanBeStart)
        {
            usedpositions.Add(a, (-1f, -1f));
            if (humansInCell[(a.Item1 + positioninTileMap.Item1, a.Item2 + positioninTileMap.Item2)] == null) nowpositions.Add(a);
            //Debug.Log(a);
        }
        if (nowpositions.Count == 0) return null;
        //foreach ((float, float) a in CanBeEnd) Debug.Log(end0)
        (float, float) end = (-1f, -1f);
        int free=0;
        while (nowpositions.Count != 0)
        {
           
            foreach ((float, float) a in nowpositions)
            {
                if (CanBeEnd.Contains(a))
                {
                    end = a;
                    newpositionsPriority.Clear();
                    newpositionsNoPriority.Clear();
                    break;
                }
                foreach ((float, float) b in RoadsInCell[a])
                {
                    if (!usedpositions.ContainsKey(b) && b != (-1f, -1f))
                    {
                        usedpositions.Add(b, a);
                        if (humansInCell[(b.Item1 + positioninTileMap.Item1, b.Item2 + positioninTileMap.Item2)] == null)
                        {
                            newpositionsPriority.Add(b);
                            free++;
                        }
                        else newpositionsNoPriority.Add(b);
                    }
                }
            }
            if (free == 0) return null;
            nowpositions.Clear();
            foreach ((float, float) a in newpositionsPriority) nowpositions.Add(a);
            foreach ((float, float) a in newpositionsNoPriority) nowpositions.Add(a);
            newpositionsPriority.Clear();
            newpositionsNoPriority.Clear();
        }
        if (end == (-1f, -1f)) return null;
        List<Vector3> ans = new List<Vector3>();
        while (!CanBeStart.Contains(end))
        {
            ans.Add(new Vector3(positioninTileMap.Item1 + end.Item1, positioninTileMap.Item2 + end.Item2, 0));
            end = usedpositions[end];
        }
        ans.Add(new Vector3(positioninTileMap.Item1 + end.Item1, positioninTileMap.Item2 + end.Item2, 0));
        ans.Add(new Vector3(from.Item1, from.Item2, 0));
        ans.Reverse();
        return ans;
    }
    public List<(int,int)> GetNearRoadsWays()
    {
        List<(int, int)> ans = new List<(int, int)>();
        foreach ((int, int) a in roadsFromCell) ans.Add(a);
        return ans;
    }
    protected override void UpdateTile()
    {
        Vector3Int tmp = new Vector3Int(positioninTileMap.Item1, positioninTileMap.Item2, 1);
        grid.tilemap.SetTile(tmp, Resources.Load<Tile>("Tiles/Roads/" + name));
        grid.tilemap.SetTileFlags(tmp, TileFlags.None);
        Color color=new Color(0,0,0,1f);
        Color from, to;
        float percents = 0;
        if (WaitTime < 4f)
        {
            from = to = COLORS.ColorRoad1;
            percents = 0f;
        }
        else if (WaitTime < 8f)
        {
            from = COLORS.ColorRoad1;
            to = COLORS.ColorRoad2;
            percents = (WaitTime-4f)/4f;
        }
        else if (WaitTime < 12f)
        {
            from = COLORS.ColorRoad2;
            to = COLORS.ColorRoad3;
            percents = (WaitTime - 8f) / 4f;
        }
        else
        {
            from = to = COLORS.ColorRoad3;
            percents = 0f;
        }
        color.r = Mathf.Lerp(from.r, to.r, percents);
        color.g = Mathf.Lerp(from.g, to.g, percents);
        color.b = Mathf.Lerp(from.b, to.b, percents);
        grid.tilemap.SetColor(tmp, color);
    }
    public HumanFunctionality CanMove((float,float) position) => humansInCell[position];
    public void MoveToThis(HumanFunctionality who, (float, float) position)
    {
        humansInCell[position] = who;
        humansNow++;
    }
    public void MoveOutThis(HumanFunctionality who, (float, float) position)
    {
        if (who == humansInCell[position])
        {
            humansInCell[position] = null;
        }
        humansNow--;
    }
    public void UpdateWaitTime()
    {
        EmptyLastFrames.Add(humansNow / ((roadsFromCell.Count*2+4)*throughput));
        humansInCelllast100frames += humansNow / ((roadsFromCell.Count * 2 + 4) * throughput);
        if (EmptyLastFrames.Count > 15)
        {
            humansInCelllast100frames -= EmptyLastFrames[0];
            EmptyLastFrames.RemoveAt(0);
        }
        humansInCelllast100frames = Math.Max(0.00001f, humansInCelllast100frames);
        WaitTime = (float)(humansInCelllast100frames);
        UpdateTile();
    }
    
}
