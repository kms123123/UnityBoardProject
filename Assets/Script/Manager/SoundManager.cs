using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioSource[] sfx;
    [SerializeField] private AudioSource[] bgm;
    public int currentBgmIndex = 3;

    private void Awake()
    {
        if (instance != null) instance = this;
    }

    private void Start()
    {
        PlayBGM(3);

        BoardManager.instance.OnDeletePieceEnd += SoundManager_OnDeletePieceEnd;
        BoardManager.instance.OnMoveEnd += SoundManager_OnMoveEnd;
        BoardManager.instance.OnPutPiece += SoundManager_OnPutPiece;
        BoardManager.instance.OnDeletePieceStart += SoundManager_OnDeletePieceStart;
        GameManager.Instance.OnGameStart += SoundManager_OnGameStart;
    }

    private void SoundManager_OnGameStart(object sender, EventArgs e)
    {
        int number = UnityEngine.Random.Range(0, 4);
        if (number == currentBgmIndex) return;

        currentBgmIndex = number;
        StopAllBGM();
        PlayBGM(number);
    }

    private void SoundManager_OnDeletePieceStart(object sender, Node e)
    {
        PlaySFX(3);
    }

    private void SoundManager_OnPutPiece(object sender, Node e)
    {
        PlaySFX(3);
    }

    private void SoundManager_OnMoveEnd(object sender, Node e)
    {
        PlaySFX(3);
    }

    private void SoundManager_OnDeletePieceEnd(object sender, DeletePieceEventArgs e)
    {
        PlaySFX(0);
    }

    public void PlayBGM(int index)
    {
        StopAllBGM();

        bgm[index].Play();
    }

    private void StopAllBGM()
    {
        for(int i = 0; i < bgm.Length; i++)
        {
            bgm[i].Stop();
        }
    }

    public void PlaySFX(int index)
    {
        sfx[index].pitch = UnityEngine.Random.Range(.8f, 1.1f);
        sfx[index].Play();
    }
}
