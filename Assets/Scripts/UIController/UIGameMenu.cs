using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Text;

namespace Starter
{
	/// <summary>
	/// Shows in-game menu, handles player connecting/disconnecting to the network game and cursor locking.
	/// </summary>
	public class UIGameMenu : MonoBehaviour
	{
		[Header("Start Game Setup")]
		[Tooltip("Specifies which game mode player should join - e.g. Platformer, ThirdPersonCharacter")]
		public string GameModeIdentifier;
		public NetworkRunner RunnerPrefab;
		private int[] MaxCountSelection = {2, 4, 6, 8};
		private int MaxPlayerCount = 8;

		[Header("Debug")]
		[Tooltip("For debug purposes it is possible to force single-player game (starts faster)")]
		public bool ForceSinglePlayer;

		[Header("UI Setup")]
		[SerializeField] private GameObject[] FuctionPanels;
		[SerializeField] private Image[] FuctionPanelChangeButtons;

		public CanvasGroup PanelGroup;

		public TMP_InputField RoomText;
		public TMP_InputField NicknameText;
		public TextMeshProUGUI StatusText;
		public TextMeshProUGUI MaxPlayerCountText;
		private bool _isPrivate;
		public GameObject StartGroup;
		public GameObject DisconnectGroup;

		[Header("Settings")]
		[SerializeField] private Slider _volumeSoundSlider;
		[SerializeField] private Slider _volumeMusicSlider;

		private NetworkRunner _runnerInstance;
		private static string _shutdownStatus;
		[SerializeField] private GameObject _jetFake;

		private static readonly System.Random _random = new System.Random();

		

		public async void StartGame(string roomName)
		{
			await Disconnect();
			

			_runnerInstance = Instantiate(RunnerPrefab);

			// Add listener for shutdowns so we can handle unexpected shutdowns
			var events = _runnerInstance.GetComponent<NetworkEvents>();
			events.OnShutdown.AddListener(OnShutdown);

			var sceneInfo = new NetworkSceneInfo();
			sceneInfo.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));

			var startArguments = new StartGameArgs()
			{
				GameMode = Application.isEditor && ForceSinglePlayer ? GameMode.Single : GameMode.Shared,
				SessionName = roomName,
				PlayerCount = MaxPlayerCount,
				// We need to specify a session property for matchmaking to decide where the player wants to join.
				// Otherwise players from Platformer scene could connect to ThirdPersonCharacter game etc.
				SessionProperties = new Dictionary<string, SessionProperty> {["GameMode"] = GameModeIdentifier},
				Scene = sceneInfo,
			};

			StatusText.text = startArguments.GameMode == GameMode.Single ? "Starting single-player..." : "Connecting...";

			var startTask = _runnerInstance.StartGame(startArguments);
			await startTask;

			if (startTask.Result.Ok)
			{
				StatusText.text = "";
				RoomText.text = roomName;
				SwitchPanel(2);
				FuctionPanelChangeButtons[1].gameObject.SetActive(false);
				_jetFake.SetActive(false);
				PanelGroup.gameObject.SetActive(false);
				
			}
			else
			{
				StatusText.text = $"Connection Failed: {startTask.Result.ShutdownReason}";
			}
		}

		public void JoinGame()
		{
			StartGame(RoomText.text);
		}

		public void CreateGame()
		{
			StartGame(GenerateRoomName());
			_runnerInstance.SessionInfo.IsVisible = !_isPrivate;
		}

		public void SetPrivate(bool isPrivate)
		{
			_isPrivate = isPrivate;
		}

		private static string GenerateRoomName()
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			StringBuilder result = new StringBuilder(6);
			for (int i = 0; i < 6; i++)
			{
				result.Append(chars[_random.Next(chars.Length)]);
			}
			return result.ToString();
		}

		public void SetMaxPlayerCount(bool isNext)
		{
			int index = Array.IndexOf(MaxCountSelection, MaxPlayerCount);
			index += isNext ? 1 : -1;
			if (index < 0)
			{
				index = MaxCountSelection.Length - 1;
			}
			else if (index >= MaxCountSelection.Length)
			{
				index = 0;
			}
			MaxPlayerCount = MaxCountSelection[index];
			MaxPlayerCountText.text = MaxPlayerCount.ToString();
		}


		//===========================================
		//======Panel switch=========================
		public void SwitchPanel(int panel)
		{
			Color MilkWhite = new Color(254, 253, 245);
			for (int i = 0; i < FuctionPanels.Length; i++)
			{
				FuctionPanels[i].SetActive(i == panel);
				FuctionPanelChangeButtons[i].color = i == panel ? MilkWhite : Color.gray;
			}

			PlayerPrefs.SetString("PlayerName", NicknameText.text);
		}

		//===========================================
		//======Sound settings=======================
		public void SetMute(bool isMute)
		{
			SoundManager.Instance.SetMute(isMute);
		}

		public void SetVolume()
		{
			SoundManager.Instance.SetFXVolume(_volumeSoundSlider.value);
			SoundManager.Instance.SetMusicVolume(_volumeMusicSlider.value);
		}
		//===========================================
		//===========================================

		public async void DisconnectClicked()
		{
			await Disconnect();
		}

		public async void BackToMenu()
		{
			await Disconnect();

			SceneManager.LoadScene(0);
		}

		public void TogglePanelVisibility()
		{
			if (PanelGroup.gameObject.activeSelf && _runnerInstance == null)
				return; // Panel cannot be hidden if the game is not running

			PanelGroup.gameObject.SetActive(!PanelGroup.gameObject.activeSelf);

			if(!PanelGroup.gameObject.activeSelf){
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}

			if(GameManager.Instance == null) return;
			if(GameManager.Instance.State != GameState.Playing)
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
		}

		private void OnEnable()
		{
			var nickname = PlayerPrefs.GetString("PlayerName");
			if (string.IsNullOrEmpty(nickname))
			{
				nickname = "Player" + UnityEngine.Random.Range(10000, 100000);
			}

			NicknameText.text = nickname;

			// Try to load previous shutdown status
			StatusText.text = _shutdownStatus != null ? _shutdownStatus : string.Empty;
			_shutdownStatus = null;
		}

		private void Update()
		{
			// Enter/Esc key is used for locking/unlocking cursor in game view.
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
			{
				TogglePanelVisibility();
			}

			if (PanelGroup.gameObject.activeSelf)
			{
				StartGroup.SetActive(_runnerInstance == null);
				DisconnectGroup.SetActive(_runnerInstance != null);
				RoomText.interactable = _runnerInstance == null;
				NicknameText.interactable = _runnerInstance == null;

				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		public async Task Disconnect()
		{
			if (_runnerInstance == null)
				return;

			StatusText.text = "Disconnecting...";
			PanelGroup.interactable = false;

			// Remove shutdown listener since we are disconnecting deliberately
			var events = _runnerInstance.GetComponent<NetworkEvents>();
			events.OnShutdown.RemoveListener(OnShutdown);

			await _runnerInstance.Shutdown();
			_runnerInstance = null;
			_jetFake.SetActive(true);
			FuctionPanelChangeButtons[1].gameObject.SetActive(true);

			// Reset of scene network objects is needed, reload the whole scene
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

		private void OnShutdown(NetworkRunner runner, ShutdownReason reason)
		{
			// Unexpected shutdown happened (e.g. Host disconnected)

			// Save status into static variable, it will be used in OnEnable after scene load
			_shutdownStatus = $"Shutdown: {reason}";
			Debug.LogWarning(_shutdownStatus);

			// Reset of scene network objects is needed, reload the whole scene
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

		
	}
}
