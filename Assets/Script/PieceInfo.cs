using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceInfo
{
    public Node currentNode = null;
    public bool is3Match = false;
    public bool ownedBy = false;

    public Node GetNode() { return currentNode; }
    public void SetNode(Node node) { currentNode = node; }
    public bool GetbMatch() { return is3Match; }
    public void SetbMatch(bool bMatch) { is3Match = bMatch; }
    public void SetOwnerToTrue() { ownedBy = true; }
    public void SetOwnerToFalse() { ownedBy = false; }
    public bool GetOwner() { return ownedBy; }
}
