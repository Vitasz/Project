using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridFunc : MonoBehaviour
{
    public int SizeX, SizeY, SizeCell;
    public GameControlls GameController;
    public Material MaterialForLines;
    public BoxCollider2D Collider;
    private readonly float _linesWidth = 0.3f;
    private void Start()
    {
        void CreateLine(Vector3[] Positions)
        {
            GameObject NewLineGrid = new GameObject();
            NewLineGrid.transform.parent = transform;
            LineRenderer NewLineGridLR = NewLineGrid.AddComponent<LineRenderer>();
            NewLineGridLR.startWidth = _linesWidth;
            NewLineGridLR.endWidth = _linesWidth;
            NewLineGridLR.startColor = Color.black;
            NewLineGridLR.endColor = Color.black;
            NewLineGridLR.material = MaterialForLines;
            NewLineGridLR.SetPositions(Positions);
        }
        for (int i = 0; i <= SizeX; i++)
        {
            Vector3[] Positions = new Vector3[] { new Vector3((float)i * SizeCell - (float)SizeX / 2 * SizeCell, (float)SizeY / 2 * SizeCell),
               new Vector3((float)i * SizeCell - (float)SizeX / 2 * SizeCell, -(float)SizeY / 2 * SizeCell)
               };
            CreateLine(Positions);
        }
        for (int i = 0; i <= SizeY; i++)
        {
            Vector3[] Positions = new Vector3[] { new Vector3((float)SizeX / 2 * SizeCell, (float)i * SizeCell - (float)SizeY / 2 * SizeCell),
               new Vector3(-(float)SizeX / 2 * SizeCell, (float)i * SizeCell - (float)SizeY / 2 * SizeCell)
               };
            CreateLine(Positions);
        }
        Collider.size = new Vector2(SizeX * SizeCell * 2, SizeY * SizeCell * 2);
    }
    private void OnMouseDown()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (SizeX % 2 == 0) worldPosition.x -= SizeCell / 2;
        if (SizeY % 2 == 0) worldPosition.y -= SizeCell / 2;
        GameController.ClickOnGrid(Mathf.RoundToInt(worldPosition.x / SizeCell) + SizeX/2, Mathf.RoundToInt(worldPosition.y / SizeCell) + SizeY / 2);
    }
}
