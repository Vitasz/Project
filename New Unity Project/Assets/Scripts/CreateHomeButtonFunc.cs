using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CreateHomeButtonFunc : MonoBehaviour
{
    public GameObject AddPointButton, ConnectPointsButton, EndButton, NewHome;
    public GameObject PointHome, HouseLine, House, HouseController, DeletePointsButton, MovePointsButton;
    public EventSystem EvSys;
    public int Mode = -1; // 0 - Add, 1 - Connect, 2 - Delete, 3 - Move
    private readonly float PointRadius = 0.135f;
    private Vector3 NowPoint;
    private readonly List<Vector3> ClickedOnHousePoints = new List<Vector3>();
    private readonly List<Vector3> HousePoints = new List<Vector3>();
    private readonly Dictionary<Vector3, int> CountLinesOnPoint = new Dictionary<Vector3, int>();
    private readonly List<(Vector3, Vector3)> Lines = new List<(Vector3, Vector3)>();
    private readonly Dictionary<(Vector3, Vector3), GameObject> LinesGameobjects = new Dictionary<(Vector3, Vector3), GameObject>();
    private readonly Dictionary<Vector3, SpriteRenderer> PointsSprites = new Dictionary<Vector3, SpriteRenderer>();
    private readonly List<NewHouseLines> CrossedLines = new List<NewHouseLines>();
    private void Start()
    {
        transform.GetComponent<Button>().onClick.AddListener(OnClickCreateHomeButton);
        AddPointButton.GetComponent<Button>().onClick.AddListener(OnClickAddPointButton);
        ConnectPointsButton.GetComponent<Button>().onClick.AddListener(OnClickConnectPointsButton);
        EndButton.GetComponent<Button>().onClick.AddListener(OnClickEndButton);
        DeletePointsButton.GetComponent<Button>().onClick.AddListener(OnClickDeletePointsButton);
        MovePointsButton.GetComponent<Button>().onClick.AddListener(OnClickMovePointButton);
    }
    public void OnClickCreateHomeButton()
    {
        AddPointButton.gameObject.SetActive(true);
        ConnectPointsButton.gameObject.SetActive(true);
        EndButton.SetActive(true);
        DeletePointsButton.SetActive(true);
        MovePointsButton.SetActive(true);
    }
    public void OnClickAddPointButton() => Mode = 0;
    public void OnClickEndButton()
    {
        bool flag = true;
        foreach(int a in CountLinesOnPoint.Values)
        {
            if (a != 2)
            {
                flag = false;
                break;
            }
        }
        if (flag && HousePoints.Count>=3 && CrossedLines.Count == 0) CreateHouse();
        Restart();
    }
    public void OnClickDeletePointsButton() => Mode = 2;
    public void OnClickConnectPointsButton()
    {
        Mode = 1;
        ClickedOnHousePoints.Clear();
        if (PointsSprites.ContainsKey(NowPoint)) ClickedOnHousePoints.Add(NowPoint);
    }
    public void OnClickMovePointButton() => Mode = 3;
    private void Update()
    {
       if (Input.GetMouseButtonDown(0)  && !EvSys.IsPointerOverGameObject() && Mode==0)
       {
            Vector3 NewPointPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            NewPointPosition.z = 0;
            if (CountLinesOnPoint.ContainsKey(NewPointPosition)) return;
            HousePoints.Add(NewPointPosition);
            CreateNewPointforHome(NewPointPosition);
        }
    }
    /// <summary>
    /// Создание новой точки для дома
    /// </summary>
    /// <param name="Position"> Позиция</param>
    private void CreateNewPointforHome(Vector3 Position)
    {
        GameObject NewPoint = Instantiate(PointHome, NewHome.transform);
        NewPoint.transform.localPosition = Position;
        NewPoint.GetComponent<NewHousePointFunc>().NewHomeButton = this;
        NewPoint.GetComponent<NewHousePointFunc>().PositionPoint = Position;
        CountLinesOnPoint.Add(Position, 0);
        PointsSprites.Add(Position, NewPoint.GetComponent<SpriteRenderer>());
    }
    /// <summary>
    /// Событие клика на точку дома
    /// </summary>
    /// <param name="Position">Позиция точки</param>
    public void ClickOnPoint(Vector3 Position)
    {
        if (NowPoint != Position && PointsSprites.ContainsKey(NowPoint)) PointsSprites[NowPoint].color = Color.white;
        PointsSprites[Position].color = Color.green;
        NowPoint = Position;
        if (Mode == 1)
        {
            if (CountLinesOnPoint[Position] < 2 && !ClickedOnHousePoints.Contains(Position))
            {
                ClickedOnHousePoints.Add(Position);
                if (ClickedOnHousePoints.Count == 2)
                {
                    if (!LinesGameobjects.ContainsKey((ClickedOnHousePoints[0], ClickedOnHousePoints[1])) && 
                        !LinesGameobjects.ContainsKey((ClickedOnHousePoints[1], ClickedOnHousePoints[0])))
                        CreateLine(ClickedOnHousePoints);
                    PointsSprites[ClickedOnHousePoints[0]].color = Color.white;
                    ClickedOnHousePoints.RemoveAt(0);
                }
            }
        }
        else if (Mode == 2)
        {
            DeletePoint(Position);
        }
    }
    /// <summary>
    /// Создаёт линию дома по точкам
    /// </summary>
    /// <param name="Positions">Точки</param>
    private void CreateLine(List<Vector3> Positions)
    {
        GameObject NewLine = Instantiate(HouseLine, NewHome.transform);
        Lines.Add((Positions[0], Positions[1]));
        LinesGameobjects.Add((Positions[0], Positions[1]), NewLine);
        NewLine.GetComponent<NewHouseLines>().HomeButton = this;
        NewLine.GetComponent<NewHouseLines>().ChangePoints(Positions[0], Positions[1], PointRadius);
        CountLinesOnPoint[Positions[0]]++;
        CountLinesOnPoint[Positions[1]]++;
        PointsSprites[ClickedOnHousePoints[0]].color = Color.white;
    }
    /// <summary>
    /// Создаёт дом только при нажатии кнопки End
    /// </summary>
    private void CreateHouse()
    {
        GameObject NewHouse = Instantiate(House, HouseController.transform);
        List<Vector3> HousePointsToCreate = new List<Vector3>();
        Vector3 now = HousePoints[0];
        HousePoints.RemoveAt(0);
        HousePointsToCreate.Add(now);
        while (HousePoints.Count != 0)
        {
            (Vector3, Vector3) next = Lines.Find(x => x.Item1 == now && !HousePointsToCreate.Contains(x.Item2) || x.Item2 == now && !HousePointsToCreate.Contains(x.Item1));
            if (next.Item1 == now)
            {
                HousePointsToCreate.Add(next.Item2);
                now = next.Item2;
            }
            else
            {
                HousePointsToCreate.Add(next.Item1);
                now = next.Item1;
            }
            HousePoints.Remove(now);
        }
        NewHouse.GetComponent<HouseFunc>().CreateHouse(HousePointsToCreate.Select(v => new Vector2(v.x, v.y)).ToArray());
    }
    /// <summary>
    /// Удаление точки
    /// </summary>
    /// <param name="Position">Позиция точки</param>
    private void DeletePoint(Vector3 Position)
    {
        HousePoints.Remove(Position);
        ClickedOnHousePoints.Remove(Position);
        List<(Vector3, Vector3)> ToDelete = new List<(Vector3, Vector3)>();
        foreach((Vector3, Vector3) a in Lines)
        {
            if (a.Item1==Position || a.Item2 == Position)
            {
                Destroy(LinesGameobjects[a].gameObject);
                LinesGameobjects.Remove(a);
                CountLinesOnPoint[a.Item1]--;
                CountLinesOnPoint[a.Item2]--;
                ToDelete.Add(a);
            }
        }
        foreach ((Vector3, Vector3) a in ToDelete) Lines.Remove(a);
        CountLinesOnPoint.Remove(Position);
        Destroy(PointsSprites[Position].transform.gameObject);
        PointsSprites.Remove(Position);
    }
    public void MovePoint(Vector3 PositionPrev, Vector3 PositionNew)
    {
        if (CountLinesOnPoint.ContainsKey(PositionNew)) return;
        HousePoints[HousePoints.FindIndex(x => x == PositionPrev)] = PositionNew;
        CountLinesOnPoint.Add(PositionNew, CountLinesOnPoint[PositionPrev]); 
        CountLinesOnPoint.Remove(PositionPrev);
        List<(Vector3, Vector3)> ToChange = new List<(Vector3, Vector3)>();
        foreach((Vector3, Vector3) a in Lines)
        {
            if (a.Item2 == PositionPrev || a.Item1 == PositionPrev)
                ToChange.Add(a);
        }
        foreach((Vector3, Vector3) a in ToChange)
        {
            if (a.Item1 == PositionPrev)
            {
                (Vector3, Vector3) newA = (PositionNew, a.Item2);
                Lines.Add(newA); Lines.Remove(a);
                LinesGameobjects[newA] = LinesGameobjects[a];    
                LinesGameobjects.Remove(a);
                LinesGameobjects[newA].GetComponent<NewHouseLines>().ChangePoints(newA.Item1, newA.Item2, PointRadius);
            }
            else
            {
                (Vector3, Vector3) newA = (a.Item1, PositionNew);
                Lines.Add(newA); Lines.Remove(a);
                LinesGameobjects[newA] = LinesGameobjects[a];
                LinesGameobjects.Remove(a);
                LinesGameobjects[newA].GetComponent<NewHouseLines>().ChangePoints(newA.Item1, newA.Item2, PointRadius);
            }
        }
        PointsSprites[PositionNew] = PointsSprites[PositionPrev];
        PointsSprites.Remove(PositionPrev);
        if (PositionPrev==NowPoint) NowPoint = PositionNew;
        PointsSprites[PositionNew].GetComponent<NewHousePointFunc>().PositionPoint = PositionNew;
        PointsSprites[PositionNew].transform.localPosition = PositionNew;
    }
    /// <summary>
    /// Обнуление всех массивов и переменных
    /// </summary>
    private void Restart()
    {
        AddPointButton.SetActive(false);
        ConnectPointsButton.SetActive(false);
        EndButton.SetActive(false);
        DeletePointsButton.SetActive(false);
        MovePointsButton.SetActive(false);
        ClickedOnHousePoints.Clear();
        HousePoints.Clear();
        CountLinesOnPoint.Clear();
        PointsSprites.Clear();
        Mode = -1;
        for (int i = 0; i < NewHome.transform.childCount; i++)
            Destroy(NewHome.transform.GetChild(i).gameObject);
    }
    public void OnLineCroosed(NewHouseLines Linecrossed) {
        if (!CrossedLines.Contains(Linecrossed)) CrossedLines.Add(Linecrossed);
    }
    public void OnLineExist(NewHouseLines Linecrossed)
    {
        CrossedLines.Remove(Linecrossed);
    }
}

