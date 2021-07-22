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
    public GameObject PointHome, HouseLine, House, HouseController, DeletePointsButton;
    public EventSystem EvSys;
    private int Mode = -1; // 0 - Add, 1 - Connect, 2 - Delete;
    private readonly float PointRadius = 0.135f;
    private readonly List<Vector3> ClickedOnHousePoints = new List<Vector3>();
    private readonly List<Vector3> HousePoints = new List<Vector3>();
    private readonly Dictionary<Vector3, int> CountLinesOnPoint = new Dictionary<Vector3, int>();
    private readonly Dictionary<Vector3, Vector3> Lines = new Dictionary<Vector3, Vector3>();
    private readonly Dictionary<(Vector3, Vector3), GameObject> LinesGameobjects = new Dictionary<(Vector3, Vector3), GameObject>();
    private readonly Dictionary<Vector3, SpriteRenderer> PointsSprites = new Dictionary<Vector3, SpriteRenderer>();
    private void Start()
    {
        transform.GetComponent<Button>().onClick.AddListener(OnClickCreateHomeButton);
        AddPointButton.GetComponent<Button>().onClick.AddListener(OnClickAddPointButton);
        ConnectPointsButton.GetComponent<Button>().onClick.AddListener(OnClickConnectPointsButton);
        EndButton.GetComponent<Button>().onClick.AddListener(OnClickEndButton);
        DeletePointsButton.GetComponent<Button>().onClick.AddListener(OnClickDeletePointsButton);
    }
    public void OnClickCreateHomeButton()
    {
        AddPointButton.gameObject.SetActive(true);
        ConnectPointsButton.gameObject.SetActive(true);
        EndButton.SetActive(true);
        DeletePointsButton.SetActive(true);
    }
    public void OnClickAddPointButton()
    {
        Mode = 0;
    }
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
        if (flag && HousePoints.Count>=3) CreateHouse();
        Restart();
    }
    public void OnClickDeletePointsButton()
    {
        Mode = 2;
    }
    public void OnClickConnectPointsButton()
    {
        Mode = 1;
    }
    private void Update()
    {
       if (Input.GetMouseButtonDown(0) && Mode == 0 && !EvSys.IsPointerOverGameObject())
       {
            Vector3 NewPointPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            NewPointPosition.z = 0;
            bool ok = true;
            foreach(Vector3 a in HousePoints)
            {
                if (Math.Sqrt(Math.Pow(a.x - NewPointPosition.x,2) + Math.Pow(a.y - NewPointPosition.y,2))<2*PointRadius)
                {
                    ok = false;
                    break;
                }
            }
            if (ok)
            {
                HousePoints.Add(NewPointPosition);
                CreateNewPointforHome(NewPointPosition);
            }
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
        if (Mode == 1)
        {
            if (CountLinesOnPoint[Position] < 2 && !ClickedOnHousePoints.Contains(Position))
            {
                ClickedOnHousePoints.Add(Position);
                if (ClickedOnHousePoints.Count == 2)
                {
                    if (LinesGameobjects.ContainsKey((ClickedOnHousePoints[0], ClickedOnHousePoints[1])) ||
                    LinesGameobjects.ContainsKey((ClickedOnHousePoints[1], ClickedOnHousePoints[0])))
                    {
                        PointsSprites[ClickedOnHousePoints[0]].color = Color.white;
                        PointsSprites[ClickedOnHousePoints[1]].color = Color.white;
                    }
                    else CreateLine(ClickedOnHousePoints);
                    ClickedOnHousePoints.Clear();
                }
            }
            else PointsSprites[Position].color = Color.white;
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
        if (!Lines.ContainsKey(Positions[1]))
        {
            Lines.Add(Positions[1], Positions[0]);
            LinesGameobjects.Add((Positions[1], Positions[0]), NewLine);
        }
        else
        {
            Lines.Add(Positions[0], Positions[1]);
            LinesGameobjects.Add((Positions[0], Positions[1]), NewLine);
        }
        NewLine.GetComponent<LineRenderer>().SetPositions(Positions.ToArray());
        CountLinesOnPoint[Positions[0]]++;
        CountLinesOnPoint[Positions[1]]++;
        PointsSprites[ClickedOnHousePoints[0]].color = Color.white;
        PointsSprites[ClickedOnHousePoints[1]].color = Color.white;
    }
    /// <summary>
    /// Создаёт дом только при нажатии кнопки End
    /// </summary>
    private void CreateHouse()
    {
        GameObject NewHouse = Instantiate(House, HouseController.transform);
        List<Vector3> HousePointsToCreate = new List<Vector3>();
        Vector3 start = HousePoints[0], now = Lines[start];
        int counter = 0;
        HousePointsToCreate.Add(start);
        while (now!=start)
        {
            HousePointsToCreate.Add(now);
            now = Lines[now];
            if (counter == 1000)
            {
                Debug.LogError("to many");
                break;
            }
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
        List<Vector3> ToDelete = new List<Vector3>();
        foreach(Vector3 a in Lines.Keys)
        {
            if (a==Position || Lines[a] == Position)
            {
                if (LinesGameobjects.ContainsKey((a, Lines[a])))
                {
                    Destroy(LinesGameobjects[(a, Lines[a])].gameObject);
                    LinesGameobjects.Remove((a, Lines[a]));
                    CountLinesOnPoint[Lines[a]]--;
                    CountLinesOnPoint[a]--;
                }
                else if (LinesGameobjects.ContainsKey((Lines[a], a)))
                {
                    Destroy(LinesGameobjects[(Lines[a], a)].gameObject);
                    LinesGameobjects.Remove((Lines[a], a));
                    CountLinesOnPoint[Lines[a]]--;
                    CountLinesOnPoint[a]--;
                }
                ToDelete.Add(a);
            }
        }
        foreach (Vector3 a in ToDelete) Lines.Remove(a);
        CountLinesOnPoint.Remove(Position);
        Destroy(PointsSprites[Position].transform.gameObject);
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
        ClickedOnHousePoints.Clear();
        HousePoints.Clear();
        CountLinesOnPoint.Clear();
        Mode = -1;
        for (int i = 0; i < NewHome.transform.childCount; i++)
            Destroy(NewHome.transform.GetChild(i).gameObject);
    }
}

