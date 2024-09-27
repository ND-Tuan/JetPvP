using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;

public class CreateAndJoinLobby : MonoBehaviourPunCallbacks
{
    public InputField roomNameCreateInputField;
    public InputField roomNameJoinInputField;

    public void CreateRoom()
    {
        Debug.Log("Creating room " + roomNameCreateInputField.text);
        PhotonNetwork.CreateRoom(roomNameCreateInputField.text);
    }

    public void JoinRoom()
    {
        Debug.Log("Joining room " + roomNameJoinInputField.text);
        PhotonNetwork.JoinRoom(roomNameJoinInputField.text);
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Game");
    }
}