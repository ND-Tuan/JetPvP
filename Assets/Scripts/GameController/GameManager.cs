using UnityEngine;
using Fusion;
using System.Collections.Generic;


	/// <summary>
	/// Handles player connections (spawning of Player instances).
	/// </summary>
	public sealed class GameManager : NetworkBehaviour
	{
		public Player PlayerPrefab;
		public float SpawnRadius = 3f;
		public List<Player> Players;
		 //Singleton
    	public static GameManager Instance { get; private set; }
		public Player LocalPlayer { get; private set; }


		public override void Spawned()
		{
			 //triển khai Singleton
        	if (Instance == null){
            	Instance = this;
            	DontDestroyOnLoad(gameObject);

        	} else if (Instance != this){
            	Destroy(gameObject);
        	}


			var randomPositionOffset = Random.insideUnitCircle * SpawnRadius;
			var spawnPosition = transform.position + new Vector3(randomPositionOffset.x, transform.position.y, randomPositionOffset.y);

			LocalPlayer = Runner.Spawn(PlayerPrefab, spawnPosition, Quaternion.identity, Runner.LocalPlayer);
			Players.Add(LocalPlayer);
		}  


		private void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireSphere(transform.position, SpawnRadius);
		}
	}
