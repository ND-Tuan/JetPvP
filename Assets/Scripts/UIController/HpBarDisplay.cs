using System.Collections;
using System.Collections.Generic;
using Fusion;
using ObserverPattern;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HpBarDisplay : NetworkBehaviour
{   
    [SerializeField] private TextMeshProUGUI _HpText;
    [SerializeField] private  TextMeshProUGUI _name;
    [SerializeField] private  Slider _hpBar;
    [SerializeField] private Image _hpFill;

    public override void Spawned()
    {
        transform.SetParent(PlayerHub.Instance._Content.transform);
    }

    public void SetInfo(object[] data){
        int MaxHp = (int)data[0];
        string name = (string)data[1];
        Color color  = (Color)data[2];

        _HpText.text = MaxHp.ToString();
        _name.text = name;
        _hpFill.color = color;
    }

    public void SetMaxHp(int maxHp){
        _hpBar.maxValue = maxHp;
        _hpBar.value = maxHp;
    }

    public void UpdateHP(int currentHP, int maxHP){
		_hpBar.value = currentHP;
		_hpBar.maxValue = maxHP;

        _HpText.text = currentHP.ToString();
	}
    

}
