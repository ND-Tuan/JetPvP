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
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private Slider _hpBar;
    [SerializeField] private Image _hpFill;

    public override void Spawned()
    {
        transform.SetParent(PlayerHub.Instance._Content.transform);
    }

    
    public void SetInfo(object[] data){

        _HpText.text = (string)data[0];
        _hpFill.color = (Color)data[2];

        if(Object.HasStateAuthority)
        _name.text = (string)data[1];
    }

    public void UpdateHP(int currentHP, int maxHP){
        if(currentHP <0) currentHP = 0;
		_hpBar.value = currentHP;
		_hpBar.maxValue = maxHP;

        _HpText.text = currentHP.ToString();
	}
    

}
