using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Multiplayer;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDetecter : MonoBehaviour
{
    [SerializeField] float _Range;
    [SerializeField] LayerMask _layer;
    [SerializeField] private Collider[] _hitColliders = new Collider[8]; // collider trong phạm vi dò
    private int _numColliders;

    void Update()
    {   
        if(GameManager.Instance.State == GameState.Waiting) return;
        _numColliders = Physics.OverlapSphereNonAlloc(transform.position, _Range, _hitColliders, _layer);
        Array.Sort(_hitColliders, 0, _numColliders, Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance)));
        for (int i = 0; i < 8; i++)
        {
            if(_hitColliders[i] != null){
                if ( Vector3.Distance(transform.position, _hitColliders[i].transform.position) <= _Range)
                {
                    UIInfoplate infoplate =  _hitColliders[i].GetComponentInChildren<UIInfoplate>();
                    if(infoplate !=null) infoplate.IsOnRange = true;
                } else {
                    UIInfoplate infoplate =  _hitColliders[i].GetComponentInChildren<UIInfoplate>();
                    if(infoplate !=null) infoplate.IsOnRange = false;
                }
            } 
        }
    }

    // private void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(transform.position, _Range);
    // }
}
