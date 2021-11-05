using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class Clock : MonoBehaviour
{
    Text TextComponent;
    public int totalWays, totalHumans, totalTimes;
    public GridFunc grid;
    public float MaxEfficiency = -1;
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
            MaxEfficiency = Math.Max(MaxEfficiency, UpdateEfficiency());
            TextComponent.text = "Time: " + Convert.ToString(seconds / 3600) + ':' + Convert.ToString(seconds % 3600 / 60) + ':' + Convert.ToString(seconds % 60) + " Efficiency: " + Convert.ToString(GetEfficiencyForOA());
            //UpdateWaitTime();
            yield return new WaitForEndOfFrame();
        }
    }
    public float UpdateEfficiency()
    {
        if (totalHumans < 0) Debug.LogError("HUMANS");
        else if (totalWays < 0) Debug.LogError("WAYS");
        if (totalHumans != 0) return (float)totalWays / totalHumans;
        else return -1;
    }
    public void UpdateWaitTime()
    {
        foreach(CellWithRoad a in grid.Roads)
        {
            a.UpdateWaitTime();
        }
    }
    
    public double GetEfficiencyForOA()
    {
        return (double)totalTimes / totalHumans;
    }
}
