﻿using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] TurnUI turnUI;
    [SerializeField] StatusUI trueStatusUI;
    [SerializeField] StatusUI falseStatusUI;
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] GameObject resultWindowUIObject;
    [SerializeField] GameModeTextData resultTextDataKor;

    private void Start()
    {
        BoardManager.instance.OnMoveEnd += UIManager_OnMoveEnd;
        BoardManager.instance.OnPutPiece += UIManager_OnPutPiece;
        BoardManager.instance.OnDeletePieceStart += UIManager_OnDeletePieceStart;
        BoardManager.instance.OnDeletePieceEnd += UIManager_OnDeletePieceEnd;
        GameManager.Instance.OnGameIsOver += UIManager_OnGameIsOver;

        trueStatusUI.OpenPutImage();
    }

    private void Update()
    {
        //결과창 켜질 시 천천히 페이드인하는 효과
        if(resultWindowUIObject.activeSelf)
        {
            resultWindowUIObject.GetComponent<CanvasGroup>().alpha += 2f * Time.deltaTime;
        }
    }

    private void UIManager_OnGameIsOver(object sender, bool turn)
    {
        resultWindowUIObject.SetActive(true);
        if(GameManager.Instance.gameMode == GameManager.EGameMode.PVPNet)
        {
            //Todo: 네트워크 로직 짠 이후 해야함
        }
        else if(GameManager.Instance.gameMode == GameManager.EGameMode.PVPLocal)
        {
            if (turn)
            {
                resultText.text = resultTextDataKor.player1Text;
            }
            else
            {
                resultText.text = resultTextDataKor.player2Text;
            }
        }
        else if(GameManager.Instance.gameMode == GameManager.EGameMode.PVE)
        {
            if(BoardManager.instance.isAiTurnTrue)
            {
                if (turn)
                {
                    resultText.text = resultTextDataKor.AIText;
                }
                else
                {
                    resultText.text = resultTextDataKor.myText;
                }
            }
            else
            {
                if (turn)
                {
                    resultText.text = resultTextDataKor.myText;
                }
                else
                {
                    resultText.text = resultTextDataKor.AIText;
                }
            }
        }
    }

    private void UIManager_OnDeletePieceEnd(object sender, DeletePieceEventArgs e)
    {

        if (GameManager.Instance.turn)
        {
            trueStatusUI.OpenPutImage();
            falseStatusUI.DisableAllImage();
        }
        else
        {
            trueStatusUI.DisableAllImage();
            falseStatusUI.OpenPutImage();
        }

        if (GameManager.Instance.turnTrueHavetoPut == 0 && GameManager.Instance.turnFalseHavetoPut == 0)
        {
            if (GameManager.Instance.turn)
            {
                trueStatusUI.OpenMoveImage();
                falseStatusUI.DisableAllImage();
            }
            else
            {
                trueStatusUI.DisableAllImage();
                falseStatusUI.OpenMoveImage();
            }
        }
    }

    private void UIManager_OnDeletePieceStart(object sender, Node e)
    {
        if (GameManager.Instance.turn)
        {
            trueStatusUI.OpenDeleteImage();
            falseStatusUI.DisableAllImage();
        }
        else
        {
            trueStatusUI.DisableAllImage();
            falseStatusUI.OpenDeleteImage();
        }
    }

    private void UIManager_OnPutPiece(object sender, Node e)
    {
        if (GameManager.Instance.turn)
        {
            trueStatusUI.DisableAllImage();
            falseStatusUI.OpenPutImage();

        }
        else
        {
            trueStatusUI.OpenPutImage();
            falseStatusUI.DisableAllImage();

            if (GameManager.Instance.turnTrueHavetoPut == 0 && GameManager.Instance.turnFalseHavetoPut == 0)
            {
                trueStatusUI.OpenMoveImage();
            }
        }
    }

    private void UIManager_OnMoveEnd(object sender, Node e)
    {
        if (GameManager.Instance.turn)
        {
            trueStatusUI.OpenMoveImage();
            falseStatusUI.DisableAllImage();
        }
        else
        {
            trueStatusUI.DisableAllImage();
            falseStatusUI.OpenMoveImage();
        }
    }
}
