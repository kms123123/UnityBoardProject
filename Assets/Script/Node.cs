using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> linkedNodes;
    public float x, y;
    public Piece currentPiece;

    private void Awake()
    {
        linkedNodes = new List<Node>();
    }

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


}
