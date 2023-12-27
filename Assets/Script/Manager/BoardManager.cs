using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public event EventHandler OnPutPiece;
    public event EventHandler OnDeletePieceStart;
    public event EventHandler OnDeletePieceEnd;
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

    public enum MoveState { BeforePick, AfterPick};
    
    public MoveState moveState = MoveState.BeforePick;
    private Node selectNodeInMove = null;

    private void Awake()
    {
        if (instance != null) return;

        instance = this;
    }

    private void Update()
    {
        if (GameManager.Instance.state == GameManager.EGameState.Putting)
        {
            PutPieceDown();
        }

        else if(GameManager.Instance.state == GameManager.EGameState.Delete)
        {
            DeletePiece();
        }

        else if(GameManager.Instance.state == GameManager.EGameState.Move)
        {
            MovePiece();
        }
    }

    //피스를 선택하고 움직이는 동작
    private void MovePiece()
    {
        Debug.Log(moveState);

        if(Input.GetMouseButtonDown(0)) 
        {
            if(moveState == MoveState.BeforePick)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, 1000, nodeMask))
                {
                    selectNodeInMove = hitInfo.transform.GetComponent<Node>();

                    if (selectNodeInMove.currentPiece.GetOwner() == GameManager.Instance.turn)
                    {
                        moveState = MoveState.AfterPick;
                    }
                }
            }

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

                    //연결되어있는 빈 노드일 경우, 그 노드로 피스 이동
                    if(selectNode.currentPiece == null && selectNodeInMove.linkedNodes.Contains(selectNode))
                    {
                        //피스이동
                        Piece toMove = selectNodeInMove.currentPiece;
                        selectNode.currentPiece = toMove;
                        toMove.SetNode(selectNode);
                        toMove.SetbMatch(false);
                        toMove.transform.position = new Vector3(selectNode.transform.position.x, toMove.transform.position.y, selectNode.transform.position.z);
                        selectNodeInMove.currentPiece = null;

                        //피스가 3매치였을때, 3매치에 해당하는 보호 flag 비활성화
                        for(int i = 0; i < Check3MatchManager.instance.current3MatchCombinations.Count; i++)
                        {
                            if (Check3MatchManager.instance.current3MatchCombinations[i].list.Contains(selectNodeInMove))
                            {
                                foreach(Node node in Check3MatchManager.instance.current3MatchCombinations[i].list)
                                {
                                    if(node.currentPiece != null)
                                        node.currentPiece.SetbMatch(false);
                                }

                                Check3MatchManager.instance.current3MatchCombinations.Remove(Check3MatchManager.instance.current3MatchCombinations[i]);
                                i--;
                            }
                        }

                        //3매치가 되는지 확인, 되면 Delete State
                        if (Check3MatchManager.instance.Check3Match(selectNode))
                        {
                            GameManager.Instance.SetState(GameManager.EGameState.Delete);
                            OnDeletePieceStart?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            OnMoveEnd?.Invoke(this, EventArgs.Empty);
                        }
                    }

                    //다른 팀의 피스일 경우 선택초기화
                    else if(selectNode.currentPiece.GetOwner() != GameManager.Instance.turn)
                    {
                        moveState = MoveState.BeforePick;
                        selectNodeInMove = null;
                    }

                    //같은 팀의 피스일 경우, 그 피스를 선택
                    else if(selectNode.currentPiece.GetOwner() == GameManager.Instance.turn)
                    {
                        selectNodeInMove = selectNode;
                    }

                }
            }
        }
    }

    //게임 시작 시, 피스를 놓는 동작
    private void PutPieceDown()
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
                    if (node.currentPiece != null) return;

                    Transform piece = Instantiate(piecePrafab, new Vector3(hitInfo.transform.position.x, 0.3f, hitInfo.transform.position.z), Quaternion.identity);
                    piece.GetComponent<MeshRenderer>().material = GameManager.Instance.turn ? turnTrueColor : turnFalseColor;
                    piece.GetComponent<Piece>().SetNode(node);
                    node.currentPiece = piece.GetComponent<Piece>();
                    
                    if(GameManager.Instance.turn)
                    {
                        piece.GetComponent<Piece>().SetOwnerToTrue();
                    }
                    else
                    {
                        piece.GetComponent<Piece>().SetOwnerToFalse();
                    }
                       
                    OnPutPiece?.Invoke(this, EventArgs.Empty);

                    //놓았을 때, 해당 노드와 연결된 3Match가 있는지 확인, 있으면 Delete 모드
                    if(Check3MatchManager.instance.Check3Match(node))
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

    //피스를 삭제하는 동작
    private void DeletePiece()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 1000, nodeMask))
            {
                Node selectNode = hitInfo.transform.GetComponent<Node>();

                if(selectNode.currentPiece.GetOwner() != GameManager.Instance.turn)
                {
                    if(!selectNode.currentPiece.GetbMatch())
                    {
                        Destroy(selectNode.currentPiece.gameObject);
                        selectNode.currentPiece = null;

                        OnDeletePieceEnd?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }
    }
}
