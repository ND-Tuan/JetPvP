using Fusion;
using UnityEngine;

	// Using projectile data buffer is the most versatile solution that can scale very well with the project.
	// In this example we use hitscan projectiles and the added complexity over Example 03 is minimal.
	// Hitscan projectiles are very easy to implement and are the most efficient. You can trick the player
	// that the projectile is flying through the air by using dummy flying projectile.
	// However if kinematic projectiles are needed, the solution needs to be more complex, proceed to Example 05.
	public class Weapon_Hitscan : WeaponBase
	{
		// PRIVATE MEMBERS
		[SerializeField]private NetworkObject _dummyProjectilePrefab;
		[Networked] private int _fireCount { get; set; }
		[Networked] private TickTimer _cooldownTimer{ get; set; }
		[Networked, Capacity(100)] private Vector3 _hitPosition { get; set; }
		[SerializeField] private int _damage;
		[SerializeField] LayerMask _layer;
		[SerializeField] private Collider[] _hitColliders = new Collider[1];
		private int _visibleFireCount;
		

		// WeaponBase INTERFACE

		public override void Fire()
		{
			if(_cooldownTimer.ExpiredOrNotRunning(Runner)) {

				_hitPosition = HitPoint;
				_fireCount++;
				_cooldownTimer = TickTimer.CreateFromSeconds(Runner, Cooldown);

				int _numColliders = Physics.OverlapSphereNonAlloc(HitPoint, 1f, _hitColliders, _layer);
				if(_numColliders<=0) return;

				Player target = _hitColliders[0].gameObject.GetComponent<Player>();
				if (target == null) return;
				
				target.RPC_TakeDamage(_damage);
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

				// Try to spawn dummy flying projectile.
				// Even though projectile hit was immediately processed in FUN we can spawn
				// dummy projectile that still travels through air with some speed until the hit position is reached.
				// That way the immediate hitscan effect is covered by the flying visuals.
				if (_dummyProjectilePrefab != null)
				{
					var projectile = Runner.Spawn(_dummyProjectilePrefab, FireTransform.position, FireTransform.rotation, Object.InputAuthority);
					projectile.GetComponent<IProjectile>().Fire(GameManager.Instance._player ,HitPoint, FireTransform.rotation);
				}
			}

			_visibleFireCount = _fireCount;
		}
	}

