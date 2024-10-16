using System.Collections;
using System.Collections.Generic;
using Starter;
using UnityEngine;

public class Flag : MonoBehaviour
{
    [SerializeField] private GameObject _flag;
    private RotateObject[] rotateObjects;

    void Start()
    {
        rotateObjects = GetComponentsInChildren<RotateObject>();
    }

    public void Captured(){
        _flag.SetActive(false);
        foreach(RotateObject rotate in rotateObjects){
            rotate.enabled = false;
        }
    }

    public void TakeBack(){
        _flag.SetActive(true);
        foreach(RotateObject rotate in rotateObjects){
            rotate.enabled = true;
        }
    }
}
