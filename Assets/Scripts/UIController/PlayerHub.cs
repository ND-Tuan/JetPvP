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
    [SerializeField] private Slider _MissileBar;
    [SerializeField] private Image _MissileBarFill;
    [SerializeField] private TextMeshProUGUI _ReadyStateText;
    [SerializeField] private GameObject ReadyMenu;
    [SerializeField] private GameObject HUBPanel;
    [SerializeField] private GameObject DeathPanel;
    [SerializeField] private TextMeshProUGUI CooldownText;
    [SerializeField] private TextMeshProUGUI BlueScoreText;
    [SerializeField] private TextMeshProUGUI RedScoreText;
    [SerializeField] private Animator Flash;
    [SerializeField] private GameObject DarkPanel;
    [SerializeField] private GameObject FinalWinPanel;
    [SerializeField] private TextMeshProUGUI[] FinalScoreText;
    [SerializeField] private GameObject WinText;
    
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
        
        DarkPanel.SetActive(false);
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

    public void OnUpdateMissileBar(float value){
        _MissileBar.value = value;

        if(value >=1){
            _MissileBarFill.color = new Color(1.0f, 0.65f, 0.0f);
            _MissileBar.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
        } else {
            _MissileBarFill.color = Color.white;
            _MissileBar.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
        }
    }

    public void Ready(){
        GameManager.Instance._player.RPC_SetReady(true);
        ReadyMenu.SetActive(false);
        if(GameManager.Instance.State == GameState.Waiting)
            SetReadyText("Waiting for other players...", Color.white, false);
    }

    public void SetReadyText(string text, Color color, bool needDark = false){
        _ReadyStateText.color = color;
        _ReadyStateText.text = text;
        
        DarkPanel.SetActive(needDark);
        
    }

    public void SetScore(Team team, int score){
        string text = score.ToString();
        if(score < 10) text = "0" + score.ToString();

        if(team == Team.Blue){
            BlueScoreText.text = text;
        } else {
            RedScoreText.text = text;
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

    public void DisplayFinalWin(float ratio){
         FinalWinPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        FinalScoreText[0].text = "";
        FinalScoreText[1].text = "";

        FinalWinPanel.SetActive(true);
        DarkPanel.SetActive(true);
        WinText.SetActive(false);

        Debug.Log("Ratio: " + ratio);
        StartCoroutine(DisplayFinalWinCoroutine(ratio));
        
    }

    private IEnumerator DisplayFinalWinCoroutine(float ratio)
    {   
        float duration = 1f; // Duration in seconds
        float elapsedTime = 0;
        int blue = 0;
        int red = 0;

        yield return new WaitForSeconds(1.2f);

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            
            blue = (int)Mathf.Lerp(0, GameManager.Instance.BlueScore, t);
            FinalScoreText[0].text = blue.ToString();
           
            red = (int)Mathf.Lerp(0, GameManager.Instance.RedScore, t);
            FinalScoreText[1].text = red.ToString();

            float X = Mathf.Lerp(0, 500*ratio, t);
            Debug.Log(X);
            FinalWinPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(X, 50);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        FinalScoreText[0].text = GameManager.Instance.BlueScore.ToString();
        FinalScoreText[1].text = GameManager.Instance.RedScore.ToString();
        FinalWinPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(500*ratio, 50);


        if(ratio > 0)
            WinText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-550, 95);
        else
            WinText.GetComponent<RectTransform>().anchoredPosition = new Vector2(560, -75);

        WinText.SetActive(true);
    }
}
