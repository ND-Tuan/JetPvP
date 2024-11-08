using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

public enum GameState
{
    Waiting,
	AllReady,
	Cooldown,
    Playing,
	Win,
}

public sealed class GameManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
	{
		[Networked, OnChangedRender(nameof(GameStateChanged))] public GameState State { get; set; }	= GameState.Waiting;	
		public GameObject Map1;
		public NetworkObject PlayerPrefab;
		public NetworkObject HpBarPrefab;
		public NetworkObject LocalPlayer { get; private set; }
		private NetworkObject HpBar;
		private List<GameObject> PlayerList;
		public Player _player;

		public Transform BlueTeamSpawnPoint;
		public Transform RedTeamSpawnPoint;

		[SerializeField] private GameObject _Hangar;
		
		public float SpawnRadius = 5f;
		[Networked, Capacity(8)]public NetworkDictionary<PlayerRef, Player> Players => default;
		[Networked] private Player RoomOwner{get; set;}
		[Networked] private bool SetTeam{get; set;} = false;
		[Networked] public Team Winner { get; set;}
		[Networked] private int BlueScore { get; set;} = 0;
		[Networked] private int RedScore { get; set;} = 0;


		private string _WinText;
		
		 //Singleton
    	public static GameManager Instance { get; private set; }
		

		public override void Spawned()
		{
			 //triển khai Singleton
        	if (Instance == null){
            	Instance = this;
            	DontDestroyOnLoad(gameObject);

        	} else if (Instance != this){
            	Destroy(gameObject);
        	}


			//trạng thái bắt đầu mặc định
			State = GameState.Waiting;
			_WinText = "";


			//Khởi tạo người chơi
			LocalPlayer = Runner.Spawn(PlayerPrefab, transform.position, Quaternion.identity, Runner.LocalPlayer);
			HpBar = Runner.Spawn(HpBarPrefab, transform.position, Quaternion.identity, Runner.LocalPlayer);

			_player = LocalPlayer.GetComponent<Player>();

			_player._HpDisplay = HpBar.GetComponent<HpBarDisplay>();
			_player.RPC_SetReady(false);
			

			Players.Add(LocalPlayer.StateAuthority, _player);
			PlayerHub.Instance.gameObject.SetActive(true);
			
			if(Players.Count() == 1) RoomOwner = _player;

			//PlayerHub.Instance.OnDiplayHp(Players);
		}

		public override void FixedUpdateNetwork()
    	{
			if (Players.Count < 1)
				return;
			
			//Kiểm tra trạng thái sẵn sàng
			if (State == GameState.Waiting && _player == RoomOwner)
			{
				bool areAllReady = true;
				foreach (KeyValuePair<PlayerRef, Player> player in Players)
				{
					if (!player.Value.IsReady)
					{
						areAllReady = false;
						break;
					}
				}

				if (areAllReady)
				{
					State = GameState.AllReady;
				}
			}
		}

		private async void OnAllReady(){
			
			PlayerHub.Instance.SetReadyText("Take off!!", Color.green);

			_Hangar.GetComponent<Animator>().Play("TakeOff");
			await Task.Delay(800);

			PlayerHub.Instance.SetFlash(true);
			await Task.Delay(1000);

			//chuẩn bị map
			Map1.SetActive(true);
			BlueTeamSpawnPoint = GameObject.FindGameObjectWithTag("BlueFlag").transform;
			RedTeamSpawnPoint = GameObject.FindGameObjectWithTag("RedFlag").transform;

			//Chuẩn bị người chơi
			PreparePlayers();

			//Khóa phòng
			Runner.SessionInfo.IsOpen = false;

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			State = GameState.Cooldown;
		}

    	private void PreparePlayers()
    	{
			foreach (KeyValuePair<PlayerRef, Player> player in Players)
			{   
				//chuyển người chơi sang trạng thái chơi
				player.Value.RPC_StartGame(SetTeam? Team.Blue : Team.Red);
				SetTeam = !SetTeam;

				//Đưa người chơi về vị trí mỗi đội
        		TelePlayer(player.Value);
			}
    	}

		private void TelePlayer(Player player){
        	var SpawnPoint = player.MyTeam == Team.Blue?   BlueTeamSpawnPoint : RedTeamSpawnPoint;
                                          
			var randomPositionOffset = Random.insideUnitCircle * SpawnRadius;
			var spawnPosition = SpawnPoint.position + new Vector3(randomPositionOffset.x, transform.position.y, randomPositionOffset.y);

			player.Teleport(spawnPosition, SpawnPoint.rotation);
		}

    	public void PlayerJoined(PlayerRef player)
    	{
        	Invoke(nameof(UpdatePlayerList), 1f);
    	}

		public void PlayerLeft(PlayerRef player)
		{
			Players.Remove(player);

			//đổi chủ phòng
			foreach (KeyValuePair<PlayerRef, Player> playerScript in Players)
			{   
				RoomOwner = playerScript.Value;
				break;
			}
		}

		private void  UpdatePlayerList(){

			PlayerList = GameObject.FindGameObjectsWithTag("Player").ToList();

			foreach(GameObject player in PlayerList){
				Player playerScript = player.GetComponent<Player>();
				if(!Players.ContainsValue(playerScript)){
					PlayerRef playerRef = player.GetComponent<NetworkObject>().StateAuthority;

					if (player != null)
						Players.Add(playerRef, playerScript);
				}
			}

		}

		private void GameStateChanged()
    	{
			switch(State){
				case GameState.Waiting:
					_Hangar.SetActive(true);
					break;

				case GameState.AllReady:
					OnAllReady();
					break;

				case GameState.Cooldown:
					_Hangar.SetActive(false);
					TelePlayer(_player);
					StartCoroutine(StartCooldown());
					break;

				case GameState.Playing:
					_player.State = Player.PlayerState.Active;
					StopAllCoroutines();
					break;
				
				case GameState.Win:
					_player.State = Player.PlayerState.Rest;
					
					if(Winner == Team.Blue){
						BlueScore++;
						PlayerHub.Instance.SetScore(Team.Blue, BlueScore);
						_WinText = "Blue Team Win! <br>";

					} else {
						RedScore++;
						PlayerHub.Instance.SetScore(Team.Red, RedScore);
						_WinText = "Red Team Win! <br>";

					}

					State = GameState.Cooldown;
					break;
			}
    	}

		private IEnumerator StartCooldown()
		{	
			int time = 3;
			while(time > 0){
				PlayerHub.Instance.SetReadyText(_WinText + "Game will start in " + time + "s", Color.white);
				yield return new WaitForSeconds(1);
				time--;
			}
			PlayerHub.Instance.SetFlash(false);
			//Hiển thị chuyển trạng thái game
			PlayerHub.Instance.SetPlaying();

			State = GameState.Playing; // Example state change after cooldown
		}
			
		

}
