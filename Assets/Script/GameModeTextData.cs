using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TurnText",menuName = "Data/GameModeText")]
public class GameModeTextData : ScriptableObject
{
    [TextArea]
    public string player1Text;
    [TextArea]
    public string player2Text;
    [TextArea]
    public string AIText;
    [TextArea]
    public string myText;
    [TextArea]
    public string opponentText;
}
