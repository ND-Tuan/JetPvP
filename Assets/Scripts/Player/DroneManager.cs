using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Multiplayer;
using UnityEngine;
public enum DroneType
{
    Sniper,
    Gatling
}

public class DroneManager : NetworkBehaviour
{   
    [SerializeField] private PlayerInput input;
    [SerializeField] private GameObject DroneRoot;
    public WeaponBase[] _attackers;
    [Networked, OnChangedRender(nameof(OnChangeDrone))] public DroneType DroneType {get; set;}
    [SerializeField] private GameObject[] SniperDrones;
    [SerializeField] private GameObject[] GatlingDrones;
    [SerializeField] LayerMask _HitMask;
    private RaycastHit[] _hits = new RaycastHit[2];
    [SerializeField] private Transform _RaycastPoint;
    [SerializeField] private Transform _RaycastEndPoint;

    public Vector3 hitPoint {get; private set;}

    public override void Spawned()
    {
        _attackers = DroneRoot.GetComponentsInChildren<WeaponBase>();

        if (!Object.HasStateAuthority)
        {   
            //ẩn trong màn hình chuẩn bị
            foreach(WeaponBase attacker in _attackers){
                attacker.transform.parent.gameObject.SetActive(false);
            }  DroneType = DroneType.Sniper;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority && GameManager.Instance.State != GameState.Waiting){
            //Ngắm bắn và xoay về vị trí mục tiêu
            Aim();
            foreach (var attacker in _attackers){
                attacker.SetRotation(hitPoint);
                attacker.GetComponentInParent<Animator>().enabled = true;

                if(input.CurrentInput.Fire){
                    attacker.Fire();
                }
            }
        } 
    }

    //xử lý việc ngắm bắn
    private void Aim()
    {
        int num = Physics.RaycastNonAlloc(_RaycastPoint.position, _RaycastPoint.forward, _hits, 300, _HitMask);
        Array.Sort(_hits, 0, num, Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance)));
        for (int i = 0; i < num; i++)
        {
            // Kiểm tra collider của hit không phải của người chơi
            if (_hits[i].collider != GetComponent<Collider>())
            {
                hitPoint = _hits[i].point;
                return; // Thoát khi tìm thấy hit mong muốn
            }
        }
        
        hitPoint = _RaycastEndPoint.position;
    }

   

    private void OnChangeDrone()
    {
        if(!Object.HasStateAuthority) return;

        foreach (var drone in SniperDrones)
        {
            drone.SetActive(DroneType == DroneType.Sniper);
        }

        foreach (var drone in GatlingDrones)
        {
            drone.SetActive(DroneType == DroneType.Gatling);
        }

        _attackers = DroneRoot.GetComponentsInChildren<WeaponBase>();
    }
}
