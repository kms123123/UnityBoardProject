﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public event EventHandler OnTurnChanged;
    public event EventHandler<bool> OnGameIsOver;

    public enum EGameState { Ready, Start, Putting, Move, Delete, Finish, Result }
    public enum EGameMode { PVPNet, PVPLocal, PVE }

    public static GameManager Instance;

    public bool turn { get; private set; }
    public int turnNumbers;

    public EGameState state { get; private set; }
    public EGameMode gameMode { get; private set; }

    public int turnTrueHavetoPut = 9;
    public int turnFalseHavetoPut = 9;
    public int totalTruePiece = 0;
    public int totalFalsePiece = 0;
    public int turnTrue3PiecesMoves = 0;
    public int turnFalse3PiecesMoves = 0;

    public bool isAiMode;

    public UserInfo player1UserInfo;
    public UserInfo player2UserInfo;

    public int testForTrueIndex;
    public int testForFalseIndex;

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
        gameMode = EGameMode.PVPLocal;
        turnNumbers = 1;
    }

    private void GameManager_OnMoveEnd(object sender, Node e)
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
            SetState(EGameState.Finish);
            Debug.Log("GameOver!");
        }
    }

    private void GameManager_OnDeletePieceEnd(object sender, DeletePieceEventArgs deleteArgs)
    {
        if (!IsGameOver())
        {
            //말 개수 감소
            if (turn) totalFalsePiece--;
            else totalTruePiece--;

            if(IsGameOver())
            {
                SetState(EGameState.Finish);
                Debug.Log("GameOver!");
                return;
            }

            ChangeTurn();
            SetState(EGameState.Putting);
        }
        else
        {
            SetState(EGameState.Finish);
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
                //게임 종료시 게임종료 델리게이트 실행
                OnGameIsOver?.Invoke(this, CheckWinner());
                SetState(EGameState.Result);
                break;
            case EGameState.Result:
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

    private void GameManager_OnPutPiece(object sender, Node e)
    {
        if(!IsGameOver())
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
        else
        {
            SetState(EGameState.Finish);
            Debug.Log("Game Over!");
        }
       
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
        turnNumbers++;

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

        //턴이 전환될때 선택가능 표식을 지운다.
        foreach(Node node in BoardManager.instance.gameBoard)
        {
            node.DisableSelectObj();
        }

        OnTurnChanged?.Invoke(this, EventArgs.Empty);
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

    public void RestartGame()
    {
        StartCoroutine(LoadInGameScene());
    }

    private IEnumerator LoadInGameScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("InGame");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void ExitGame()
    {
        Debug.Log("Exit!");
    }
}

