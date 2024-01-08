using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ListWrapper
{
    public List<Node> list;
}


public class Check3MatchManager : MonoBehaviour
{
    public static Check3MatchManager instance;

    private void Awake()
    {
        if (instance != null) return;
        instance = this;
    }

    [SerializeField] private List<ListWrapper> matchCombinations;
    public List<ListWrapper> current3MatchCombinations = new List<ListWrapper>();

    //Check Whether there is 3 match in certain Time
    public bool Check3Match(Node node)
    {
        bool flag = false;
        for (int i = 0; i < matchCombinations.Count; i++)
        {
            if (!matchCombinations[i].list.Contains(node)) continue;

            if (matchCombinations[i].list[0].currentPiece == null ||
                matchCombinations[i].list[1].currentPiece == null ||
                matchCombinations[i].list[2].currentPiece == null)
                    continue;

            if (matchCombinations[i].list[0].currentPiece.GetOwner() == matchCombinations[i].list[1].currentPiece.GetOwner()
                && matchCombinations[i].list[1].currentPiece.GetOwner() == matchCombinations[i].list[2].currentPiece.GetOwner())
            {
                for(int j = 0; j < matchCombinations[i].list.Count;j++)
                {
                    matchCombinations[i].list[j].currentPiece.SetbMatch(true);
                }
                current3MatchCombinations.Add(matchCombinations[i]);
                flag = true;
            }
        }

        return flag;
    }

    public bool Check3MatchAndDeleteFlag(Node node)
    {
        bool flag = false;
        for (int i = 0; i < matchCombinations.Count; i++)
        {
            if (!matchCombinations[i].list.Contains(node)) continue;

            if (matchCombinations[i].list[0].currentPiece == null ||
                matchCombinations[i].list[1].currentPiece == null ||
                matchCombinations[i].list[2].currentPiece == null)
                continue;

            if (matchCombinations[i].list[0].currentPiece.GetOwner() == matchCombinations[i].list[1].currentPiece.GetOwner()
                && matchCombinations[i].list[1].currentPiece.GetOwner() == matchCombinations[i].list[2].currentPiece.GetOwner())
            {
                for (int j = 0; j < matchCombinations[i].list.Count; j++)
                {
                    matchCombinations[i].list[j].currentPiece.SetbMatch(false);
                }
                current3MatchCombinations.Remove(matchCombinations[i]);
                flag = true;
            }
        }

        return flag;
    }

    public List<ListWrapper> GetPossibleCombinations()
    {
        return matchCombinations;
    }
}
