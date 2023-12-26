using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public event EventHandler OnPutPiece;

    public static BoardManager instance;

    public List<Node> gameBoard = new List<Node>();
    [SerializeField] private Transform piecePrafab;

    [SerializeField] private Material turnTrueColor;
    [SerializeField] private Material turnFalseColor;



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
    }

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

                    OnPutPiece?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
