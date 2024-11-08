using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
public enum DroneType
{
    Sniper,
    Gatling
}

public class DroneManager : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnChangeDrone))] public DroneType DroneType {get; set;}
    [SerializeField] private GameObject[] SniperDrones;
    [SerializeField] private GameObject[] GatlingDrones;


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

        GetComponent<Player>()._attackers = GetComponentsInChildren<WeaponBase>();
    }
}
