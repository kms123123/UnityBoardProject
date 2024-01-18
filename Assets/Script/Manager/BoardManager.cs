using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
using UnityEngine;
public class DeletePieceEventArgs : EventArgs
{
    public bool turn;
    public Node deleteNode;
}

public class BoardManager : MonoBehaviour
{
    

    public event EventHandler<Node> OnPutPiece;
    public event EventHandler<Node> OnDeletePieceStart;
    public event EventHandler<DeletePieceEventArgs> OnDeletePieceEnd;
    public event EventHandler<Node> OnMoveEnd;

    public static BoardManager instance;

    public List<Node> gameBoard = new List<Node>();
    [SerializeField] private Transform piecePrafab;

    [SerializeField] private Material turnTrueColor;
    [SerializeField] private Material turnFalseColor;
    [SerializeField] private Material selectedColor;
    [SerializeField] private Material canSelectColor;
    [SerializeField] private Material cannotSelectColor;

    [SerializeField] private LayerMask nodeMask;
    [SerializeField] private GameObject tempPieceObj;

    public enum MoveState { BeforePick, AfterPick };

    public MoveState moveState = MoveState.BeforePick;
    private Node selectNodeInMove = null;

    private List<PieceInfo> truePieceList;
    private List<PieceInfo> falsePieceList;
    private int depth = 3;

    public bool isAiMove = true;
    public int aiWantToDelete = -1;

    //Putting 중에 돌을 다 쓰는 상황이 되어 게임오버되는 현상 방지
    public bool isAiCalculating = false;
    private bool isAiStart = false;


    private void Awake()
    {
        if (instance != null) return;

        instance = this;
    }

    private void Start()
    {
        truePieceList = new List<PieceInfo>();
        falsePieceList = new List<PieceInfo>();
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
        if (!isAiMove)
        {
            PlayerMovePiece();
        }

        else
        {
            if(!isAiStart)
            {
                StartCoroutine(AIMovePiece());
                isAiStart = true;
            }
        }
    }


