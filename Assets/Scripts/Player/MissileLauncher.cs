using System.Collections;
using System.Collections.Generic;
using Fusion;
using Multiplayer;
using UnityEngine;

public class MissileLauncher : NetworkBehaviour
{
    [SerializeField] private WeaponBase _missileLauncher;
    [SerializeField] private float _cooldown;
    [Networked] private TickTimer _Cooldown { get; set; }
    
    private PlayerInput _input;


    public override void Spawned()
    {
        _input = GetComponentInParent<PlayerInput>();
        //Runner.SimulationTime
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if(GameManager.Instance.State != GameState.Playing) return;
        
        //display missile bar progress
        if (_Cooldown.IsRunning && !_Cooldown.Expired(Runner))
        {
            PlayerHub.Instance.OnUpdateMissileBar(1 - (_Cooldown.RemainingTime(Runner).GetValueOrDefault() / _cooldown));
            return;
        }
       
        PlayerHub.Instance.OnUpdateMissileBar(1);
        if (_input.CurrentInput.FireMissile)
        {
            Debug.Log("Fire Missile");
            _missileLauncher.Fire();
            _Cooldown = TickTimer.CreateFromSeconds(Runner, _cooldown);
        }
        
    }

}
