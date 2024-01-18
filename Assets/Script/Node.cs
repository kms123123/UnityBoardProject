using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> linkedNodes;
    public float x, y;
    public Piece currentPiece;
    public PieceInfo pieceInfo;

    [SerializeField] private GameObject CanSelectObj;
    [SerializeField] private GameObject CannotSelectObj;

    [Header("Debugging Test")]
    public bool isOwned;
    public bool isMatched;
    public bool whoOwned;

    private void Start()
    {
        BoardManager.instance.gameBoard.Add(this);
    }

    private void OnValidate()
    {
        x = transform.position.x;
        y = transform.position.z;

        gameObject.name = string.Format("Node ({0}, {1})", x, y);
    }

    private void Update()
    {

        if (pieceInfo != null) isOwned = true;
        else isOwned = false;

        if(isOwned)
        {
            whoOwned = pieceInfo.GetOwner();
            isMatched = pieceInfo.GetbMatch();
        }
        else
        {
            whoOwned=false;
            isMatched = false;
        }
        
    }

    public void CanSelectNode()
    {
        CannotSelectObj.SetActive(false);
        CanSelectObj.SetActive(true);
    }

    public void CannotSelectNode()
    {
        CannotSelectObj.SetActive(true);
        CanSelectObj.SetActive(false);
    }

    public void DisableSelectObj()
    {
        CannotSelectObj.SetActive(false);
        CanSelectObj.SetActive(false);
    }

}
