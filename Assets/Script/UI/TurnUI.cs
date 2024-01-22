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

    [SerializeField] GameModeTextData currentTextData;

    private void Start()
    {
        GameManager.Instance.OnTurnChanged += TurnUIManager_OnTurnChanged;
        InitTurnUI();
    }

    public void InitTurnUI()
    {
        turnNumberText.text = "Turn 1:";
        ChangeTurnTextByCurrentTurn();
        whosTurnText.color = GreenFontColor;
    }

    private void TurnUIManager_OnTurnChanged(object sender, System.EventArgs e)
    {
        turnNumberText.text = "Turn " + GameManager.Instance.turnNumbers.ToString() + ":";
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
