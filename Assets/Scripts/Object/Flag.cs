using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Flag : NetworkBehaviour
{
    [SerializeField] private GameObject _flag;
    [SerializeField] private Team _flagType;
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
        GetComponentInChildren<ParticleSystem>().Play();
    }

    void Awake()
    {
        rotateObjects = GetComponentsInChildren<RotateObject>();
       
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && GameManager.Instance.State == GameState.Playing){
            FlagCapturer capturer = other.GetComponentInChildren<FlagCapturer>();

            if(capturer == null) return;    
            
            if(capturer.HasFlag && _flagType == capturer.check){
                GameManager.Instance.Winner = _flagType;


                if(_flagType == Team.Blue)
                    PlayerHub.Instance.SetReadyText("Blue Team Win", Color.blue, true);
                else
                    PlayerHub.Instance.SetReadyText("Red Team Win", Color.red, true );

                GameManager.Instance.GameOverTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
                
                Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
            }
        }
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
