using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] List<CinemachineVirtualCamera> cameraPoints;
    [SerializeField] CinemachineVirtualCamera topdownCameraPoint;
    int currentCameraIndex = 0;
    int totalCameraCounts;
    bool isTopDownOn = false;

    private void Start()
    {
        totalCameraCounts = cameraPoints.Count;

        GameManager.Instance.OnGameStart += CameraController_OnGameStart;
    }

    private void CameraController_OnGameStart(object sender, System.EventArgs e)
    {
        ResetCameraPriorities();
        currentCameraIndex = 0;
        CameraOn(currentCameraIndex);
    }

    private void Update()
    {
        if (GameManager.Instance.state == GameManager.EGameState.Ready) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isTopDownOn) return;

            currentCameraIndex--;
            if (currentCameraIndex < 0) currentCameraIndex = cameraPoints.Count - 1;

            ResetCameraPriorities();
            CameraOn(currentCameraIndex);
        }

        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (isTopDownOn) return;

            currentCameraIndex++;
            if (currentCameraIndex >= cameraPoints.Count) currentCameraIndex = 0;

            ResetCameraPriorities();
            CameraOn(currentCameraIndex);
        }

        else if(Input.GetKeyDown(KeyCode.O))
        {
            if(!isTopDownOn)
            {
                isTopDownOn = true;
                ResetCameraPriorities();
                TopDownCameraOn(currentCameraIndex);
            }

            else
            {
                isTopDownOn = false;
                ResetCameraPriorities();
                CameraOn(currentCameraIndex);
            }
        }
    }

    //모든 카메라의 우선순위를 초기화
    private void ResetCameraPriorities()
    {
        foreach(var camera in cameraPoints)
        {
            camera.Priority = 1;
        }

        topdownCameraPoint.Priority = 1;
    }

    private void CameraOn(int index)
    {
        cameraPoints[index].Priority = 10;
    }

    private void TopDownCameraOn(int index)
    {
        Transform topdownCamera = topdownCameraPoint.transform;

        switch(currentCameraIndex)
        {
            case 0:
                topdownCamera.rotation = Quaternion.Euler(90, 90, 0);
                break;
            case 1:
                topdownCamera.rotation = Quaternion.Euler(90, 0, 0);
                break;
            case 2:
                topdownCamera.rotation = Quaternion.Euler(90, 270, 0);
                break;
            case 3:
                topdownCamera.rotation = Quaternion.Euler(90, 180, 0);
                break;
        }

        topdownCameraPoint.Priority = 10;
    }
}
