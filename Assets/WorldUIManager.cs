using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class WorldUIManager : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera introCamera;
    [SerializeField] CinemachineVirtualCamera localCamera;
    [SerializeField] CinemachineVirtualCamera tutoCamera;
    [SerializeField] CinemachineVirtualCamera onlineCamera;
    [SerializeField] CinemachineVirtualCamera waitingCamera;

    [SerializeField] GameObject mainCanvas;
    [SerializeField] GameObject localCanvas;
    [SerializeField] GameObject tutoCanvas;
    [SerializeField] GameObject onlineCanvas;
    [SerializeField] GameObject waitingCanvas;

    [SerializeField] Toggle playerFirstinPVE;

    private void Start()
    {
        introCamera.enabled = true;
        introCamera.Priority = 20;
    }

    public void StopAllWorldCameraAndCanvas()
    {
        introCamera.Priority = 1;
        localCamera.Priority = 1;
        waitingCamera.Priority = 1;
        onlineCamera.Priority = 1;
        waitingCamera.Priority = 1;
        mainCanvas.SetActive(false);
        localCanvas.SetActive(false);
        tutoCanvas.SetActive(false);
        onlineCanvas.SetActive(false);
        waitingCanvas.SetActive(false);
    }

    public void GoToMainCanvas()
    {
        StopAllWorldCameraAndCanvas();
        mainCanvas.SetActive(true);
        introCamera.Priority = 20;
    }

    public void GoToLocalCanvas()
    {
        StopAllWorldCameraAndCanvas();
        localCanvas.SetActive(true);
        localCamera.Priority = 20;
    }

    public void GoToTutoCanvas()
    {
        StopAllWorldCameraAndCanvas();
        tutoCanvas.SetActive(true);
        tutoCamera.Priority = 20;
    }

    public void GoToOnlineCanvas()
    {
        StopAllWorldCameraAndCanvas();
        onlineCanvas.SetActive(true);
        onlineCamera.Priority = 20;
    }

    public void GoToWaitingCanvas()
    {
        StopAllWorldCameraAndCanvas();
        waitingCanvas.SetActive(true);
        waitingCamera.Priority = 20;
    }

    public void PlayLocalPVE()
    {
        GameManager.Instance.state = EGameState.Start;
        GameManager.Instance.gameMode = EGameMode.PVE;
        GameManager.Instance.isAiMode = true;
        if(!playerFirstinPVE.isOn)
        {
            BoardManager.instance.isAiMove = true;
        }
        else
        {
            BoardManager.instance.isAiMove = false;
        }
        StopAllWorldCameraAndCanvas();
    }

    public void PlayLocalPVP()
    {
        GameManager.Instance.state = EGameState.Start;
        GameManager.Instance.gameMode = EGameMode.PVPLocal;
        GameManager.Instance.isAiMode = false;
        BoardManager.instance.isAiMove = false;
        StopAllWorldCameraAndCanvas();
    }
}
