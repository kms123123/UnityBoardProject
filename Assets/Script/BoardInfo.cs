using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardInfo
{
    private List<NodeInfo> boardNodes;
    private int turnTrueHavetoPut;
    private int turnFalseHavetoPut;
    private int totalTruePiece;
    private int totalFalsePiece;
    private int turnTrue3PiecesMoves;
    private int turnFalse3PiecesMoves;

    public BoardInfo()
    {
        boardNodes = new List<NodeInfo>();
    }

    public void SetBoardInfo(List<NodeInfo> nodeList)
    {
        boardNodes = nodeList;
    }
}
