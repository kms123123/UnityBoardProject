using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] TurnUI turnUI;
    [SerializeField] StatusUI trueStatusUI;
    [SerializeField] StatusUI falseStatusUI;

    private void Start()
    {
        BoardManager.instance.OnMoveEnd += UIManager_OnMoveEnd;
        BoardManager.instance.OnPutPiece += UIManager_OnPutPiece;
        BoardManager.instance.OnDeletePieceStart += UIManager_OnDeletePieceStart;
        BoardManager.instance.OnDeletePieceEnd += UIManager_OnDeletePieceEnd;

        trueStatusUI.OpenPutImage();
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
                trueStatusUI.DisableAllImage();
                falseStatusUI.OpenMoveImage();
            }
            else
            {
                trueStatusUI.OpenMoveImage();
                falseStatusUI.DisableAllImage();
            }
        }
        else
        {
            Debug.LogWarning("This cannot be Happen");
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
