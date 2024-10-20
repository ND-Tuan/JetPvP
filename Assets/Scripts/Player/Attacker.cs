using System.Collections;
using System.Collections.Generic;
using Fusion;
using Multiplayer;
using UnityEngine;

public class Attacker : NetworkBehaviour, IAttack
{
    [SerializeField] private int _damage = 10;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _FirePos;
    [SerializeField] private Cooldown _cooldown;
    [SerializeField] private ParticleSystem _particle;


    public Vector3 _currentDirection = default;
    [Networked] private Angle angleY { get; set; }
    [Networked] private Angle angleX { get; set; }
    private Weapon _weapon;

    
    public override void Spawned()
    {
        _currentDirection = transform.forward;
        _weapon = GetComponent<Weapon>();
    }

   
    public void SetRotation(Vector3 Diraction){
        RPC_Rotation(Diraction);
    }

    public void Attack(Team team)
    {
        if(_cooldown.IsCoolingDown) return;
        _weapon.Fire(Runner,Object.InputAuthority,transform.forward);
        _particle.Play();
        _cooldown.StartCooldown();

    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_Rotation(Vector3 Diraction)
    {
        transform.forward = Diraction;
    }

    
}
