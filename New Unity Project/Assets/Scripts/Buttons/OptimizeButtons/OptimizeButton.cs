using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class OptimizeButton : MonoBehaviour
{
    public Button thisButton;
    public InputField deep, K, hods, maxVariantCount;
    public Toggle canrHouses, canrRoads;
    public GridFunc grid;
    int Deep, HODS, maxVarCount;
    float k;
    bool canRHouses, canRRoads;
    void Start()
    {
        thisButton.onClick.AddListener(Optimize);
    }
    private void Optimize()
    {
        Deep = Convert.ToInt32(deep.text); Deep = Math.Min(Deep, 5); Deep = Math.Max(Deep, 2);
        HODS = Convert.ToInt32(hods.text); HODS = Math.Max(HODS, 1);
        maxVarCount = Convert.ToInt32(maxVariantCount.text); maxVarCount = Math.Max(maxVarCount, 1);
        maxVarCount = Math.Min(maxVarCount, 5000);
        k = (float)Convert.ToDouble(K.text); k = Math.Max(1f, k);
        canRRoads = canrRoads.isOn;
        canRHouses = canrHouses.isOn;
        grid.Optimize(Deep, k, HODS, maxVarCount, canRHouses, canRRoads);
    }
}
