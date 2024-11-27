using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Fusion;
using Multiplayer;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDetecter : MonoBehaviour
{
    public float detectionRange = 10f;  // Phạm vi hiển thị
    private Vector3 viewportPoint;
    
    
    private void Update()
    {
        foreach (KeyValuePair<PlayerRef, Player> player in GameManager.Instance.Players)
        {
            if (player.Value == null) continue;
            if (player.Value == GameManager.Instance._player) continue;

            float distance = Vector3.Distance(player.Value.transform.position, transform.position);
            UIInfoplate infoplate = player.Value.GetComponentInChildren<UIInfoplate>();

            if (distance <= detectionRange)
            {
                viewportPoint = Camera.main.WorldToViewportPoint(player.Value.transform.position);   
                bool isInViewport = viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1;

                infoplate.IsOnRange = isInViewport;
            } else {
                infoplate.IsOnRange = false;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
