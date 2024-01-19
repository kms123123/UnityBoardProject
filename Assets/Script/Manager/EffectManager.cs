using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField] Transform explosionEffect;
    [SerializeField] Transform putEffect;

    [SerializeField] List<Transform> explosionRagdollPrefabs;
    private int prefabIndex;
    [SerializeField] private float explosionForce;

    [SerializeField] CinemachineImpulseSource impulseSource;

    private void Start()
    {
        BoardManager.instance.OnDeletePieceEnd += EffectManager_OnDeletePieceEnd;
        BoardManager.instance.OnPutPiece += EffectManager_OnPutPiece;
        BoardManager.instance.OnMoveEnd += EffectManager_OnMoveEnd;
        BoardManager.instance.OnDeletePieceStart += EffectManager_OnDeletePieceStart;
    }

    private void EffectManager_OnDeletePieceStart(object sender, Node e)
    {
        if (GameManager.Instance.state == GameManager.EGameState.Putting) return;
        Instantiate(putEffect, e.transform.position + new Vector3(0, 0.3f, 0), Quaternion.identity);
        e.currentPiece.GetComponentInChildren<Animator>().SetTrigger("Move");
    }

    private void EffectManager_OnMoveEnd(object sender, Node e)
    {
        Instantiate(putEffect, e.transform.position + new Vector3(0, 0.3f, 0), Quaternion.identity);
        e.currentPiece.GetComponentInChildren<Animator>().SetTrigger("Move");
    }

    //Put 모드 일시, 생성 이펙트 생성
    private void EffectManager_OnPutPiece(object sender, Node e)
    {
        Instantiate(putEffect, e.transform.position + new Vector3(0, 0.3f, 0), Quaternion.identity);
    }

    //삭제 모드 종료시, 폭발 이펙트 생성 및 래그돌 생성 후 물리효과
    private void EffectManager_OnDeletePieceEnd(object sender, DeletePieceEventArgs e)
    {
        Node deleteNode = e.deleteNode;

        Instantiate(explosionEffect, deleteNode.transform.position + new Vector3(0, 0.3f, 0), Quaternion.identity);

        if(e.turn)
        {
            prefabIndex = GameManager.Instance.testForFalseIndex - 1;
        }
        else
        {
            prefabIndex = GameManager.Instance.testForTrueIndex - 1;
        }

        Transform ragdoll = Instantiate(explosionRagdollPrefabs[prefabIndex], deleteNode.transform.position + new Vector3(0, 1f, 0), Quaternion.identity);

        Vector3 explodePosition = new Vector3(ragdoll.position.x + Random.Range(-1f, 1f), ragdoll.position.y - 2f, ragdoll.position.z + Random.Range(-1f, 1f));
        ragdoll.GetComponentInChildren<Rigidbody>().AddExplosionForce(explosionForce, explodePosition, 5f);
        ragdoll.GetComponentInChildren<Rigidbody>().AddTorque(Random.Range(-25, 25), Random.Range(-25, 25), Random.Range(-25, 25));
        Destroy(ragdoll.gameObject, 5f);

        //폭발로 인한 카메라 흔들림 효과
        impulseSource.GenerateImpulse();
    }
}
