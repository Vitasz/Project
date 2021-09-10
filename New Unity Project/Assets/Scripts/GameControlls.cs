using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
public enum Modes: int
{
    /// <summary>
    /// Движение камеры
    /// </summary>
    CameraMove,
    /// <summary>
    /// Редактор ячейки
    /// </summary>
    CellRedactor,

}
public class GameControlls : MonoBehaviour
{
    public int Mode = (int)Modes.CameraMove;
    public HouseControlles HouseController;
    public RoadsControlles RoadsController;
    public Button SelectRedactorCellsButton;
    public CameraFunc cameraScript;
    public GridFunc GridScript;
    private bool inCell = false;
    void Start()
    {
        void SelectRedactorMode()
        {
            if (Mode == (int)Modes.CellRedactor)
            {
                Mode = (int)Modes.CameraMove;
                GridScript.CloseRedactorCell();
                inCell = false;
            }
            else Mode = (int)Modes.CellRedactor;
        }
        SelectRedactorCellsButton.onClick.AddListener(SelectRedactorMode);
    }
    public void ClickOnGrid((int,int) Position)
    {
        if (Mode == (int)Modes.CellRedactor&&!inCell)
        {
            cameraScript.SetTargetCell(GridScript.PositionCell(Position));
            GridScript.OpenRedactorCell(Position);
            inCell = true;
        }
    }
}
