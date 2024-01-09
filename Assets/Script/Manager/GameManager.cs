using UnityEngine;

public class GameManager : MonoBehaviour
{

    public enum EGameState { Ready, Start, Putting, Move, Delete, Finish }

    public static GameManager Instance;

    public bool turn { get; private set; }

    public EGameState state { get; private set; }

    public int turnTrueHavetoPut = 9;
    public int turnFalseHavetoPut = 9;
    public int totalTruePiece = 0;
    public int totalFalsePiece = 0;
    public int turnTrue3PiecesMoves = 0;
    public int turnFalse3PiecesMoves = 0;

    public bool isAiMode;

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;

        turn = true;
    }

    private void Start()
    {
        BoardManager.instance.OnPutPiece += GameManager_OnPutPiece;
        BoardManager.instance.OnDeletePieceEnd += GameManager_OnDeletePieceEnd;
        BoardManager.instance.OnMoveEnd += GameManager_OnMoveEnd;

        state = EGameState.Ready;
    }

    private void GameManager_OnMoveEnd(object sender, System.EventArgs e)
    {
        //Move가 끝날 시 3피스 상태였다면, 움직임 횟수 추가
        if (turn && IsTurnTrueHas3Pieces()) turnTrue3PiecesMoves++;
        else if (!turn && IsTurnFalseHas3Pieces()) turnFalse3PiecesMoves++;

        if (!IsGameOver())
        {
            ChangeTurn();
            BoardManager.instance.moveState = BoardManager.MoveState.BeforePick;
        }
        else
        {
            Debug.Log("GameOver!");
        }
    }

    private void GameManager_OnDeletePieceEnd(object sender, bool turn)
    {
        if (!IsGameOver())
        {
            //말 개수 감소
            if (turn) totalFalsePiece--;
            else totalTruePiece--;

            ChangeTurn();
            SetState(EGameState.Putting);
        }
        else
        {
            Debug.Log("GameOver!");
        }
    }

    private void Update()
    {
        switch (state)
        {
            case EGameState.Ready:
                Update_Ready();
                break;
            case EGameState.Start:
                Update_Start();
                break;
            case EGameState.Putting:
                Update_Putting();
                break;
            case EGameState.Move:

                break;
            case EGameState.Delete:
                break;
            case EGameState.Finish:
                break;
        }

        Debug.Log(state);
    }

    //Todo: Ready구현해야함
    public void Update_Ready()
    {
        state = EGameState.Start;
    }

    //Todo: Start구현해야함
    public void Update_Start()
    {
        state = EGameState.Putting;
    }

    public void Update_Putting()
    {
        if (!CanPutDown())
        {
            state = EGameState.Move;
            BoardManager.instance.moveState = BoardManager.MoveState.BeforePick;
        }
    }

    public bool CanPutDown()
    {
        if (BoardManager.instance.isAiCalculating) return true;

        return turnTrueHavetoPut > 0 || turnFalseHavetoPut > 0;
    }

    private void GameManager_OnPutPiece(object sender, System.EventArgs e)
    {
        if (turn)
        {
            turnTrueHavetoPut--;
            totalTruePiece++;
        }
        else
        {
            turnFalseHavetoPut--;
            totalFalsePiece++;
        }

        Debug.Log(turnTrueHavetoPut + " " + turnFalseHavetoPut);
    }

    private bool IsTurnTrueHas3Pieces()
    {
        return totalTruePiece == 3;
    }

    private bool IsTurnFalseHas3Pieces()
    {
        return totalFalsePiece == 3;
    }

    public bool Is3PieceMove()
    {
        if (turn) return IsTurnTrueHas3Pieces();
        else return IsTurnFalseHas3Pieces();
    }

    public bool IsTurnTrueDefeat()
    {
        if (turnTrueHavetoPut > 0) return false;

        if (totalTruePiece < 3) return true;

        if (BoardManager.instance.IsCantMove(true)) return true;

        if (IsOver10Moves(true)) return true;

        return false;
    }

    public bool IsTurnFalseDefeat()
    {
        if (turnFalseHavetoPut > 0) return false;

        if (totalFalsePiece < 3) return true;

        if (BoardManager.instance.IsCantMove(false)) return true;

        if (IsOver10Moves(false)) return true;

        return false;
    }

    public bool CheckWinner()
    {
        if (IsTurnTrueDefeat()) return false;
        else return true;
    }

    public bool IsGameOver()
    {
        return IsTurnTrueDefeat() || IsTurnFalseDefeat();
    }

    public void SetState(EGameState state)
    {
        this.state = state;
    }

    public void ChangeTurn()
    {
        turn = !turn;

        //vs Ai 모드라면 Ai의 통제권을 주던가 뺏음
        if (isAiMode)
        {
            if (BoardManager.instance.isAiMove)
            {
                BoardManager.instance.isAiMove = false;
            }
            else
            {
                BoardManager.instance.isAiMove = true;
            }
        }
    }

    public bool IsOver10Moves(bool turn)
    {
        if (turn) return turnTrue3PiecesMoves >= 10;
        else return turnFalse3PiecesMoves >= 10;
    }

    public void SetZeroOf3Moves(bool turn)
    {
        if (turn && IsTurnTrueHas3Pieces()) turnTrue3PiecesMoves = 0;
        else if (!turn && IsTurnFalseHas3Pieces()) turnFalse3PiecesMoves = 0;
    }
}
