using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class GenerateButton : MonoBehaviour
{
    public Button thisButton;
    public InputField hods;
    public GridFunc grid;
    int HODS;
    void Start()
    {
        thisButton.onClick.AddListener(Generate);
    }
    private void Generate()
    {
        HODS = Convert.ToInt32(hods.text); HODS = Math.Max(HODS, 0);
        grid.GenerateCity(HODS);
    }
}
