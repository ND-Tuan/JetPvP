using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using Fusion.Async;

public enum GameState
{
    Waiting,
	AllReady,
	Cooldown,
    Playing,
	Win
}

public sealed class GameManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
	{
		[Networked, OnChangedRender(nameof(GameStateChanged))] public GameState State { get; set; }	= GameState.Waiting;
		public int ScoreToWin = 3;	
		public GameObject Map1;
		public NetworkObject PlayerPrefab;
		public NetworkObject HpBarPrefab;
		public NetworkObject LocalPlayer { get; private set; }
		private NetworkObject HpBar;
		private List<GameObject> PlayerList;
		public Player _player;

		[Networked] public Vector3 BlueTeamSpawnPoint{get; set;}
		[Networked] public Vector3 RedTeamSpawnPoint{get; set;}

		[SerializeField] private GameObject _Hangar;
		
		public float SpawnRadius = 5f;
		[Networked, Capacity(8)]public NetworkDictionary<PlayerRef, Player> Players => default;
		[Networked] private bool SetTeam{get; set;} = false;
		[Networked] public Team Winner { get; set;}
		[Networked, OnChangedRender(nameof(OnScoreChange))] public int BlueScore { get; private set;} = 0;
		[Networked, OnChangedRender(nameof(OnScoreChange))] public int RedScore { get; private set;} = 0;
		[Networked] public TickTimer _cooldown { get; set; }
		[Networked]	public TickTimer GameOverTimer { get; set; }
		
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
			Runner.SetPlayerObject(Runner.LocalPlayer, _player.Object);

			_player._HpDisplay = HpBar.GetComponent<HpBarDisplay>();
			_player.RPC_SetReady(false);
			

			Players.Add(LocalPlayer.StateAuthority, _player);
			PlayerHub.Instance.gameObject.SetActive(true);
			

			//PlayerHub.Instance.OnDiplayHp(Players);
		}

		public override void FixedUpdateNetwork()
    	{
			if (Players.Count < 1)
				return;
			
			//Kiểm tra trạng thái sẵn sàng
			if (State == GameState.Waiting && Runner.IsSharedModeMasterClient)
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
					_cooldown = TickTimer.CreateFromSeconds(Runner, 1.8f);
				}
			}

			if (GameOverTimer.Expired(Runner))
			{
			
				if(Runner.IsSharedModeMasterClient){
					if (Winner == Team.Blue ) BlueScore++;
					if (Winner == Team.Red ) RedScore++;
				}		

				// Restart the game
				Winner = default;
				GameOverTimer = default;

				if (BlueScore >= ScoreToWin || RedScore >= ScoreToWin){
					return;
				}

				// Prepare players for next round
				RPC_RespawnPlayer();
				

				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;

				PlayerHub.Instance.SetReadyText("", Color.green, false);
			}
		}

		private async void OnAllReady(){
			
			PlayerHub.Instance.SetReadyText("Take off!!", Color.green, false);

			_Hangar.GetComponent<Animator>().Play("TakeOff");
			await Task.Delay(800);

			PlayerHub.Instance.SetFlash(true);
			await Task.Delay(1000);

			//chuẩn bị map
			Map1.SetActive(true);
			BlueTeamSpawnPoint = GameObject.FindGameObjectWithTag("BlueFlag").transform.position;
			RedTeamSpawnPoint = GameObject.FindGameObjectWithTag("RedFlag").transform.position;

			//Khóa phòng
			Runner.SessionInfo.IsOpen = false;

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			if(Runner.IsSharedModeMasterClient){
				State = GameState.Cooldown;
				//Chuẩn bị người chơi
				PreparePlayers();
			}
				
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
				
				Quaternion spawnRotation = Quaternion.LookRotation(SpawnPoint - transform.position);
				var randomPositionOffset = Random.insideUnitCircle * SpawnRadius;
				var spawnPosition = SpawnPoint + new Vector3(randomPositionOffset.x, transform.position.y, randomPositionOffset.y);

				player.Teleport(spawnPosition, spawnRotation);
			
		}

    	public void PlayerJoined(PlayerRef player)
    	{
        	Invoke(nameof(UpdatePlayerList), 1f);
    	}

		public void PlayerLeft(PlayerRef player)
		{
			Players.Remove(player);
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

		private void ChangePlayersState(Player.PlayerState state)
		{
			foreach (KeyValuePair<PlayerRef, Player> player in Players)
			{
				player.Value.State = state;
			}
		}

		private void OnScoreChange(){
			PlayerHub.Instance.SetScore(Team.Blue, BlueScore);
			PlayerHub.Instance.SetScore(Team.Red, RedScore);

			if ((BlueScore >= ScoreToWin || RedScore >= ScoreToWin) && Runner.IsSharedModeMasterClient){
				State = GameState.Win;
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
					ChangePlayersState(Player.PlayerState.Active);
					StopAllCoroutines();
					break;
				case GameState.Win:
					float ratio = (float)(BlueScore - RedScore) / ScoreToWin;
					ratio = (float)System.Math.Round(ratio, 2);
					PlayerHub.Instance.DisplayFinalWin(ratio);
					break;
			}
    	}

		private IEnumerator StartCooldown()
		{	
			int time = 3;
			while(time > 0){
				PlayerHub.Instance.SetReadyText("Game will start in " + time + "s", Color.white, false);

				if(Runner.IsSharedModeMasterClient)
					_cooldown = TickTimer.CreateFromSeconds(Runner, 1);
					
				yield return new WaitUntil(() => _cooldown.Expired(Runner));
				time--;
			}
			PlayerHub.Instance.SetFlash(false);
			//Hiển thị chuyển trạng thái game
			PlayerHub.Instance.SetPlaying();

			if(Runner.IsSharedModeMasterClient){
				State = GameState.Playing;
			}
		}
		
		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		private void RPC_RespawnPlayer()
		{
			_player.Respawn();
		}

}
