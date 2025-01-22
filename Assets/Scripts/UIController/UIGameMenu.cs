using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Text;
using UnityEngine.SocialPlatforms.Impl;

namespace Starter
{
    /// <summary>
    /// Hiển thị menu trong trò chơi, xử lý việc kết nối/ngắt kết nối người chơi với trò chơi mạng và khóa con trỏ.
    /// </summary>
    public class UIGameMenu : MonoBehaviour
    {
        [Header("Cài đặt bắt đầu trò chơi")]
        [Tooltip("Xác định chế độ trò chơi mà người chơi sẽ tham gia - ví dụ: Platformer, ThirdPersonCharacter")]
        public string GameModeIdentifier;
        public NetworkRunner RunnerPrefab;
        private int[] MaxCountSelection = {2, 4, 6, 8};
        private int MaxPlayerCount = 8;

        [Header("Debug")]
        [Tooltip("Cho mục đích gỡ lỗi, có thể ép buộc trò chơi đơn (bắt đầu nhanh hơn)")]
        public bool ForceSinglePlayer;

        [Header("Cài đặt UI")]
        [SerializeField] private GameObject[] FuctionPanels;
        [SerializeField] private Image[] FuctionPanelChangeButtons;

        public CanvasGroup PanelGroup;

        public TMP_InputField RoomText;
        public TMP_InputField NicknameText;
        public TMP_InputField MaxScoreText;
        public TextMeshProUGUI StatusText;
        public TextMeshProUGUI MaxPlayerCountText;
        private bool _isPrivate;
        public GameObject[] StartGroup;
        public GameObject[] DisconnectGroup;

        [Header("Cài đặt")]
        [SerializeField] private Slider _volumeSoundSlider;
        [SerializeField] private Slider _volumeMusicSlider;

        private NetworkRunner _runnerInstance;
        private static string _shutdownStatus;
        [SerializeField] private GameObject _jetFake;

        private static readonly System.Random _random = new System.Random();
        private bool ScoreAvailable = true;


		//khởi tạo phòng
        public async void StartGame(string roomName)
        {
            await Disconnect();

            _runnerInstance = Instantiate(RunnerPrefab);

            // Thêm listener cho việc tắt máy để xử lý các trường hợp tắt máy không mong muốn
            var events = _runnerInstance.GetComponent<NetworkEvents>();
            events.OnShutdown.AddListener(OnShutdown);

            var sceneInfo = new NetworkSceneInfo();
            sceneInfo.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));

            var startArguments = new StartGameArgs()
            {
                GameMode = Application.isEditor && ForceSinglePlayer ? GameMode.Single : GameMode.Shared,
                SessionName = roomName,
                PlayerCount = MaxPlayerCount,
                // Cần xác định thuộc tính phiên cho việc ghép trận để quyết định nơi người chơi muốn tham gia.
                // Nếu không, người chơi từ cảnh Platformer có thể kết nối với trò chơi ThirdPersonCharacter, v.v.
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

		//nút tham gia phòng
        public void JoinGame()
        {
            StartGame(RoomText.text.ToUpper());
        }


		//nút tạo phòng
        public void CreateGame()
        {
            if (ScoreAvailable == false) return;
            StartGame(GenerateRoomName());
            _runnerInstance.SessionInfo.IsVisible = !_isPrivate;

            GameManager.ScoreToWin = int.Parse(MaxScoreText.text);
        }
		
		//cài đặt phòng riêng tư
        public void SetPrivate(bool isPrivate)
        {
            _isPrivate = isPrivate;
        }


		//tạo tên phòng ngẫu nhiên
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

		//cài đặt số lượng người chơi tối đa
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

		//kiểm tra nhập vào hợp lệlệ
        public void CheckMaxScore()
        {
            string score = MaxScoreText.text;
            if (string.IsNullOrEmpty(score))
            {
                StatusText.text = "Score cannot be empty!";
                ScoreAvailable = false;
                return;
            }

            if (!int.TryParse(score, out int a) || a <= 0)
            {
                StatusText.text = "Score must be greater than 0!";
                ScoreAvailable = false;
                return;
            }

            StatusText.text = "";
            ScoreAvailable = true;
        }

        //===========================================
        //======Chuyển đổi panel=====================
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
        //======Cài đặt âm thanh=====================
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
                return; // Panel không thể ẩn nếu trò chơi không chạy

            PanelGroup.gameObject.SetActive(!PanelGroup.gameObject.activeSelf);

            if (!PanelGroup.gameObject.activeSelf)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (GameManager.Instance == null) return;
            if (GameManager.Instance.State != GameState.Playing)
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
            MaxScoreText.text = GameManager.ScoreToWin.ToString();

            // Cố gắng tải trạng thái tắt máy trước đó
            StatusText.text = _shutdownStatus != null ? _shutdownStatus : string.Empty;
            _shutdownStatus = null;
        }

        private void Update()
        {
            // Phím Enter/Esc được sử dụng để khóa/mở khóa con trỏ trong chế độ xem trò chơi.
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePanelVisibility();
            }

            if (PanelGroup.gameObject.activeSelf)
            {
                foreach (var start in StartGroup)
                {
                    start.SetActive(_runnerInstance == null);
                }

                foreach (var disconnect in DisconnectGroup)
                {
                    disconnect.SetActive(_runnerInstance != null);
                }

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

            // Xóa listener tắt máy vì chúng ta đang ngắt kết nối có chủ đích
            var events = _runnerInstance.GetComponent<NetworkEvents>();
            events.OnShutdown.RemoveListener(OnShutdown);

            await _runnerInstance.Shutdown();
            _runnerInstance = null;
            _jetFake.SetActive(true);
            FuctionPanelChangeButtons[1].gameObject.SetActive(true);

            // Cần đặt lại các đối tượng mạng của cảnh, tải lại toàn bộ cảnh
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnShutdown(NetworkRunner runner, ShutdownReason reason)
        {
            // Đã xảy ra tắt máy không mong muốn (ví dụ: Máy chủ ngắt kết nối)

            // Lưu trạng thái vào biến tĩnh, nó sẽ được sử dụng trong OnEnable sau khi tải cảnh
            _shutdownStatus = $"Shutdown: {reason}";
            Debug.LogWarning(_shutdownStatus);

            // Cần đặt lại các đối tượng mạng của cảnh, tải lại toàn bộ cảnh
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}