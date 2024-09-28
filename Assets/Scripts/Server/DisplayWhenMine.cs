using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DisplayWhenMine : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {   
        if(!GetComponentInParent<PhotonView>().IsMine){
            gameObject.SetActive(false);
        }
    }

   
}
