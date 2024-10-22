using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Multiplayer;
using ObserverPattern;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHub : MonoBehaviour
{
    public GameObject _Content;
    [SerializeField] private GameObject _HpBarPrefab;
    [SerializeField] private Slider _HpBar;
    [SerializeField] private Slider _EnergyBar;
    [SerializeField] private Image _EnergyBarFill;
    [SerializeField] private TextMeshProUGUI _ReadyStateText;
    [SerializeField] private GameObject ReadyMenu;
   
    [SerializeField] private GameObject HUBPanel; 
    
    private List<Player> _PlayerList;
    private List<GameObject> _HpBarList;

    public static PlayerHub Instance;

    void Awake()
    {   
         //triển khai Singleton
        if (Instance == null){
            Instance = this;
        } else if (Instance != this){
            Destroy(gameObject);
        }
        
        gameObject.SetActive(false);
        _PlayerList = new();
    }

    public void OnUndisplayHp(Player player){
        
    }

    public void OnUpdateEnergyBar(float value, float maxValue, bool isRegen){
        Color color = _EnergyBarFill.color;
        if(isRegen){
            color.a = 200/255f;
        } else {
            color.a = 135/255f;
        }
        _EnergyBarFill.color = color;

        _EnergyBar.maxValue = maxValue;
        _EnergyBar.value = value;
    }

    public void OnUpdateHpBar(float value, float maxValue){

        _HpBar.maxValue = maxValue;
        _HpBar.value = value;
    }

    public void Ready(){
        GameManager.Instance._player.RPC_SetReady(true);
        ReadyMenu.SetActive(false);
        if(GameManager.Instance.State == GameState.Waiting)
            _ReadyStateText.text = "Waiting for other players...";
    }

    public void SetPlaying(){
        
        _ReadyStateText.gameObject.SetActive(false);
        HUBPanel.SetActive(true);
    }
}
