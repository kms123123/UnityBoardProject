using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeInfo
{
    public List<NodeInfo> linkedNodes;
    public float x, y;
    public bool is3Match;
    public bool ownedBy;

    public NodeInfo(List<NodeInfo> linkedNodes, float x, float y, bool is3Match, bool ownedBy)
    {
        this.linkedNodes = linkedNodes;
        this.x = x;
        this.y = y;
        this.is3Match = is3Match;
        this.ownedBy = ownedBy;
    }
}
