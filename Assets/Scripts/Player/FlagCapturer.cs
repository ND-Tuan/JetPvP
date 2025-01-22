using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FlagCapturer : NetworkBehaviour
{
    [SerializeField]private GameObject RedFlag;
    [SerializeField]private GameObject BlueFlag;
    [SerializeField] private GameObject HasFlagSign;
    public Team check;
    public bool HasFlag = false;
    private GameObject OpponentFlag;
    private Flag flag;


    public void SetCapturer(Team team)
    {
        check = team;
        
        OpponentFlag = check == Team.Blue? RedFlag : BlueFlag;
    }



    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(check == Team.Blue? "RedFlag" : "BlueFlag")){
            flag = other.GetComponent<Flag>();
            RPC_SetFlag(true);
        }
    }

    public void DropFlag(){
        if(!HasFlag) return;
        RPC_SetFlag(false);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetFlag(bool HasCaptured){

        OpponentFlag.SetActive(HasCaptured);
        GetComponent<RotateObject>().enabled = HasCaptured;
        flag.HasBeenCaptured = HasCaptured;
        HasFlag = HasCaptured;
        HasFlagSign.SetActive(HasCaptured);
    }
}
