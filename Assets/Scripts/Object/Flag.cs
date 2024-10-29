using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Flag : NetworkBehaviour
{
    [SerializeField] private GameObject _flag;
    private RotateObject[] rotateObjects;
    [Networked, HideInInspector, OnChangedRender(nameof(OnStateChanged))] public bool HasBeenCaptured {get; set;}

    private void OnStateChanged()
    {
        if(HasBeenCaptured) Captured();
        else TakeBack();
    }

    public override void Spawned()
    {
       HasBeenCaptured = false;
    }

    void Awake()
    {
        rotateObjects = GetComponentsInChildren<RotateObject>();
        
    }

    private void Captured(){
        _flag.SetActive(false);
        foreach(RotateObject rotate in rotateObjects){
            rotate.enabled = false;
        }
    }

    private void TakeBack(){
        _flag.SetActive(true);
        foreach(RotateObject rotate in rotateObjects){
            rotate.enabled = true;
        }
    }
}
