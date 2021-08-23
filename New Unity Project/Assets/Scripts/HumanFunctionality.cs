using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFunctionality : MonoBehaviour
{
    List<(int, int)> way;
    int nowposition = 1;
    GameObject end;
    GridFunc Grid;
    public void StartGo(List<(int, int)> waytogo, GameObject EndHouse, GridFunc Grid)
    {
        transform.localPosition = Grid.PositionCell(waytogo[0].Item1, waytogo[0].Item2);
        way = waytogo;
        end = EndHouse;
        this.Grid = Grid;
        StartCoroutine("Go");
    }
    IEnumerator Go()
    {
        float progress = 0;
        while (true)
        {
            progress += Time.deltaTime;
            Vector2 from = Grid.PositionCell(way[nowposition - 1].Item1, way[nowposition - 1].Item2);
            Vector2 to = Grid.PositionCell(way[nowposition].Item1, way[nowposition].Item2);
            transform.localPosition = Vector3.Lerp(from, to, progress);
            if (transform.localPosition.x == to.x && transform.localPosition.y == to.y)
            {
                nowposition++;
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
