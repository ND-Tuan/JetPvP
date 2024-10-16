using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class BulletHit : NetworkBehaviour
{
    [Networked]
    private TickTimer life { get; set; }

    [SerializeField] private float bulletSpeed = 5f;
    


    public Team team;
    

    public override void Spawned()
    {
        GetComponentInChildren<TrailRenderer>().Clear();
        life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        
    }

    

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner)){
            Runner.Despawn(Object);
        }
        else{
            transform.position += bulletSpeed * transform.forward * Runner.DeltaTime;
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player" && other.gameObject.GetComponent<Player>().MyTeam != team)
        {
            Debug.Log("Hit Enemy");
        }
    }
}
