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
    public Button SelectRedactorCellsButton, HouseChooseButton, RoadsChooseButton;
    public Button SelectHouseModePeople, SelectHouseModeCom, SelectHouseModeFact, SelectRoadModeDefolt;
    public GameObject Redactor, HousesChoose, RoadsChoose;
    public CameraFunc cameraScript;
    public GridFunc GridScript;
    void Start()
    {
        void SelectRedactorMode()
        {
            if (Mode == (int)Modes.CellRedactor)
            {
                Mode = (int)Modes.CameraMove;
                GridScript.CloseRedactorCell();
                SelectRedactorCellsButton.transform.GetChild(0).GetComponent<Text>().text = "Open cell Redactor";
                Redactor.SetActive(false);
            }
            else
            {
                Mode = (int)Modes.CellRedactor;
                GridScript.OpenRedactorCell();
                cameraScript.SetTargetCell();
                SelectRedactorCellsButton.transform.GetChild(0).GetComponent<Text>().text = "Close cell Redactor";
                Redactor.SetActive(true);
            }
        }
        void OnHousesChooseButtonClick()
        {
            SetActiveAllButtonsFalse();
            HousesChoose.SetActive(true);
        }
        void OnRoadsChooseButtonClick()
        {
            SetActiveAllButtonsFalse();
            RoadsChoose.SetActive(true);
        }
        void OnSelectHouseModePeopleButtonClick() => GridScript.SetMode(ThingsInCell.HousePeople);
        void OnSelectHouseModeComButtonClick() => GridScript.SetMode(ThingsInCell.HouseCom);
        void OnSelectHouseModeFactButtonClick() => GridScript.SetMode(ThingsInCell.HouseFact);
        void OnSelectRoadModeDefoltButtonClick() => GridScript.SetMode(ThingsInCell.RoadForCars);
        SelectRedactorCellsButton.onClick.AddListener(SelectRedactorMode);
        HouseChooseButton.onClick.AddListener(OnHousesChooseButtonClick);
        RoadsChooseButton.onClick.AddListener(OnRoadsChooseButtonClick);
        SelectHouseModePeople.onClick.AddListener(OnSelectHouseModePeopleButtonClick);
        SelectHouseModeCom.onClick.AddListener(OnSelectHouseModeComButtonClick);
        SelectHouseModeFact.onClick.AddListener(OnSelectHouseModeFactButtonClick);
        SelectRoadModeDefolt.onClick.AddListener(OnSelectRoadModeDefoltButtonClick);
    }
    private void SetActiveAllButtonsFalse()
    {
        HousesChoose.SetActive(false);
        RoadsChoose.SetActive(false);
    }
}
