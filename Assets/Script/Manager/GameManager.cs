using System;
using System.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public event EventHandler OnTurnChanged;
    public event EventHandler<bool> OnGameIsOver;
    public event EventHandler OnGameStart;

    public Server server;
    public Client client;

    public enum EGameState { Ready, Start, Putting, Move, Delete, Finish, Result }
    public enum EGameMode { PVPNet, PVPLocal, PVE }

    public static GameManager Instance;

    public bool turn { get; private set; }
    public int turnNumbers;

    public EGameState state;
    public EGameMode gameMode;

    public int turnTrueHavetoPut = 9;
    public int turnFalseHavetoPut = 9;
    public int totalTruePiece = 0;
    public int totalFalsePiece = 0;
    public int turnTrue3PiecesMoves = 0;
    public int turnFalse3PiecesMoves = 0;

    public bool isAiMode;

    public int truePieceIndex;
    public int falsePieceIndex;

    // Multi Logic
    private int playerCount = -1;
    public int currentTeam = -1;
    private bool[] playerRematch = new bool[2];
    [SerializeField] GameObject rematchIndicatorObj;
    [SerializeField] Button rematchButton;

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;

        RegisterEvents();
    }

    private void Start()
    {
        BoardManager.instance.OnPutPiece += GameManager_OnPutPiece;
        BoardManager.instance.OnDeletePieceEnd += GameManager_OnDeletePieceEnd;
        BoardManager.instance.OnMoveEnd += GameManager_OnMoveEnd;

        state = EGameState.Ready;
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
        
    }

    public void Update_Start()
    {
        //턴정보 초기화
        turnNumbers = 1;
        turn = true;
        totalTruePiece = 0;
        totalFalsePiece = 0;
        turnTrueHavetoPut = 9;
        turnFalseHavetoPut = 9;
        turnTrue3PiecesMoves = 0;
        turnFalse3PiecesMoves = 0;
        playerRematch[0] = playerRematch[1] = false;

        //Todo: 보드초기화, UI 활성화 및 초기화 필요
        /**
         * BoardManager: 보드 초기화
         * CameraController: 카메라 위치 초기화
         * UIManager: 턴, 정보 창 활성화
         * */
        OnGameStart?.Invoke(this, EventArgs.Empty);
            
        //피스 랜덤으로 설정해줌
        truePieceIndex = UnityEngine.Random.Range(1, 5);
        falsePieceIndex = UnityEngine.Random.Range(1, 5);
        if(truePieceIndex == falsePieceIndex)
        {
            falsePieceIndex++;
            if(falsePieceIndex == 5)
            {
                falsePieceIndex = 1;
            }
        }

        //게임시작
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
        if (gameMode != EGameMode.PVPNet)
        {
            state = EGameState.Start;
        }
        else
        {
            NetRematch rm = new NetRematch();
            rm.teamId = currentTeam;
            rm.wantRematch = 1;
            Client.instance.SendToServer(rm);
        }
    }

    public void ExitGame()
    {
        state = EGameState.Ready;
        WorldUIManager.instance.StopAllWorldCameraAndCanvas();
        WorldUIManager.instance.GoToMainCanvas();

        if(gameMode == EGameMode.PVPNet)
        {
            NetRematch rm = new NetRematch();
            rm.teamId = currentTeam;
            rm.wantRematch = 0;
            Client.instance.SendToServer(rm);

            Invoke("ShutDownRelay", 1f);

            playerCount = -1;
            currentTeam = -1;

            rematchButton.interactable = true;
            rematchIndicatorObj.transform.GetChild(0).gameObject.SetActive(false);
            rematchIndicatorObj.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    public void OnOnlineHostButton()
    {
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
    }

    public void OnOnlineConnectButton()
    {
        client.Init(WorldUIManager.instance.addressInput.text, 8007);
    }

    public void OnHostBackButton()
    {
        server.ShutDown();
        client.ShutDown();
        playerCount = -1;
        currentTeam = -1;
    }

    private void OnApplicationQuit()
    {
        server.ShutDown();
        client.ShutDown();
    }

    private void RegisterEvents()
    {
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.S_MAKE_MOVE += OnMakeMoveServer;
        NetUtility.S_REMATCH += OnRematchServer;

        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGameClient;
        NetUtility.C_MAKE_MOVE += OnMakeMoveClient;
        NetUtility.C_REMATCH += OnRematchClient;
    }

    private void UnRegisterEvents()
    {
        NetUtility.S_WELCOME -= OnWelcomeServer;
        NetUtility.S_MAKE_MOVE -= OnMakeMoveServer;
        NetUtility.S_REMATCH -= OnRematchServer;

        NetUtility.C_WELCOME -= OnWelcomeClient;
        NetUtility.C_START_GAME -= OnStartGameClient;
        NetUtility.C_MAKE_MOVE -= OnMakeMoveClient;
        NetUtility.C_REMATCH -= OnRematchClient;
    }

    private void OnWelcomeServer(NetMessage msg, NetworkConnection cnn)
    {
        // Client has connected, assign a team and return the message back to him
        NetWelcome nw = msg as NetWelcome;

        // Assign a team
        nw.assignedTeam = ++playerCount;

        // Return back to the client
        Server.instance.SendToClient(cnn, nw);
        

        //If full, Start the game
        if(playerCount == 1)
        {
            Server.instance.BroadCast(new NetStartGame());
        }
    }

    private void OnWelcomeClient(NetMessage msg)
    {
        // Receive the connection message
        NetWelcome nw = msg as NetWelcome;

        // Assign the team
        currentTeam = nw.assignedTeam;

        Debug.Log($"My Assigned team is {nw.assignedTeam}");
    }

    private void OnStartGameClient(NetMessage obj)
    {
        // Game Start on Client
        gameMode = EGameMode.PVPNet;
        state = EGameState.Start;
        GameManager.Instance.isAiMode = false;
        BoardManager.instance.isAiMove = false;
        WorldUIManager.instance.StopAllWorldCameraAndCanvas();
    }

    private void OnMakeMoveServer(NetMessage msg, NetworkConnection cnn)
    {
        NetMakeMove mm = msg as NetMakeMove;

        // Receive and just broadcast it back
        Server.instance.BroadCast(mm);
    }

    private void OnRematchServer(NetMessage msg, NetworkConnection cnn)
    {
        Server.instance.BroadCast(msg);
    }

    private void OnRematchClient(NetMessage msg)
    {
        // Receive the Connection Message
        NetRematch rm = msg as NetRematch;

        // Set the boolean for rematch
        playerRematch[rm.teamId] = rm.wantRematch == 1;

        // Activate the piece of UI
        if(rm.teamId != currentTeam)
        {
            rematchIndicatorObj.transform.GetChild((rm.wantRematch == 1) ? 0 : 1).gameObject.SetActive(true);
            if(rm.wantRematch != 1)
            {
                rematchButton.interactable = false;
            }
        }

        // If both wants to rematch
        if (playerRematch[0] && playerRematch[1])
        {
            state = EGameState.Start;
            rematchButton.interactable = true;
            rematchIndicatorObj.transform.GetChild(0).gameObject.SetActive(false);
            rematchIndicatorObj.transform.GetChild(1).gameObject.SetActive(false);
        }

    }

    private void OnMakeMoveClient(NetMessage msg)
    {
        NetMakeMove mm = msg as NetMakeMove;

        Debug.Log($"MM: {mm.teamId} : {mm.startIndex} -> {mm.endIndex} | {mm.removeIndex} in {mm.gameState}");

        if(mm.teamId != currentTeam)
        {
            //Put 로직
            if(mm.gameState == 3)
            {
                Move move = new Move(mm.startIndex, mm.endIndex, mm.removeIndex, EGameState.Putting);
                BoardManager.instance.OnlineOpponentPutPiece(move);
            }

            //Move 로직
            else if(mm.gameState == 4)
            {
                Move move = new Move(mm.startIndex, mm.endIndex, mm.removeIndex, EGameState.Move);
                BoardManager.instance.OnlineOpponentMovePiece(move);
            }

            //Delete 로직
            else if(mm.gameState == 5)
            {
                Move move = new Move(mm.startIndex, mm.endIndex, mm.removeIndex, EGameState.Delete);
                BoardManager.instance.OnlineOpponentDeletePiece(move);
            }
        }
    }

    private void ShutDownRelay()
    {
        Client.instance.ShutDown();
        Server.instance.ShutDown();
    }
}

