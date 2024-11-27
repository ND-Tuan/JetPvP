using Fusion;
using UnityEngine;


	public class Weapon_NetworkObject : WeaponBase
	{
		// PRIVATE MEMBERS
		[SerializeField] private NetworkObject _projectilePrefab;
		[Networked] private int _fireCount { get; set; }
		[Networked] private TickTimer _cooldownTimer{ get; set; }
		private int _visibleFireCount;

		// WeaponBase INTERFACE

		public override void Fire()
		{
			// Spawn the projectile
			if(_cooldownTimer.ExpiredOrNotRunning(Runner)) {
			// Spawn can be called only on state authority
				if (HasStateAuthority == false) return;
				Vector3 hit = GetComponentInParent<DroneManager>().hitPoint;
				var projectile = Runner.Spawn(_projectilePrefab, FireTransform.position, FireTransform.rotation, Object.InputAuthority);
				projectile.GetComponent<IProjectile>().Fire(GameManager.Instance._player ,hit, FireTransform.rotation);
			
				_fireCount++;
				_cooldownTimer = TickTimer.CreateFromSeconds(Runner, Cooldown);
			}
		}

		public override void Spawned()
		{
			_visibleFireCount = _fireCount;
		}

		public override void Render()
		{
			if (_visibleFireCount < _fireCount)
			{
				PlayFireEffect();
			}

			_visibleFireCount = _fireCount;
		}
	}