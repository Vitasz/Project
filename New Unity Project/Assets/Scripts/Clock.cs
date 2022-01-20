using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class Clock : MonoBehaviour
{
    Text TextComponent;
    public GridFunc grid;
    public double efficiency = 0;
    private void Start()
    {
        TextComponent = GetComponent<Text>();
        StartCoroutine("Timer");
    }
    private IEnumerator Timer()
    {
        while (true)
        {
            int seconds = Mathf.CeilToInt(Time.time);
            TextComponent.text = "Time: " + Convert.ToString(seconds / 3600) + ':' + Convert.ToString(seconds % 3600 / 60) + ':' + Convert.ToString(seconds % 60) + " Efficiency: " + Convert.ToString(Math.Round(efficiency, 3));
            //UpdateWaitTime();
            yield return new WaitForEndOfFrame();
        }
    }
    public void UpdateWaitTime()
    {
        double totalwaittime = 0;
        foreach(CellWithRoad a in grid.Roads.Values)
        {
            totalwaittime+=a.UpdateWaitTime();
        }
        efficiency = grid.Roads.Count / totalwaittime;
    }
}
