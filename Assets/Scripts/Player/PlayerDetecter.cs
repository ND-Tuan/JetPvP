using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Multiplayer;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDetecter : MonoBehaviour
{
    public float detectionRange = 10f;  // Phạm vi hiển thị
    public LayerMask playerLayer;       // Lớp của người chơi để xác định mục tiêu
    [SerializeField] private Collider[] detectedPlayers= new Collider[8]; // Bộ đệm để lưu trữ các collider trong tầm
    

    private void Update()
    {
        // Đếm số collider của người chơi trong vùng
        int numPlayersInRange = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, detectedPlayers, playerLayer);
        // Hiển thị người chơi trong tầm
        for (int i = 0; i < numPlayersInRange; i++)
        {   
            UIInfoplate infoplate = detectedPlayers[i].GetComponentInChildren<UIInfoplate>();

            Vector3 viewportPoint = Camera.main.WorldToViewportPoint(detectedPlayers[i].transform.position);
            bool isInViewport = viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1;

            if(infoplate !=null) infoplate.IsOnRange = isInViewport;
        }

        // Tắt hiển thị người chơi ngoài tầm
        for (int i = numPlayersInRange; i < detectedPlayers.Length; i++)
        {
            if (detectedPlayers[i] != null)
            {
                UIInfoplate infoplate = detectedPlayers[i].GetComponentInChildren<UIInfoplate>();
                if(infoplate !=null) infoplate.IsOnRange = false;
                detectedPlayers[i] = null; // Reset lại bộ đệm để tránh lỗi lần sau
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
