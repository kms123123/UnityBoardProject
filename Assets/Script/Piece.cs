using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private Node currentNode = null;

    public Node GetNode() { return currentNode; }
    public void SetNode(Node node) {  currentNode = node; }
}
