using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class DroneSelectPanel : MonoBehaviour
{
    [SerializeField] private Sprite[] IconsSprite;
    [SerializeField] private Image IconImage;
    [SerializeField] private TextMeshProUGUI DroneName;

    public void SetDrone(int index)
    {
        IconImage.sprite = IconsSprite[index];
        DroneName.text = index==0? "Rifle Drone" : "Gatling Drone";

        GameManager.Instance._player.GetComponent<DroneManager>().DroneType = index == 0 ? DroneType.Sniper : DroneType.Gatling;
    }

}
