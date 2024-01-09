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

            if (matchCombinations[i].list[0].pieceInfo == null ||
                matchCombinations[i].list[1].pieceInfo == null ||
                matchCombinations[i].list[2].pieceInfo == null)
                    continue;

            if (matchCombinations[i].list[0].pieceInfo.GetOwner() == matchCombinations[i].list[1].pieceInfo.GetOwner()
                && matchCombinations[i].list[1].pieceInfo.GetOwner() == matchCombinations[i].list[2].pieceInfo.GetOwner())
            {
                for(int j = 0; j < matchCombinations[i].list.Count;j++)
                {
                    matchCombinations[i].list[j].pieceInfo.SetbMatch(true);
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

            if (matchCombinations[i].list[0].pieceInfo == null ||
                matchCombinations[i].list[1].pieceInfo == null ||
                matchCombinations[i].list[2].pieceInfo == null)
                continue;

            if (matchCombinations[i].list[0].pieceInfo.GetOwner() == matchCombinations[i].list[1].pieceInfo.GetOwner()
                && matchCombinations[i].list[1].pieceInfo.GetOwner() == matchCombinations[i].list[2].pieceInfo.GetOwner())
            {
                for (int j = 0; j < matchCombinations[i].list.Count; j++)
                {
                    matchCombinations[i].list[j].pieceInfo.SetbMatch(false);
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
