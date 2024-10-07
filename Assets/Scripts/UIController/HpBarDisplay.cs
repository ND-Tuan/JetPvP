using System.Collections;
using System.Collections.Generic;
using ObserverPattern;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HpBarDisplay : MonoBehaviour
{   
    [SerializeField] private  int _id;
    [SerializeField] private TextMeshProUGUI _idText;
    [SerializeField] private  TextMeshProUGUI _name;
    [SerializeField] private  Slider _hpBar;

    void Start()
    {
        Observer.AddListener(EvenID.UpdateHp, OnUpdateHp);
    }

    public void SetText(string name){
        _idText.text = _id.ToString();
        _name.text = name;
    }

    public void SetId(int id){
        _id = id;
    }

    public void SetMaxHp(int maxHp){
        _hpBar.maxValue = maxHp;
        _hpBar.value = maxHp;
    }

    public void OnUpdateHp(object[] data){
        if((int)data[0] == _id){
            _hpBar.value = (int)data[1];
        }
    }
    

}
