using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public event EventHandler OnPutPiece;
    public event EventHandler OnDeletePieceStart;
    public event EventHandler<bool> OnDeletePieceEnd;
    public event EventHandler OnMoveEnd;

    public static BoardManager instance;

    public List<Node> gameBoard = new List<Node>();
    [SerializeField] private Transform piecePrafab;

    [SerializeField] private Material turnTrueColor;
    [SerializeField] private Material turnFalseColor;
    [SerializeField] private Material selectedColor;
    [SerializeField] private Material canSelectColor;
    [SerializeField] private Material cannotSelectColor;

    [SerializeField] private LayerMask nodeMask;

    public enum MoveState { BeforePick, AfterPick };

    public MoveState moveState = MoveState.BeforePick;
    private Node selectNodeInMove = null;

    private List<Piece> truePieceList;
    private List<Piece> falsePieceList;
    private int depth = 3;

    private bool isAiMove = false;

    private void Awake()
    {
        if (instance != null) return;

        instance = this;
    }

    private void Start()
    {
        truePieceList = new List<Piece>();
        falsePieceList = new List<Piece>();
    }

    private void Update()
    {
        if (GameManager.Instance.state == GameManager.EGameState.Putting)
        {
            PutPieceDown();
        }

        else if (GameManager.Instance.state == GameManager.EGameState.Delete)
        {
            DeletePiece();
        }

        else if (GameManager.Instance.state == GameManager.EGameState.Move)
        {
            MovePiece();
        }
    }

    //피스를 선택하고 움직이는 동작
    private void MovePiece()
    {
        //플레이어 턴일 시
        if(!isAiMove)
        {
            PlayerMovePiece();
        }

        else
        {
            if(GameManager.Instance.turn)
            {

            }
            else
            {

            }
        }
    }



    //현 상태의 보드의 정보를 갖고 옵니다.
    private BoardInfo GetInfoFromBoard()
    {
        BoardInfo boardInfo = new BoardInfo();
        boardInfo.SetBoardInfo(MakeNodeInfoList());
        return boardInfo;
    }

    
    private List<NodeInfo> MakeNodeInfoList()
    {
        List<NodeInfo> infoList = new List<NodeInfo>();

        foreach(Node node in gameBoard)
        { 

        }

        return infoList;
    }

    //게임 시작 시, 피스를 놓는 동작
    private void PutPieceDown()
    {
        //플레이어 턴일 시
        if (!isAiMove)
        {
            PlayerPutPiece();
        }

        else
        {

        }
    }

    //피스를 삭제하는 동작
    private void DeletePiece()
    {
        //플레이어 턴일 시
        if (!isAiMove)
        {
            PlayerDeletePiece();
        }

        else
        {

        }
    }

    #region PlayerLogic

    //Move Piece 단계의 플레이어 로직
    private void PlayerMovePiece()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //BeforePick: 피스를 선택하는 단계
            if (moveState == MoveState.BeforePick)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, 1000, nodeMask))
                {
                    selectNodeInMove = hitInfo.transform.GetComponent<Node>();

                    //해당 턴의 주인이 자신의 말을 골랐을 때, 제대로 선택되었음을 확인
                    if (selectNodeInMove.currentPiece.GetOwner() == GameManager.Instance.turn)
                    {
                        moveState = MoveState.AfterPick;
                    }
                }
            }

            //AfterPick: 피스를 움직일 공간을 누르는 단계
            /**
             * case 1: 빈 노드를 누르는 경우
             *  - 3개 이하일때는 순간이동 가능
             * case 2: 같은 팀의 피스를 누르는 경우
             * case 3: 다른 팀의 피스를 누르는 경우
             * */
            else
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, 1000, nodeMask))
                {
                    Node selectNode = hitInfo.transform.GetComponent<Node>();

                    //빈 노드일 경우
                    if (selectNode.currentPiece == null)
                    {
                        //3피스가 남아서 순간이동이 가능한 상태이거나, 연결된 노드일 경우 움직임
                        if (GameManager.Instance.Is3PieceMove() || selectNodeInMove.linkedNodes.Contains(selectNode))
                        {
                            //피스이동
                            Piece toMove = selectNodeInMove.currentPiece;
                            selectNode.currentPiece = toMove;
                            toMove.SetNode(selectNode);
                            toMove.SetbMatch(false);
                            toMove.transform.position = new Vector3(selectNode.transform.position.x, toMove.transform.position.y, selectNode.transform.position.z);
                            selectNodeInMove.currentPiece = null;

                            //피스가 3매치였을때, 3매치에 해당하는 보호 flag 비활성화
                            for (int i = 0; i < Check3MatchManager.instance.current3MatchCombinations.Count; i++)
                            {
                                if (Check3MatchManager.instance.current3MatchCombinations[i].list.Contains(selectNodeInMove))
                                {
                                    foreach (Node node in Check3MatchManager.instance.current3MatchCombinations[i].list)
                                    {
                                        if (node.currentPiece != null)
                                            node.currentPiece.SetbMatch(false);
                                    }

                                    Check3MatchManager.instance.current3MatchCombinations.Remove(Check3MatchManager.instance.current3MatchCombinations[i]);
                                    i--;
                                }
                            }

                            //3매치가 되는지 확인, 되면 Delete State
                            if (Check3MatchManager.instance.Check3Match(selectNode))
                            {
                                //3피스만 남아있다면, 3피스 움직임 횟수 초기화
                                GameManager.Instance.SetZeroOf3Moves(GameManager.Instance.turn);
                                GameManager.Instance.SetState(GameManager.EGameState.Delete);
                                OnDeletePieceStart?.Invoke(this, EventArgs.Empty);
                            }
                            else
                            {
                                OnMoveEnd?.Invoke(this, EventArgs.Empty);
                            }
                        }
                    }

                    //다른 팀의 피스일 경우 선택초기화
                    else if (selectNode.currentPiece.GetOwner() != GameManager.Instance.turn)
                    {
                        moveState = MoveState.BeforePick;
                        selectNodeInMove = null;
                    }

                    //같은 팀의 피스일 경우, 그 피스를 선택
                    else if (selectNode.currentPiece.GetOwner() == GameManager.Instance.turn)
                    {
                        selectNodeInMove = selectNode;
                    }

                }
            }
        }
    }

    //Put Piece 단계의 플레이어 로직
    private void PlayerPutPiece()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                if (hitInfo.transform.GetComponent<Node>() != null)
                {
                    Node node = hitInfo.transform.GetComponent<Node>();

                    //해당 노드에 피스가 이미 있을 시 무시
                    if (node.currentPiece != null) return;

                    //아니라면 피스를 생성하고 설정
                    Transform piece = Instantiate(piecePrafab, new Vector3(hitInfo.transform.position.x, 0.3f, hitInfo.transform.position.z), Quaternion.identity);
                    piece.GetComponent<MeshRenderer>().material = GameManager.Instance.turn ? turnTrueColor : turnFalseColor;
                    piece.GetComponent<Piece>().SetNode(node);
                    node.currentPiece = piece.GetComponent<Piece>();

                    //턴이 누구인지에 따라 피스를 각각의 리스트에 넣는다.
                    if (GameManager.Instance.turn)
                    {
                        piece.GetComponent<Piece>().SetOwnerToTrue();
                        truePieceList.Add(piece.GetComponent<Piece>());
                    }
                    else
                    {
                        piece.GetComponent<Piece>().SetOwnerToFalse();
                        falsePieceList.Add(piece.GetComponent<Piece>());
                    }

                    OnPutPiece?.Invoke(this, EventArgs.Empty);

                    //놓았을 때, 해당 노드와 연결된 3Match가 있는지 확인, 있으면 Delete 모드
                    if (Check3MatchManager.instance.Check3Match(node))
                    {
                        GameManager.Instance.SetState(GameManager.EGameState.Delete);
                        OnDeletePieceStart?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        GameManager.Instance.ChangeTurn();
                    }
                }
            }
        }
    }

    //Delete Piece 단계의 플레이어 로직
    private void PlayerDeletePiece()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 1000, nodeMask))
            {
                Node selectNode = hitInfo.transform.GetComponent<Node>();

                //상대의 말을 제대로 골랐을 때,
                if (selectNode.currentPiece.GetOwner() != GameManager.Instance.turn)
                {
                    //그 말이 3매치로 보호되지 않는다면 삭제
                    if (!selectNode.currentPiece.GetbMatch())
                    {
                        //턴의 반대 주인의 피스를 삭제
                        if (GameManager.Instance.turn)
                        {
                            falsePieceList.Remove(selectNode.currentPiece);
                        }
                        else
                        {
                            truePieceList.Remove(selectNode.currentPiece);
                        }

                        Destroy(selectNode.currentPiece.gameObject);
                        selectNode.currentPiece = null;

                        OnDeletePieceEnd?.Invoke(this, GameManager.Instance.turn);
                    }
                }
            }
        }
    }

    #endregion

    //Todo: 모든 state에 대해서 move를 반환해주는 로직필요
    public List<Move> GenerateAllPossibleMoves(bool turn, GameManager.EGameState state)
    {
        List<Move> result = new List<Move>();
        return result;
    }

    //Ai의 Putting
    private int PutMinimax()
    {
        List<Move> puts = GenerateAllPossibleMoves(GameManager.Instance.turn, GameManager.EGameState.Putting);

        foreach(Move move in puts)
        {
            DoPut(move, GameManager.Instance.turn, gameBoard);
            move.score += CalculateHeuristic(!GameManager.Instance.turn, gameBoard, depth - 1, int.MinValue, int.MaxValue);
            UndoPut(move, GameManager.Instance.turn, gameBoard);
        }
            
        puts.Sort((Move a, Move b) =>
        {
            return a.score - b.score;
        });

        List<Move> result = new List<Move>();
        int bestScore = puts[0].score;
        result.Add(puts[0]);

        for(int i = 0; i <  puts.Count; i++)
        {
            if (puts[i].score == bestScore) 
            {
                result.Add(puts[i]);
            }
            else
            {
                break;
            }    
        }

        Move bestMove = result[UnityEngine.Random.Range(0, result.Count)];
        return bestMove.endIndex;
    }

    //Putting을 시뮬레이션 합니다.
    private void DoPut(Move move, bool turn, List<Node> gameBoard)
    {
        Node node = gameBoard[move.endIndex];
        if(node.currentPiece == null)
        {
            Piece piece = new Piece();
            piece.SetNode(node);
            if (turn)
            {
                piece.SetOwnerToTrue();
                GameManager.Instance.turnTrueHavetoPut--;
                GameManager.Instance.totalTruePiece++;
            } 
            else
            {
                piece.SetOwnerToFalse();
                GameManager.Instance.turnFalseHavetoPut--;
                GameManager.Instance.totalFalsePiece++;
            }
            node.currentPiece = piece;
        }
    }

    //Putting을 원상복구 합니다.
    private void UndoPut(Move move, bool turn, List<Node> gameBoard)
    {
        Node node = gameBoard[move.endIndex];

        node.currentPiece = null;
        if (turn)
        {
            GameManager.Instance.turnTrueHavetoPut++;
            GameManager.Instance.totalTruePiece--;
        }
        else
        {
            GameManager.Instance.turnFalseHavetoPut++;
            GameManager.Instance.totalFalsePiece--;
        }
    }

    //특정 보드 순간의 점수를 계산합니다.
    private int CalculateHeuristic(bool turn, List<Node> gameBoard,int depth, int alpha, int beta)
    {
        throw new NotImplementedException();
    }

    //리스트의 피스를 탐색하면서, 모든 피스가 이동 불가능 상태라면(연결된 노드가 모두 주인이 있다면) true
    public bool IsCantMove(bool turn)
    {
        List<Piece> list;

        if (turn) list = truePieceList;
        else list = falsePieceList;

        foreach (Piece piece in list)
        {
            Node node = piece.GetNode();

            foreach (Node linkedNode in node.linkedNodes)
            {
                if (linkedNode.currentPiece == null)
                {
                    return false;
                }
            }
        }

        return true;
    }

}
