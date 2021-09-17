using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.EventSystems ;
using UnityEngine.Tilemaps;
public class GridFunc : MonoBehaviour
{
    public EventSystem EventSystemManager;
    public int SizeX, SizeY, SizeCell;
    public HouseControlles houseControlles;
    public GameControlls GameController;
    public Material MaterialForLines;
    public Camera nowCamera;
    public Tilemap tilemap;
    public readonly float _linesWidth = 0.3f;
    private Dictionary<Vector3Int, Cell> Map = new Dictionary<Vector3Int, Cell>();
    private bool isRedactorActive = false;
    private int ModeRedactor = 0;
    Vector3Int prevpositionClick = new Vector3Int(), nowpositionClick = new Vector3Int();
    bool hasfirstclick = false;
    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
        {
            //Debug.Log(isRedactorActive);
            if (isRedactorActive && tilemap.WorldToCell(nowCamera.ScreenToWorldPoint(Input.mousePosition)) != prevpositionClick)
            {
                prevpositionClick = nowpositionClick;
                nowpositionClick = tilemap.WorldToCell(nowCamera.ScreenToWorldPoint(Input.mousePosition));
                if (hasfirstclick && Mathf.Abs(prevpositionClick.x - nowpositionClick.x) + Mathf.Abs(prevpositionClick.y - nowpositionClick.y)<=1)
                {
                    if (!Map.ContainsKey(prevpositionClick)) Map.Add(prevpositionClick, new Cell(this, houseControlles, prevpositionClick));
                    if (!Map.ContainsKey(nowpositionClick)) Map.Add(nowpositionClick, new Cell(this, houseControlles, nowpositionClick));
                    Map[prevpositionClick].AddRoad(prevpositionClick, nowpositionClick, ModeRedactor, Map[nowpositionClick]);
                    Map[prevpositionClick].UpdateTile(); Map[nowpositionClick].UpdateTile();
                    //Debug.Log("Add road from " + Convert.ToString(prevpositionClick) + " to " + Convert.ToString(nowpositionClick));
                }
                hasfirstclick = true;
            }
        }
    }
    public Vector3 PositionCell((int,int) cell)
    {
        Vector3 toret = new Vector3(0, 0,-100);
        toret.x = -(float)SizeX / 2 * SizeCell + (float)SizeCell / 2 + cell.Item1 * SizeCell;
        toret.y = -(float)SizeY / 2 * SizeCell + (float)SizeCell / 2 + cell.Item2 * SizeCell;
        return toret;
    }
    public void OpenRedactorCell() {
        isRedactorActive = true;
    }
    public void CloseRedactorCell()
    {
        isRedactorActive = false;
        hasfirstclick = false;
    }
    public void SetMode(ThingsInCell mode)
    {
        ModeRedactor = (int)mode;
    }
    public List<Vector3Int> FindWay(Vector3Int from, Vector3Int to)
    {
        SortedSet<Vector3Int> now = new SortedSet<Vector3Int>(), nownew = new SortedSet<Vector3Int>();
        now.Add(from);
        Dictionary<Vector3Int, int> timetoRoads = new Dictionary<Vector3Int, int>();
        timetoRoads.Add(from, 0);
        int timer = 0;
        while (timer < 1000)
        {
            timer++;
            foreach(Vector3Int a in now)
            {
                if (a == to) break;
                foreach (Vector3Int b in Map[a].GetNearRoadsWays())
                {
                    nownew.Add(b);
                    if (!timetoRoads.ContainsKey(b)) timetoRoads[b] = timer;
                }
            }
            now = nownew;
            nownew.Clear();
        }
        List<Vector3Int> ans = new List<Vector3Int>();
        ans.Add(to);

        ans.Reverse();
        return ans;
    }
    public Cell GetCell(Vector3Int Position)
    {
        return Map[Position];
    }
}
