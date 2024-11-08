using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    [SerializeField] private GameObject SniperDrone;
    [SerializeField] private GameObject GatlingDrone;

    [SerializeField] private Animator _animator;

    public void SetDrone(int index)
    {
        SniperDrone.SetActive(index == 0);
        GatlingDrone.SetActive(index == 1);
    }

    

}