    private IEnumerator AIMovePiece()
    {
        Move bestMove = MoveMinimax();
        Node startNode = gameBoard[bestMove.startIndex];
        Node endNode = gameBoard[bestMove.endIndex];

        //AI가 고민하는 것같은 효과 연출
        float randomTime = UnityEngine.Random.Range(2, 3);
        yield return new WaitForSeconds(randomTime);

        //피스이동
        Piece toMove = startNode.currentPiece;
        endNode.currentPiece = toMove;
        toMove.SetNode(endNode);
        toMove.SetbMatch(false);
        toMove.transform.position = new Vector3(endNode.transform.position.x, toMove.transform.position.y, endNode.transform.position.z);
        startNode.currentPiece = null;

        PieceInfo pieceInfo = startNode.pieceInfo;
        endNode.pieceInfo = pieceInfo;
        pieceInfo.SetNode(endNode);
        pieceInfo.SetbMatch(false);
        startNode.pieceInfo = null;

        //피스가 3매치였을때, 3매치에 해당하는 보호 flag 비활성화
        for (int i = 0; i < Check3MatchManager.instance.current3MatchCombinations.Count; i++)
        {
            if (Check3MatchManager.instance.current3MatchCombinations[i].list.Contains(startNode))
            {
                foreach (Node node in Check3MatchManager.instance.current3MatchCombinations[i].list)
                {
                    if (node.pieceInfo != null)
                        node.pieceInfo.SetbMatch(false);
                }

                Check3MatchManager.instance.current3MatchCombinations.Remove(Check3MatchManager.instance.current3MatchCombinations[i]);
                i--;
            }
        }

        //3매치가 되는지 확인, 되면 Delete State
        if (Check3MatchManager.instance.Check3Match(endNode))
        {
            //3피스만 남아있다면, 3피스 움직임 횟수 초기화
            GameManager.Instance.SetZeroOf3Moves(GameManager.Instance.turn);
            OnDeletePieceStart?.Invoke(this, endNode);
            GameManager.Instance.SetState(GameManager.EGameState.Delete);
        }
        else
        {
            OnMoveEnd?.Invoke(this, endNode);
        }

        isAiStart = false;
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
            if(!isAiStart)
            {
                StartCoroutine(AIPutPiece());
                isAiStart = true;
            }
        }
    }

    private IEnumerator AIPutPiece()
    {
        Node node = gameBoard[PutMinimax()];

        float randomTime = UnityEngine.Random.Range(2, 3);
        yield return new WaitForSeconds(randomTime);

        //아니라면 피스를 생성하고 설정
        Transform pieceObj = GameManager.Instance.turn ? GetCharacterPiece(GameManager.Instance.testForTrueIndex) : GetCharacterPiece(GameManager.Instance.testForFalseIndex);
        Transform piece = Instantiate(pieceObj, new Vector3(node.transform.position.x, 0.3f, node.transform.position.z), Quaternion.identity);
        piece.GetComponent<Piece>().SetNode(node);
        node.currentPiece = piece.GetComponent<Piece>();

        PieceInfo pieceInfo = new PieceInfo();
        node.pieceInfo = pieceInfo;
        pieceInfo.SetNode(node);

        //턴이 누구인지에 따라 피스를 각각의 리스트에 넣는다.
        if (GameManager.Instance.turn)
        {
            piece.GetComponent<Piece>().SetOwnerToTrue();
            pieceInfo.SetOwnerToTrue();
            truePieceList.Add(pieceInfo);
        }
        else
        {
            piece.GetComponent<Piece>().SetOwnerToFalse();
            pieceInfo.SetOwnerToFalse();
            falsePieceList.Add(pieceInfo);
        }

        OnPutPiece?.Invoke(this, node);

        //놓았을 때, 해당 노드와 연결된 3Match가 있는지 확인, 있으면 Delete 모드
        if (Check3MatchManager.instance.Check3Match(node))
        {
            OnDeletePieceStart?.Invoke(this, node);
            GameManager.Instance.SetState(GameManager.EGameState.Delete);
        }
        else
        {
            GameManager.Instance.ChangeTurn();
        }

        isAiStart = false;
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
            if(!isAiStart)
            {
                StartCoroutine(AIDeletePiece());
                isAiStart = true;
            }
        }
    }

    private IEnumerator AIDeletePiece()
    {
        //Put 또는 Move 단계에서 정했던 index의 노드를 고른다.
        Node selectNode = gameBoard[aiWantToDelete];

        yield return new WaitForSeconds(UnityEngine.Random.Range(1, 2));

        //상대의 말을 제대로 골랐을 때,
        if (selectNode.pieceInfo.GetOwner() != GameManager.Instance.turn)
        {
            //그 말이 3매치로 보호되지 않는다면 삭제
            if (!selectNode.pieceInfo.GetbMatch())
            {
                //턴의 반대 주인의 피스를 삭제
                if (GameManager.Instance.turn)
                {
                    falsePieceList.Remove(selectNode.pieceInfo);
                }
                else
                {
                    truePieceList.Remove(selectNode.pieceInfo);
                }

                DestroyImmediate(selectNode.currentPiece.gameObject);
                selectNode.pieceInfo.SetNode(null);
                selectNode.pieceInfo = null;
                selectNode.currentPiece = null;

                OnDeletePieceEnd?.Invoke(this, new DeletePieceEventArgs
                {
                    turn = GameManager.Instance.turn,
                    deleteNode = selectNode,
                });
            }
        }

        isAiStart = false;
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

                    if (selectNodeInMove.pieceInfo == null) return;

                    //해당 턴의 주인이 자신의 말을 골랐을 때, 제대로 선택되었음을 확인
                    if (selectNodeInMove.pieceInfo.GetOwner() == GameManager.Instance.turn)
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
                    if (selectNode.pieceInfo == null)
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

                            PieceInfo pieceInfo = selectNodeInMove.pieceInfo;
                            selectNode.pieceInfo = pieceInfo;
                            pieceInfo.SetNode(selectNode);
                            pieceInfo.SetbMatch(false);
                            selectNodeInMove.pieceInfo = null;

                            //피스가 3매치였을때, 3매치에 해당하는 보호 flag 비활성화
                            for (int i = 0; i < Check3MatchManager.instance.current3MatchCombinations.Count; i++)
                            {
                                if (Check3MatchManager.instance.current3MatchCombinations[i].list.Contains(selectNodeInMove))
                                {
                                    foreach (Node node in Check3MatchManager.instance.current3MatchCombinations[i].list)
                                    {
                                        if (node.pieceInfo != null)
                                            node.pieceInfo.SetbMatch(false);
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
                                OnDeletePieceStart?.Invoke(this, selectNode);
                                GameManager.Instance.SetState(GameManager.EGameState.Delete);
                            }
                            else
                            {
                                OnMoveEnd?.Invoke(this, selectNode);
                            }
                        }
                    }

                    //다른 팀의 피스일 경우 선택초기화
                    else if (selectNode.pieceInfo.GetOwner() != GameManager.Instance.turn)
                    {
                        moveState = MoveState.BeforePick;
                        selectNodeInMove = null;
                    }

                    //같은 팀의 피스일 경우, 그 피스를 선택
                    else if (selectNode.pieceInfo.GetOwner() == GameManager.Instance.turn)
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
                    if (node.pieceInfo != null) return;

                    Transform pieceObj = GameManager.Instance.turn ? GetCharacterPiece(GameManager.Instance.testForTrueIndex) : GetCharacterPiece(GameManager.Instance.testForFalseIndex);

                    //아니라면 피스를 생성하고 설정
                    Transform piece = Instantiate(pieceObj, new Vector3(node.transform.position.x, 0.3f, node.transform.position.z), Quaternion.identity);
                    piece.GetComponent<Piece>().SetNode(node);
                    node.currentPiece = piece.GetComponent<Piece>();

                    PieceInfo pieceInfo = new PieceInfo();
                    pieceInfo.SetNode(node);
                    node.pieceInfo = pieceInfo;

                    //턴이 누구인지에 따라 피스를 각각의 리스트에 넣는다.
                    if (GameManager.Instance.turn)
                    {
                        piece.GetComponent<Piece>().SetOwnerToTrue();
                        pieceInfo.SetOwnerToTrue();
                        truePieceList.Add(pieceInfo);
                    }
                    else
                    {
                        piece.GetComponent<Piece>().SetOwnerToFalse();
                        pieceInfo.SetOwnerToFalse();
                        falsePieceList.Add(pieceInfo);
                    }

                    OnPutPiece?.Invoke(this, node);

                    //놓았을 때, 해당 노드와 연결된 3Match가 있는지 확인, 있으면 Delete 모드
                    if (Check3MatchManager.instance.Check3Match(node))
                    {
                        OnDeletePieceStart?.Invoke(this, node);
                        GameManager.Instance.SetState(GameManager.EGameState.Delete);
                    }
                    else
                    {
                        GameManager.Instance.ChangeTurn();
                    }
                }
            }
        }
    }

    private Transform GetCharacterPiece(int characterIndex)
    {
        return Resources.Load<Transform>("Prefabs/Piece_" + characterIndex.ToString());
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
                if (selectNode.pieceInfo.GetOwner() != GameManager.Instance.turn)
                {
                    //그 말이 3매치로 보호되지 않는다면 삭제
                    if (!selectNode.pieceInfo.GetbMatch())
                    {
                        //턴의 반대 주인의 피스를 삭제
                        if (GameManager.Instance.turn)
                        {
                            falsePieceList.Remove(selectNode.pieceInfo);
                        }
                        else
                        {
                            truePieceList.Remove(selectNode.pieceInfo);
                        }

                        selectNode.pieceInfo.SetNode(null);
                        DestroyImmediate(selectNode.currentPiece.gameObject);
                        selectNode.currentPiece = null;
                        selectNode.pieceInfo = null;

                        OnDeletePieceEnd?.Invoke(this, new DeletePieceEventArgs
                        {
                            turn = GameManager.Instance.turn,
                            deleteNode = selectNode,
                        });
                    }
                }
            }
        }
    }

    #endregion

    //특정 state에 따라 Ai가 행동가능한 모든 경우의 수를 반환
    public List<Move> GenerateAllPossibleMoves(bool turn, GameManager.EGameState state)
    {
        List<Move> result = new List<Move>();

        //Put state일 시 
        if (state == GameManager.EGameState.Putting)
        {
            for (int i = 0; i < gameBoard.Count; i++)
            {
                Node node = gameBoard[i];
                Move move = new Move(-1, -1, -1, GameManager.EGameState.Putting);

                if (node.pieceInfo == null)
                {
                    PieceInfo pieceInfo = new PieceInfo();
                    pieceInfo.SetNode(node);
                    if (turn)
                    {
                        pieceInfo.SetOwnerToTrue();
                        GameManager.Instance.turnTrueHavetoPut--;
                        GameManager.Instance.totalTruePiece++;
                    }
                    else
                    {
                        pieceInfo.SetOwnerToFalse();
                        GameManager.Instance.turnFalseHavetoPut--;
                        GameManager.Instance.totalFalsePiece++;
                    }
                    node.pieceInfo = pieceInfo;

                    move.endIndex = i;
                    if (Check3MatchManager.instance.Check3Match(node))
                    {
                        for (int j = 0; j < gameBoard.Count; j++)
                        {
                            Node deleteNode = gameBoard[j];
                            if (deleteNode.pieceInfo != null)
                            {
                                if (deleteNode.pieceInfo.GetOwner() != turn && !deleteNode.pieceInfo.GetbMatch())
                                {
                                    Move newMove = new Move(-1, move.endIndex, j, GameManager.EGameState.Delete);
                                    result.Add(newMove);
                                }
                            }
                        }
                    }
                    if (!Check3MatchManager.instance.Check3MatchAndDeleteFlag(node))
                    {
                        result.Add(move);
                    }

                    if (node.pieceInfo != null)
                    {
                        node.pieceInfo.SetNode(null);
                        node.pieceInfo = null;
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

                }
            }

        }

        //Move state일 시
        else if(state == GameManager.EGameState.Move)
        {
            for (int i = 0; i < gameBoard.Count; i++)
            {
                if (gameBoard[i].pieceInfo == null) continue;
                if (gameBoard[i].pieceInfo.GetOwner() != turn) continue;

                Node node = gameBoard[i];

                //True 턴, 아니면 False턴이 피스 3개일 시
                if ((turn && GameManager.Instance.totalTruePiece == 3) || (!turn && GameManager.Instance.totalFalsePiece == 3))
                {
                    for (int k = 0; k < gameBoard.Count; k++)
                    {
                        Node nextNode = gameBoard[k];

                        if (nextNode == node) continue;
                        if (nextNode.pieceInfo != null) continue;

                        Move move = new Move(i, k, -1, GameManager.EGameState.Move);
                        Check3MatchManager.instance.Check3MatchAndDeleteFlag(node);
                        PieceInfo movePiece = node.pieceInfo;
                        node.pieceInfo = null;
                        movePiece.SetNode(nextNode);
                        movePiece.SetbMatch(false);
                        nextNode.pieceInfo = movePiece;

                        if (Check3MatchManager.instance.Check3Match(nextNode))
                        {
                            for (int j = 0; j < gameBoard.Count; j++)
                            {
                                Node deleteNode = gameBoard[j];
                                if (deleteNode.pieceInfo != null)
                                {
                                    if (deleteNode.pieceInfo.GetOwner() != turn && !deleteNode.pieceInfo.GetbMatch())
                                    {
                                        Move newMove = new Move(i, k, j, GameManager.EGameState.Delete);
                                        result.Add(newMove);
                                    }
                                }
                            }
                        }
                        if (!Check3MatchManager.instance.Check3MatchAndDeleteFlag(nextNode))
                        {
                            result.Add(move);
                        }

                        nextNode.pieceInfo = null;
                        movePiece.SetNode(node);
                        node.pieceInfo = movePiece;
                        Check3MatchManager.instance.Check3Match(node);
                    }
                }
                //다 아니라면 연결된 노드로만 이동가능
                else
                {
                    for (int k = 0; k < node.linkedNodes.Count; k++)
                    {
                        Node nextNode = node.linkedNodes[k];

                        if (nextNode.pieceInfo != null) continue;

                        for(int z = 0; z < gameBoard.Count; z++)
                        {
                            if (gameBoard[z] == nextNode)
                            {
                                Move move = new Move(i, z, -1, GameManager.EGameState.Move);
                                Check3MatchManager.instance.Check3MatchAndDeleteFlag(node);
                                PieceInfo movePiece = node.pieceInfo;
                                node.pieceInfo = null;
                                movePiece.SetNode(nextNode);
                                nextNode.pieceInfo = movePiece;

                                if (Check3MatchManager.instance.Check3Match(nextNode))
                                {
                                    for (int j = 0; j < gameBoard.Count; j++)
                                    {
                                        Node deleteNode = gameBoard[j];
                                        if (deleteNode.pieceInfo != null)
                                        {
                                            if (deleteNode.pieceInfo.GetOwner() != turn && !deleteNode.pieceInfo.GetbMatch())
                                            {
                                                Move newMove = new Move(i, z, j, GameManager.EGameState.Delete);
                                                result.Add(newMove);
                                            }
                                        }
                                    }
                                }
                                if (!Check3MatchManager.instance.Check3MatchAndDeleteFlag(nextNode))
                                {
                                    result.Add(move);
                                }

                                nextNode.pieceInfo = null;
                                movePiece.SetNode(node);
                                node.pieceInfo = movePiece;
                                Check3MatchManager.instance.Check3Match(node);

                                break;
                            }
                        }
                    }
                }
            }

        }

        return result;
    }

    //Ai의 Putting
    private int PutMinimax()
    {
        isAiCalculating = true;
        List<Move> puts = GenerateAllPossibleMoves(GameManager.Instance.turn, GameManager.EGameState.Putting);

        foreach (Move move in puts)
        {
            DoPut(move, GameManager.Instance.turn, gameBoard);
            move.score += AlphaBeta(!GameManager.Instance.turn, gameBoard, depth, int.MinValue, int.MaxValue, GameManager.EGameState.Putting);
            UndoPut(move, GameManager.Instance.turn, gameBoard);
        }

        if (!GameManager.Instance.turn)
        {
            puts.Sort((Move a, Move b) =>
            {
                return a.score - b.score;
            });
        }
        else
        {
            puts.Sort((Move a, Move b) =>
            {
                return b.score - a.score;
            });
        }


        List<Move> result = new List<Move>();
        int bestScore = puts[0].score;
        result.Add(puts[0]);

        for (int i = 0; i < puts.Count; i++)
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
        isAiCalculating = false;

        if (bestMove.removeIndex != -1)
        {
            aiWantToDelete = bestMove.removeIndex;
        }

        return bestMove.endIndex;
    }

    //Ai의 Move
    private Move MoveMinimax()
    {
        isAiCalculating = true;
        List<Move> puts = GenerateAllPossibleMoves(GameManager.Instance.turn, GameManager.EGameState.Move);

        foreach (Move move in puts)
        {
            DoMove(move, GameManager.Instance.turn, gameBoard);
            move.score += AlphaBeta(!GameManager.Instance.turn, gameBoard, depth, int.MinValue, int.MaxValue, GameManager.EGameState.Move);
            UndoMove(move, GameManager.Instance.turn, gameBoard);
        }

        if (!GameManager.Instance.turn)
        {
            puts.Sort((Move a, Move b) =>
            {
                return a.score - b.score;
            });
        }
        else
        {
            puts.Sort((Move a, Move b) =>
            {
                return b.score - a.score;
            });
        }


        List<Move> result = new List<Move>();
        int bestScore = puts[0].score;
        result.Add(puts[0]);

        for (int i = 0; i < puts.Count; i++)
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
        isAiCalculating = false;

        if (bestMove.removeIndex != -1)
        {
            aiWantToDelete = bestMove.removeIndex;
        }

        return bestMove;
    }

    //Putting을 시뮬레이션 합니다.
    private void DoPut(Move move, bool turn, List<Node> gameBoard)
    {
        Node node = gameBoard[move.endIndex];
        if (node.pieceInfo == null)
        {
            PieceInfo pieceInfo = new PieceInfo();
            pieceInfo.SetNode(node);
            if (turn)
            {
                pieceInfo.SetOwnerToTrue();
                GameManager.Instance.turnTrueHavetoPut--;
                GameManager.Instance.totalTruePiece++;
                truePieceList.Add(pieceInfo);
            }
            else
            {
                pieceInfo.SetOwnerToFalse();
                GameManager.Instance.turnFalseHavetoPut--;
                GameManager.Instance.totalFalsePiece++;
                falsePieceList.Add(pieceInfo);
            }
            node.pieceInfo = pieceInfo;

            //만약 delete되는 상대 피스가 있을 시 반영
            if (move.removeIndex != -1)
            {
                Check3MatchManager.instance.Check3Match(node);
                Node deleteNode = gameBoard[move.removeIndex];

                //턴의 반대 주인의 피스를 삭제
                if (turn)
                {
                    falsePieceList.Remove(deleteNode.pieceInfo);
                    GameManager.Instance.totalFalsePiece--;
                }
                else
                {
                    truePieceList.Remove(deleteNode.pieceInfo);
                    GameManager.Instance.totalTruePiece--;
                }


                deleteNode.pieceInfo.SetNode(null);
                deleteNode.pieceInfo = null;

            }
        }
    }

    //Putting을 원상복구 합니다.
    private void UndoPut(Move move, bool turn, List<Node> gameBoard)
    {
        Node node = gameBoard[move.endIndex];

        if (node.pieceInfo != null)
        {
            if (move.removeIndex != -1)
            {
                Check3MatchManager.instance.Check3MatchAndDeleteFlag(node);
            }

            node.pieceInfo.SetNode(null);
            if (turn)
            {
                GameManager.Instance.turnTrueHavetoPut++;
                GameManager.Instance.totalTruePiece--;
                truePieceList.Remove(node.pieceInfo);
            }
            else
            {
                GameManager.Instance.turnFalseHavetoPut++;
                GameManager.Instance.totalFalsePiece--;
                falsePieceList.Remove(node.pieceInfo);
            }
            node.pieceInfo = null;

            //삭제된 피스가 있었다면 복구
            if (move.removeIndex != -1)
            {
                Node deleteNode = gameBoard[move.removeIndex];
                PieceInfo pieceInfo = new PieceInfo();
                pieceInfo.SetNode(deleteNode);
                deleteNode.pieceInfo = pieceInfo;

                if (turn)
                {
                    pieceInfo.SetOwnerToFalse();
                    falsePieceList.Add(deleteNode.pieceInfo);
                    GameManager.Instance.totalFalsePiece++;
                }
                else
                {
                    pieceInfo.SetOwnerToTrue();
                    truePieceList.Add(deleteNode.pieceInfo);
                    GameManager.Instance.totalTruePiece++;
                }

            }
        }
    }

    //Move를 시뮬레이션 합니다.
    private void DoMove(Move move, bool turn, List<Node> gameBoard)
    {
        Node startNode = gameBoard[move.startIndex];
        Node endNode = gameBoard[move.endIndex];
        if (endNode.pieceInfo == null)
        {
            PieceInfo pieceInfo = startNode.pieceInfo;
            Check3MatchManager.instance.Check3MatchAndDeleteFlag(startNode);
            startNode.pieceInfo = null;
            pieceInfo.SetNode(endNode);
            pieceInfo.SetbMatch(false);
            endNode.pieceInfo = pieceInfo;

            //만약 delete되는 상대 피스가 있을 시 반영
            if (move.removeIndex != -1)
            {
                if (!Check3MatchManager.instance.Check3Match(endNode)) Debug.LogWarning("This Cant be Happen");
                Node deleteNode = gameBoard[move.removeIndex];

                //턴의 반대 주인의 피스를 삭제
                if (turn)
                {
                    falsePieceList.Remove(deleteNode.pieceInfo);
                    GameManager.Instance.totalFalsePiece--;
                }
                else
                {
                    truePieceList.Remove(deleteNode.pieceInfo);
                    GameManager.Instance.totalTruePiece--;
                }

                deleteNode.pieceInfo.SetNode(null);
                deleteNode.pieceInfo = null;
            }
        }
    }

    //Move를 원상복구 합니다.
    private void UndoMove(Move move, bool turn, List<Node> gameBoard)
    {
        Node startNode = gameBoard[move.startIndex];
        Node endNode = gameBoard[move.endIndex];
        if (endNode.pieceInfo != null)
        {
            if(move.removeIndex != -1)
            {
                Check3MatchManager.instance.Check3MatchAndDeleteFlag(endNode);
            }

            PieceInfo pieceInfo = endNode.pieceInfo;
            endNode.pieceInfo.SetNode(null);
            endNode.pieceInfo = null;
            startNode.pieceInfo = pieceInfo;
            pieceInfo.SetNode(startNode);

            //삭제된 피스가 있었다면 복구
            if (move.removeIndex != -1)
            {
                Node deleteNode = gameBoard[move.removeIndex];
                PieceInfo deleteInfo = new PieceInfo();
                deleteInfo.SetNode(deleteNode);
                deleteNode.pieceInfo = deleteInfo;

                if (turn)
                {
                    deleteInfo.SetOwnerToFalse();
                    falsePieceList.Add(deleteNode.pieceInfo);
                    GameManager.Instance.totalFalsePiece++;
                }
                else
                {
                    deleteInfo.SetOwnerToTrue();
                    truePieceList.Add(deleteNode.pieceInfo);
                    GameManager.Instance.totalTruePiece++;
                }

            }

            Check3MatchManager.instance.Check3Match(startNode);
        }
    }

    //특정 보드 순간의 점수를 계산합니다.
    private int AlphaBeta(bool turn, List<Node> gameBoard, int depth, int alpha, int beta, GameManager.EGameState state)
    {
        List<Move> moves;
        //최종 깊이의 경우 휴리스틱을 계산해서 return
        if (depth == 0)
        {
            return CalculateHeuristic(state);
        }
        else if (GameManager.Instance.IsGameOver())
        {
            if (GameManager.Instance.IsTurnFalseDefeat())
            {
                return HeuristicScore.TRUEWIN;
            }
            else if (GameManager.Instance.IsTurnTrueDefeat())
            {
                return HeuristicScore.FALSEWIN;
            }
        }
        else
        {
            moves = GenerateAllPossibleMoves(turn, state);

            foreach (Move move in moves)
            {
                if (state == GameManager.EGameState.Putting)
                {
                    DoPut(move, turn, gameBoard);

                    if (turn)
                    {
                        alpha = math.max(alpha, AlphaBeta(!turn, gameBoard, depth - 1, alpha, beta, GameManager.EGameState.Putting));
                        if (beta <= alpha)
                        {
                            UndoPut(move, turn, gameBoard);
                            break;
                        }
                    }
                    else
                    {
                        beta = math.min(beta, AlphaBeta(!turn, gameBoard, depth - 1, alpha, beta, GameManager.EGameState.Putting));
                        if (beta <= alpha)
                        {
                            UndoPut(move, turn, gameBoard);
                            break;
                        }
                    }

                    UndoPut(move, turn, gameBoard);
                }

                if(state == GameManager.EGameState.Move)
                {
                    DoMove(move, turn, gameBoard);

                    if (turn)
                    {
                        alpha = math.max(alpha, AlphaBeta(!turn, gameBoard, depth - 1, alpha, beta, GameManager.EGameState.Move));
                        if (beta <= alpha)
                        {
                            UndoMove(move, turn, gameBoard);
                            break;
                        }
                    }
                    else
                    {
                        beta = math.min(beta, AlphaBeta(!turn, gameBoard, depth - 1, alpha, beta, GameManager.EGameState.Move));
                        if (beta <= alpha)
                        {
                            UndoMove(move, turn, gameBoard);
                            break;
                        }
                    }

                    UndoMove(move, turn, gameBoard);
                }

            }

            if (turn) return alpha;
            else return beta;
        }

        Debug.LogWarning("Error in AlphaBeta");
        return -1;
    }

    private int CalculateHeuristic(GameManager.EGameState state)
    {
        int score = 0;
        int turnTrueCombiCount = 0, turnFalseCombiCount = 0;
        int turnTrueTwoPieceCount = 0, turnFalseTwoPieceCount = 0;

        /**
         * 모든 3매치를 탐색하면서 현 보드의 상황을 파악한다.
         * 3매치, 2매치, 1매치의 개수, 주요 요충지를 누가 갖고있는지를 모두 판단.
         * 이후, 각 수마다 보정치를 곱하여 최종 스코어를 계산한다.
         * */
        for (int i = 0; i < Check3MatchManager.instance.GetPossibleCombinations().Count; i++)
        {
            int turnTruePiece = 0, emptyNode = 0, turnFalsePiece = 0;

            ListWrapper wrapperList = Check3MatchManager.instance.GetPossibleCombinations()[i];
            foreach (Node node in wrapperList.list)
            {
                if (node.pieceInfo == null) emptyNode++;
                else if (node.pieceInfo.GetOwner()) turnTruePiece++;
                else if (!node.pieceInfo.GetOwner()) turnFalsePiece++;
            }

            if (turnTruePiece == 3)
            {
                turnTrueCombiCount++;
            }
            else if (turnTruePiece == 2 && emptyNode == 1)
            {
                turnTrueTwoPieceCount++;
            }
            else if (turnTruePiece == 1 && emptyNode == 2)
            {
                score += 1;
            }
            else if (turnFalsePiece == 1 && emptyNode == 2)
            {
                score -= 1;
            }
            else if (turnFalsePiece == 2 && emptyNode == 1)
            {
                turnFalseTwoPieceCount++;
            }
            else if (turnFalsePiece == 3)
            {
                turnFalseCombiCount++;
            }

            foreach (Node node in wrapperList.list)
            {
                if (node.pieceInfo != null)
                {
                    if (node.linkedNodes.Count == 4)
                    {
                        if (node.pieceInfo.GetOwner())
                        {
                            score += 2;
                        }
                        else if (!node.pieceInfo.GetOwner())
                        {
                            score -= 2;
                        }
                    }

                    else if (node.linkedNodes.Count == 3)
                    {
                        if (node.pieceInfo.GetOwner())
                        {
                            score += 1;
                        }
                        else if (!node.pieceInfo.GetOwner())
                        {
                            score -= 1;
                        }
                    }
                }
            }
        }

        int multiplier;

        //3매치의 개수
        if (state == GameManager.EGameState.Putting)
        {
            multiplier = HeuristicScore.PUT3Match;
        }
        else if (state == GameManager.EGameState.Move)
        {
            multiplier = HeuristicScore.Move3Match;
        }
        else
        {
            multiplier = HeuristicScore.Delete3Match;
        }

        score += multiplier * turnTrueCombiCount;
        score -= multiplier * turnFalseCombiCount;

        //말의 개수
        if (state == GameManager.EGameState.Putting)
        {
            multiplier = HeuristicScore.PutNumberPiece;
        }
        else if (state == GameManager.EGameState.Move)
        {
            multiplier = HeuristicScore.MoveNumberPiece;
        }
        else
        {
            multiplier = HeuristicScore.DeleteNumberPiece;
        }

        score += multiplier * GameManager.Instance.totalTruePiece;
        score -= multiplier * GameManager.Instance.totalFalsePiece;

        //2매치의 개수
        if (state == GameManager.EGameState.Putting)
        {
            multiplier = HeuristicScore.Put2Match;
        }
        else
        {
            multiplier = HeuristicScore.MoveDelete2Match;
        }

        score += multiplier * turnTrueTwoPieceCount;
        score -= multiplier * turnFalseTwoPieceCount;

        return score;
    }

    //리스트의 피스를 탐색하면서, 모든 피스가 이동 불가능 상태라면(연결된 노드가 모두 주인이 있다면) true
    public bool IsCantMove(bool turn)
    {
        if(turn)
        {
            if (GameManager.Instance.totalTruePiece == 3) return false;
        }
        else
        {
            if (GameManager.Instance.totalFalsePiece == 3) return false;
        }


        List<PieceInfo> list;

        if (turn) list = truePieceList;
        else list = falsePieceList;

        foreach (PieceInfo piece in list)
        {
            Node node = piece.GetNode();

            foreach (Node linkedNode in node.linkedNodes)
            {
                if (linkedNode.pieceInfo == null)
                {
                    return false;
                }
            }
        }

        return true;
    }

}
