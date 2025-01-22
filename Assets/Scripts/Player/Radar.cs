using System.Collections.Generic;
using Fusion;
using Multiplayer;
using UnityEngine;

public class Radar : NetworkBehaviour
{
    [SerializeField] private float PlayerDetectionRange = 15f;  // Phạm vi phát hiện người chơi
    [SerializeField] private LayerMask MissileLayer;
    private Vector3 viewportPoint;
    
    private void Update()
    {
        PlayerDetect();
        
    }

    private void PlayerDetect(){
        if(GameManager.Instance.State != GameState.Playing) return;
        foreach (KeyValuePair<PlayerRef, Player> player in GameManager.Instance.Players)
        {
            if (player.Value == null) continue;
            if (player.Value == GameManager.Instance._player) continue;
            if (player.Value.State == Player.PlayerState.Death) continue;

            float distance = Vector3.Distance(player.Value.transform.position, GameManager.Instance._player.transform.position);
            UIInfoplate infoplate = player.Value.GetComponentInChildren<UIInfoplate>();

            if(infoplate == null) continue;

            if (distance <= PlayerDetectionRange)
            {
                viewportPoint = Camera.main.WorldToViewportPoint(player.Value.transform.position);   
                bool isInViewport = viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1;

                infoplate.IsOnRange = isInViewport;
                continue;
            } 
            
            infoplate.IsOnRange = false;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_MissileDetecter(bool isDetect)
    {
        if(Object.HasStateAuthority)
            PlayerHub.Instance.SetMissileCaution(isDetect);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, PlayerDetectionRange);
    }
}
