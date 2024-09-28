using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;

public class CreateAndJoinLobby : MonoBehaviourPunCallbacks
{
    public InputField roomNameCreateInputField;
    public InputField roomNameJoinInputField;
    private List<RoomInfo> _roomList;

    public void CreateRoom()
    {
        Debug.Log("Creating room " + roomNameCreateInputField.text);
        PhotonNetwork.CreateRoom(roomNameCreateInputField.text);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(roomNameJoinInputField.text);
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Game");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Create room failed: " + message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Join room failed: " + message);
    }
}