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
    [SerializeField] private GameObject DeathPanel;
    [SerializeField] private TextMeshProUGUI CooldownText;
    [SerializeField] private TextMeshProUGUI BlueScoreText;
    [SerializeField] private TextMeshProUGUI RedScoreText;
    [SerializeField] private Animator Flash;
    
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
        if(value <0) value = 0;
        _HpBar.maxValue = maxValue;
        _HpBar.value = value;
    }

    public void Ready(){
        GameManager.Instance._player.RPC_SetReady(true);
        ReadyMenu.SetActive(false);
        if(GameManager.Instance.State == GameState.Waiting)
            SetReadyText("Waiting for other players...", Color.white);
    }

    public void SetReadyText(string text, Color color){
        _ReadyStateText.color = color;
        _ReadyStateText.text = text;
    }

    public void SetScore(Team team, int score){
        if(team == Team.Blue){
            BlueScoreText.text = score.ToString();
        } else {
            RedScoreText.text = score.ToString();
        }
    }

    public void SetFlash(bool active){
        if(active){
            Flash.Play("Flash");
        } else {
            Flash.Play("FadeOut");
        }
    }

    public void SetPlaying(){
        
        _ReadyStateText.text = "";
        HUBPanel.SetActive(true);
    }

    public void SetStatusDisplay(bool IsAlive){
        HUBPanel.SetActive(IsAlive);
        DeathPanel.SetActive(!IsAlive);
    }

    public void UpdateRespawnTime(int time){
        if(time<0) time = 0;
        CooldownText.text = "Revive in: " + time.ToString()+"s";
    }
}
