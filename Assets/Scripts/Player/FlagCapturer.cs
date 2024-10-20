using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FlagCapturer : MonoBehaviour
{
    private string _flagtag;
    [SerializeField]private GameObject RedFlag;
    [SerializeField]private GameObject BlueFlag;

    private GameObject OpponentFlag;


    void Start()
    {
       bool check = GetComponent<Player>().MyTeam == Team.Blue;

       _flagtag = check? "RedFlag" : "BlueFlag";
       OpponentFlag = check? RedFlag : BlueFlag;
    }

    // void OnTriggerEnter(Collider other)
    // {
    //     if(other.CompareTag(_flagtag)){
    //         other.GetComponent<Flag>().Captured();
    //         OpponentFlag.SetActive(true);
    //     }
    // }

    public void DropFlag(){
        OpponentFlag.SetActive(false);
    }
}
