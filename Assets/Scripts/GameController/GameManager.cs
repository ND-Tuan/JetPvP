using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Linq;

public enum GameState
{
    Waiting,
    Playing,
}

public sealed class GameManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
	{
		[Networked, OnChangedRender(nameof(GameStateChanged))] public GameState State { get; set; }	= GameState.Waiting;	
		public GameObject Map1;
		public Camera MainCamera;
		public NetworkObject PlayerPrefab;
		public NetworkObject HpBarPrefab;
		public NetworkObject LocalPlayer { get; private set; }
		private NetworkObject HpBar;
		private List<GameObject> PlayerList;
		public Player _player;

		public Transform BlueTeamSpawnPoint;
		public Transform RedTeamSpawnPoint;
		
		public float SpawnRadius = 3f;
		[Networked, Capacity(8)]public NetworkDictionary<PlayerRef, Player> Players => default;
		[Networked] private Player RoomOwner{get; set;}
		[Networked] private bool SetTeam{get; set;} = false;
		
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
					State = GameState.Playing;
				}
			}
		}

		private void OnAllReady(){
			//chuẩn bị map
			Map1.SetActive(true);
			BlueTeamSpawnPoint = GameObject.FindGameObjectWithTag("BlueFlag").transform;
			RedTeamSpawnPoint = GameObject.FindGameObjectWithTag("RedFlag").transform;
					
			//Set cam
			MainCamera.clearFlags = CameraClearFlags.Skybox;

			//Hiển thị chuyển trạng thái game
			PlayerHub.Instance.SetPlaying();
			

			//Chuẩn bị người chơi
			PreparePlayers();

			//Khóa phòng
			Runner.SessionInfo.IsOpen = false;
		}

    	private void PreparePlayers()
    	{
			foreach (KeyValuePair<PlayerRef, Player> player in Players)
			{   
				//Đưa người chơi về vị trí mỗi đội
				var SpawnPoint = _player.MyTeam == Team.Blue? BlueTeamSpawnPoint : RedTeamSpawnPoint;
				var randomPositionOffset = Random.insideUnitCircle * SpawnRadius;
				var spawnPosition = SpawnPoint.position + new Vector3(randomPositionOffset.x, transform.position.y, randomPositionOffset.y);

				//chuyển người chơi sang trạng thái chơi
				player.Value.RPC_StartGame(SetTeam? Team.Blue : Team.Red, spawnPosition, SpawnPoint.rotation);
				SetTeam = !SetTeam;
			}

			
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
        	if(State == GameState.Playing){
				OnAllReady();
			}
    	}

		

}
