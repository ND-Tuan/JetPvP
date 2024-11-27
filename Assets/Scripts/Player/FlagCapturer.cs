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
    [SerializeField] private MeshRenderer DisplayOnMiniMap;
    [SerializeField] private Material[] materials = new Material[2];
    [SerializeField] private Image Flag;
    public Team check;
    public bool HasFlag = false;
    private GameObject OpponentFlag;
    private Flag flag;

    public override void FixedUpdateNetwork()
	{   if(Object.HasStateAuthority) return;
        Flag.gameObject.SetActive(HasFlag);
		if(HasFlag)
			Flag.transform.position =  Camera.main.WorldToScreenPoint(transform.parent.position + new Vector3(0,0.25f,0));
	}

    public void SetCapturer(Team team)
    {
        check = team;
        
        OpponentFlag = check == Team.Blue? RedFlag : BlueFlag;
        DisplayOnMiniMap.material = materials[check == Team.Blue? 0:1];
        Flag.color = check == Team.Red? new Color(0,152,255) : Color.red;
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
