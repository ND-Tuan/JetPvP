using Fusion;
using UnityEngine;


	public class Weapon_Hitscan : WeaponBase
	{
	
		[SerializeField]private NetworkObject _dummyProjectilePrefab;
		[Networked] private int _fireCount { get; set; }
		[Networked] private TickTimer _cooldownTimer{ get; set; }
		[Networked, Capacity(100)] private Vector3 _hitPosition { get; set; }
		[SerializeField] private int _damage;
		[SerializeField] LayerMask _layer;
		[SerializeField] private Collider[] _hitColliders = new Collider[1];
		private int _visibleFireCount;
		


		public override void Fire()
		{
			//kiểm tra cooldown
			if(_cooldownTimer.ExpiredOrNotRunning(Runner)) {

				_hitPosition = HitPoint;
				_fireCount++;

				//set lại cooldown
				_cooldownTimer = TickTimer.CreateFromSeconds(Runner, Cooldown);

				//kiểm tra va chạm
				int _numColliders = Physics.OverlapSphereNonAlloc(HitPoint, 1f, _hitColliders, _layer);
				if(_numColliders<=0) return;

				Player target = _hitColliders[0].gameObject.GetComponent<Player>();
				if (target == null) return;

				//ko gây sát thương cho đồng đội
				if(target.MyTeam == GameManager.Instance._player.MyTeam) return;
				
				target.RPC_TakeDamage(_damage);
			}
		}

		public override void Spawned()
		{
			// Khởi tạo giá trị ban đầu cho biến đếm số lần bắn hiển thị
			_visibleFireCount = _fireCount;
		}

		public override void Render()
		{
			// Kiểm tra số lần bắn thực tế (tránh mất đồng bộ giữa các client)
			if (_visibleFireCount < _fireCount)
			{
				 // Chạy hiệu ứng bắn
				PlayFireEffect();

				// tạo và bắn đạn giả để làm hiệu ứng hiển thị
				if (_dummyProjectilePrefab != null)
				{
					var projectile = Runner.Spawn(_dummyProjectilePrefab, FireTransform.position, FireTransform.rotation, Object.InputAuthority);
					projectile.GetComponent<IProjectile>().Fire(GameManager.Instance._player ,HitPoint, FireTransform.rotation);
				}
			}

			_visibleFireCount = _fireCount;
		}
	}

