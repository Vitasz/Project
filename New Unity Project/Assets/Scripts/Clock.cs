using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class Clock : MonoBehaviour
{
    Text TextComponent;
    public int totalWays, totalHumans;
    public GridFunc grid;
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
            TextComponent.text = "Time: " + Convert.ToString(seconds / 3600) + ':' + Convert.ToString(seconds % 3600 / 60) + ':' + Convert.ToString(seconds % 60) + " Efficiency: " + Convert.ToString(UpdateEfficiency());
            UpdateWaitTime();
            yield return new WaitForSecondsRealtime(1f);
        }
    }
    private float UpdateEfficiency()
    {
        if (totalHumans < 0) Debug.LogError("HUMANS");
        else if (totalWays < 0) Debug.LogError("WAYS");
        if (totalHumans != 0) return totalWays / totalHumans;
        else return -1;
    }
    private void UpdateWaitTime()
    {
        foreach(CellWithRoad a in grid.Roads)
        {
            a.UpdateWaitTime();
        }
    }
}
