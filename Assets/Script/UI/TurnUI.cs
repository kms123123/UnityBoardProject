using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI turnNumberText;
    [SerializeField] TextMeshProUGUI whosTurnText;

    [SerializeField] Color GreenFontColor;
    [SerializeField] Color RedFontColor;

    [SerializeField] TurnTextData turnTextDataKor;
    TurnTextData currentTextData;

    private void Start()
    {
        GameManager.Instance.OnTurnChanged += TurnUIManager_OnTurnChanged;
        currentTextData = turnTextDataKor;
        InitTurnUI();
    }

    private void InitTurnUI()
    {
        turnNumberText.text = "1턴:";
        ChangeTurnTextByCurrentTurn();
    }

    private void TurnUIManager_OnTurnChanged(object sender, System.EventArgs e)
    {
        turnNumberText.text = GameManager.Instance.turnNumbers.ToString() + "턴:";
        ChangeTurnTextByCurrentTurn();
        ChangeFontColor();
    }

    private void ChangeTurnTextByCurrentTurn()
    {
        switch (GameManager.Instance.gameMode)
        {
            case GameManager.EGameMode.PVE:
                if (BoardManager.instance.isAiMove)
                {
                    whosTurnText.text = currentTextData.AIText;
                }
                else
                {
                    whosTurnText.text = currentTextData.myText;
                }
                break;
            case GameManager.EGameMode.PVPLocal:
                if (GameManager.Instance.turn)
                {
                    whosTurnText.text = currentTextData.player1Text;
                }
                else
                {
                    whosTurnText.text = currentTextData.player2Text;
                }
                break;
            case GameManager.EGameMode.PVPNet:
                //Todo: 네트워크 관련 로직 설정 후 텍스트 설정 필요
                break;
        }
    }

    private void ChangeFontColor()
    {
        if(whosTurnText.color != GreenFontColor) whosTurnText.color = GreenFontColor;
        else whosTurnText.color = RedFontColor;
    }
}
