using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HouseControlles : MonoBehaviour
{
    public GridFunc Grid;
    public GameObject HousePrefab;
    public void CreateHouse((int, int) from, (int, int) to)
    {
        if (Grid.TestSquare(from, to))
        {
            Grid.SetSquare(from, to, 1);
            GameObject NewHouse = Instantiate(HousePrefab, transform);
            NewHouse.transform.localScale = new Vector2((Math.Abs(to.Item1 - from.Item1)*Grid.SizeCell + Grid.SizeCell)*100, (Math.Abs(to.Item2 - from.Item2) * Grid.SizeCell + Grid.SizeCell)*100);
            NewHouse.transform.localPosition = (Grid.PositionCell(from.Item1, from.Item2) + Grid.PositionCell(to.Item1, to.Item2)) / 2;
        }
    }
}
