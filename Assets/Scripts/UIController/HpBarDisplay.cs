using System.Collections;
using System.Collections.Generic;
using ObserverPattern;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HpBarDisplay : MonoBehaviour
{   
    [SerializeField] private TextMeshProUGUI _HpText;
    [SerializeField] private  TextMeshProUGUI _name;
    [SerializeField] private  Slider _hpBar;
    [SerializeField] private Image _hpFill;


    public void SetInfo(int MaxHp, string name, Color color){
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
