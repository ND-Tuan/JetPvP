using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FlagCapturer : MonoBehaviour
{
    [SerializeField]private GameObject RedFlag;
    [SerializeField]private GameObject BlueFlag;
    [SerializeField] private MeshRenderer DisplayOnMiniMap;
    [SerializeField] private Material[] materials = new Material[2];
    public Team check;
    public bool HasFlag = false;
    private GameObject OpponentFlag;
    private Flag flag;


    public void SetCapturer(Team team)
    {
        check = team;
        
        OpponentFlag = check == Team.Blue? RedFlag : BlueFlag;
        DisplayOnMiniMap.material = materials[check == Team.Blue? 0:1];
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(check == Team.Blue? "RedFlag" : "BlueFlag")){
            flag = other.GetComponent<Flag>();
            flag.HasBeenCaptured = true;
            OpponentFlag.SetActive(true);
            GetComponent<RotateObject>().enabled = true;

            HasFlag = true;
        }

        if(other.CompareTag(check == Team.Red? "RedFlag" : "BlueFlag") && HasFlag){

        }
    }

    public void DropFlag(){
        if(!HasFlag) return;
        OpponentFlag.SetActive(false);
        GetComponent<RotateObject>().enabled = false;
        flag.HasBeenCaptured = false;
        HasFlag = false;
    }
}
