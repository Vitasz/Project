using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public enum ThingsInCell : int
{
    Nothing,
    HousePeople,
    HouseCom,
    HouseFact,
    RoadForCars,
}
public class Cell
{
    private GridFunc grid;
    private Vector2 position;
    private (int, int) indexposition;
    private int countMiniCellsX=4, countMiniCellsY=4;
    private float sizeCell;
    private List<GameObject> linesminicells = new List<GameObject>();
    Dictionary<(int, int), Dictionary<(int, int), int>> RoadsInCell = new Dictionary<(int, int), Dictionary<(int, int), int>>();
    public Cell(GridFunc Grid, Vector2 PositionCell, (int,int) IndexPosition)
    {
        grid = Grid;
        position = PositionCell;
        sizeCell = Grid.SizeCell;
        indexposition = IndexPosition;
    }
    //List<House> HousesInCell = new List<House>();
    public void AddRoad((int,int) from, (int, int) to, int type)
    {
        if (!RoadsInCell.ContainsKey(from)) RoadsInCell.Add(from, new Dictionary<(int, int), int>());
        RoadsInCell[from].Add(to, type);
    }
    public void ShowMiniCells()
    {
        void CreateLine(Vector2 from,  Vector2 to)
        {
            GameObject newLine = new GameObject();
            LineRenderer newLineR = newLine.AddComponent<LineRenderer>();
            newLineR.material = grid.MaterialForLines;
            newLineR.startWidth = 0.2f;
            newLineR.endWidth = 0.2f;
            newLineR.startColor = Color.black;
            newLineR.endColor = Color.black;
            newLineR.SetPositions(new List<Vector3>() { from, to }.ToArray());
            linesminicells.Add(newLine);
        }
        float distancebetweenLinesX = sizeCell / countMiniCellsX, distancebetweenLinesY = sizeCell / countMiniCellsY;
        for (int i = -countMiniCellsX/2+1; i < countMiniCellsX/2; i++)
        {
            CreateLine(new Vector2(position.x + i * distancebetweenLinesX, position.y - sizeCell / 2), new Vector2(position.x + i * distancebetweenLinesX, position.y + sizeCell / 2));
        }
        for (int i = -countMiniCellsY / 2+1; i < countMiniCellsY / 2; i++)
        {
            CreateLine(new Vector2(position.x - sizeCell / 2, position.y + i * distancebetweenLinesY), new Vector2(position.x + sizeCell / 2, position.y + i * distancebetweenLinesY));
        }
    }
    public void StopShowMiniCells()
    {
        foreach (GameObject a in linesminicells) Object.Destroy(a.gameObject);
        linesminicells.Clear();
    }

}
