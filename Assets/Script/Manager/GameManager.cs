using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public enum EGameState { Ready, Start, Putting, Move, Delete, Finish }

    public static GameManager Instance;

    public bool turn { get; private set; }

    public EGameState state { get; private set; }

    private int turnTrueHavetoPut = 9;
    private int turnFalseHavetoPut = 9;
    public int totalTruePiece = 0;
    public int totalFalsePiece = 0;    

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
        ChangeTurn();
        BoardManager.instance.moveState = BoardManager.MoveState.BeforePick;
    }

    private void GameManager_OnDeletePieceEnd(object sender, System.EventArgs e)
    {
        ChangeTurn();
        SetState(EGameState.Putting);
    }

    private void Update()
    {
        switch(state)
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
        if(!CanPutDown())
        {
            state = EGameState.Move;
            BoardManager.instance.moveState = BoardManager.MoveState.BeforePick;
        }
    }

    public bool CanPutDown()
    {
        return turnTrueHavetoPut > 0 || turnFalseHavetoPut > 0;
    }

    private void GameManager_OnPutPiece(object sender, System.EventArgs e)
    {
        if (turn) turnTrueHavetoPut--;
        else turnFalseHavetoPut--;

        Debug.Log(turnTrueHavetoPut + " " + turnFalseHavetoPut);
    }

    public void SetState(EGameState state)
    {
        this.state = state;
    }

    public void ChangeTurn()
    {
        turn = !turn;
    }
}
