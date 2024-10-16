using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Multiplayer;
using ObserverPattern;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHub : MonoBehaviour
{
    [SerializeField] private GameObject _Content;
    [SerializeField] private GameObject _HpBar;
    [SerializeField] private GameObject _HpBarPrefab;
    [SerializeField] private Slider _EnergyBar;
    [SerializeField] private Image _EnergyBarFill;
    private List<Player> _PlayerList;


    void Start()
    {
        _PlayerList = GameManager.Instance.Players;
        OnDiplayHp();
    }

    


    private void OnDiplayHp(){
        foreach (var player in _PlayerList)
        {   
            object[] data = player.GetInfo();

            int MaxHp = (int)data[0];
            string name = (string)data[1];
            Color color  = (Color)data[2];

            GameObject hpBar = Instantiate(_HpBarPrefab, _Content.transform);

            HpBarDisplay display =  hpBar.GetComponent<HpBarDisplay>();

            display.SetInfo(MaxHp, name, color);
            player._HpDisplay = display;
            
        }

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
}
