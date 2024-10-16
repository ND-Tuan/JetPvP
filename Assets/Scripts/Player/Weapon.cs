using Fusion;
using FusionHelpers;
using UnityEngine;



public class Weapon : NetworkBehaviourWithState<Weapon.NetworkState>
{
	[Networked] public override ref NetworkState State => ref MakeRef<NetworkState>();
	public struct NetworkState : INetworkStruct
	{
		[Networked, Capacity(12)] 
		public NetworkArray<ShotState> bulletStates => default;
	}

	[SerializeField] private Transform[] _gunExits;
	[SerializeField] private Shot _bulletPrefab;
	[SerializeField]
	public SparseCollection<ShotState, Shot> bullets;
	private Collider[] _areaHits = new Collider[4];
	private Player _player;

	private void Awake()
	{
		_player = GetComponentInParent<Player>();
	}

	public override void Spawned()
	{
		bullets = new SparseCollection<ShotState, Shot>(State.bulletStates, _bulletPrefab);	
	}

	public override void FixedUpdateNetwork()
	{
		if(bullets==null) return;
		bullets.Process( this, (ref ShotState bullet, int tick) =>
		{

			if (!_bulletPrefab.IsHitScan && bullet.EndTick>Runner.Tick)
			{
				Vector3 dir = bullet.Direction.normalized;
				float length = Mathf.Max(_bulletPrefab.Radius, _bulletPrefab.Speed * Runner.DeltaTime);
				if(Physics.Raycast(bullet.Position - length * dir, dir, out var hitinfo, length, _bulletPrefab.HitMask.value, QueryTriggerInteraction.Ignore))
				//if (Runner.LagCompensation.Raycast(bullet.Position - length*dir, dir, length, Object.InputAuthority, out var hitinfo, _bulletPrefab.HitMask.value, HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX))
				{ 
					bullet.Position = hitinfo.point;
					bullet.EndTick = Runner.Tick;
					ApplyAreaDamage(hitinfo.point);
					return true;
				}
			}
			return false;
		});
	}

	public override void Render()
	{
		if (TryGetStateChanges(out var from, out var to))
			OnFireTickChanged();
		else
			TryGetStateSnapshots(out from, out _, out _, out _, out _);

		bullets.Render(this, from.bulletStates );
	}

	private void OnFireTickChanged()
	{
			// Recharge the laser sight if this weapon has it
			
	}


	public void Fire(NetworkRunner runner, PlayerRef owner, Vector3 ownerVelocity)
	{
		if (_gunExits.Length == 0) return;

		Transform exit = GetExitPoint(Runner.Tick);

		Debug.DrawLine(exit.position, exit.position+exit.forward, Color.blue, 1.0f);
		Debug.Log($"Bullet fired in tick {runner.Tick} from position {exit.position} weapon is at {transform.position}");
		SpawnNetworkShot(runner, owner, exit, ownerVelocity);
	}


	private void SpawnNetworkShot(NetworkRunner runner, PlayerRef owner, Transform exit, Vector3 ownerVelocity)
	{
		if (_bulletPrefab.IsHitScan)
		{
			bool impact;
			Vector3 hitPoint = exit.position + _bulletPrefab.Range * exit.forward;

			impact = runner.GetPhysicsScene().Raycast(exit.position, exit.forward,out var hitinfo, _bulletPrefab.Range, _bulletPrefab.HitMask.value);
			hitPoint = hitinfo.point;

			if (impact)
			{
				ApplyAreaDamage(hitPoint);
			}

			bullets.Add( runner, new ShotState(exit.position, hitPoint-exit.position), 0);

		} else
			bullets.Add(runner, new ShotState(exit.position, exit.forward), _bulletPrefab.TimeToLive);
	}

	private void ApplyAreaDamage(Vector3 hitPoint)
	{
		int cnt = Physics.OverlapSphereNonAlloc(hitPoint, _bulletPrefab.AreaRadius, _areaHits, _bulletPrefab.HitMask.value, QueryTriggerInteraction.Ignore);
		if (cnt > 0)
		{
			for (int i = 0; i < cnt; i++)
			{
				GameObject other = _areaHits[i].gameObject;
				if (other)
				{
					Player target = other.GetComponent<Player>();
					if (target != null && target!=_player )
						target.TakeDamage(_bulletPrefab.AreaDamage);
				}
			}
		}
	}		

	public Transform GetExitPoint(int tick)
	{
		Transform exit = _gunExits[tick% _gunExits.Length];
		return exit;
	}
}