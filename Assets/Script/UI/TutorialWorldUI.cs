using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialWorldUI : MonoBehaviour
{
    [SerializeField] GameModeTextData tutorialTextData;
    private int currentTutoIndex;

    [SerializeField] TextMeshProUGUI tutorialText;
    [SerializeField] TextMeshProUGUI indexText;

    public void InitializeTuto()
    {
        currentTutoIndex = 1;
        SetTutoText(currentTutoIndex);
        SetIndexText();
    }

    private void SetIndexText()
    {
        indexText.text = currentTutoIndex.ToString() + " / 4";
    }

    private void SetTutoText(int currentTutoIndex)
    {
        switch(currentTutoIndex)
        {
            case 1:
                tutorialText.text = tutorialTextData.player1Text;
                break;
            case 2:
                tutorialText.text = tutorialTextData.player2Text;
                break;
            case 3:
                tutorialText.text = tutorialTextData.AIText;
                break;
            case 4:
                tutorialText.text = tutorialTextData.myText;
                break;
        }
    }

    public void SetNextTuto()
    {
        currentTutoIndex++;
        if (currentTutoIndex == 5) currentTutoIndex = 1;
        SetTutoText(currentTutoIndex);
        SetIndexText();
    }

    public void SetPrevTuto()
    {
        currentTutoIndex--;
        if (currentTutoIndex == 0) currentTutoIndex = 4;
        SetTutoText(currentTutoIndex);
        SetIndexText();
    }
}
