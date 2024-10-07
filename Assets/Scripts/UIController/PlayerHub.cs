using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ObserverPattern;
using UnityEngine;

public class PlayerHub : MonoBehaviour
{
    [SerializeField] private GameObject _Content;
    [SerializeField] private GameObject _HpBar;
    [SerializeField] private GameObject _HpBarPrefab;
    private List<GameObject> _PlayerList;




    void Start()
    {
        _PlayerList = GameObject.FindGameObjectsWithTag("Player").ToList();
        Observer.AddListener(EvenID.DisplayHp, OnUpdatePlayerList);
        OnDiplayHp();
    }

    


    private void OnDiplayHp(){
        // foreach (var player in _PlayerList)
        // {   
        //     object[] data = player.GetComponent<Player>().GetInfo();

        //     int MaxHp = (int)data[0];

        //     GameObject hpBar = Instantiate(_HpBarPrefab, _Content.transform);
        //     hpBar.GetComponent<HpBarDisplay>().SetMaxHp(MaxHp);
        // }

    }

    private void OnUpdatePlayerList(object[] data){
        if(_PlayerList.Contains((GameObject)data[0])) return;
        _PlayerList.Add((GameObject)data[0]);
        
    }

}
