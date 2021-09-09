using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraFunc : MonoBehaviour
{
    private bool isMouseDragging;
    Vector3 targetPosition=new Vector3(0,0,-100), startPosition;
    private float cameraSpeed = 10f, progressMove = 0;
    private float zoomSpeed = 5, zoomMultiplayer = 10, progressZoom;
    [Range(15, 100f)]
    private float nowZoom = 50, startZoom = 50;
    public GameControlls gameController;
    public Camera thisCamera;
    private void Update()
    {
        Vector3 MousePosition = thisCamera.ScreenToWorldPoint(Input.mousePosition);
        MousePosition.z = -100;
        //Camera Position
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()
            &&gameController.Mode==0)
        {
            targetPosition = MousePosition;
            startPosition = transform.position;
            progressMove = 0;
        }
        if (transform.position != targetPosition)
        {
            progressMove += Time.deltaTime * cameraSpeed;
            transform.position = Vector3.Lerp(startPosition, targetPosition, progressMove);
        }
        //Camera Zoom
        if (Input.mouseScrollDelta.y!=0)
        {
            nowZoom -= Input.mouseScrollDelta.y * zoomMultiplayer;
            startZoom = thisCamera.orthographicSize;
            progressZoom = 0;
        }
        if (nowZoom < 15) nowZoom = 15;
        if (thisCamera.orthographicSize != nowZoom)
        {
            progressZoom += Time.deltaTime * zoomSpeed;
            thisCamera.orthographicSize = Mathf.Lerp(startZoom, nowZoom, progressZoom);
        }
    }
}
