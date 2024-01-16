using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInfo : MonoBehaviour
{
    private int avatarIndex;
    private string userName;
    private int eloRating;

    public void SetAvatarIndex(int index)
    {
        this.avatarIndex = index;
    }

    public int GetAvatarIndex() 
    {
        return this.avatarIndex;
    }
}
