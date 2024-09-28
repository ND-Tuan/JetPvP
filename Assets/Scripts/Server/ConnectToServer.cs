
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.UI;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    [SerializeField] private InputField _playerNameInputField;
    [SerializeField] private GameObject _InputPanel;
    [SerializeField] private GameObject _ConnectingPanel;
    public void LogToGame(){
        _InputPanel.SetActive(false);
        _ConnectingPanel.SetActive(true);

        //connect to the photon server
        PhotonNetwork.LocalPlayer.NickName = _playerNameInputField.text;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();

    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        SceneManager.LoadScene("Lobby");
    }
}