
using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;
using Unity.VisualScripting;

[RequireComponent(typeof(NetworkRigidbody3D))]
public class Missile : NetworkBehaviour, IProjectile
	{
		// PRIVATE MEMBERS

		[SerializeField] private float _initialImpulse = 100f;
		[SerializeField] private int _damge = 5;
        [SerializeField] private float _aoeDamageRange = 5;
		[SerializeField] private float _lifeTime = 4f;
		[SerializeField] private GameObject _visualsRoot;
		[SerializeField] private NetworkObject _hitEffectPrefab;
		[SerializeField] private ParticleSystem _flyEffect;
		private NetworkObject _hitEffect;
		[SerializeField] private TrailRenderer _trailRenderer;
		[SerializeField] private float _lifeTimeAfterHit = 2f;
		[SerializeField] private Cooldown ChaseDelay;

		[Networked] private TickTimer _lifeCooldown { get; set; }
		[Networked] private NetworkBool _isDestroyed { get; set; }
		[Networked] private Player _firePlayer{ get; set; }

		private bool _isDestroyedRender;

		private NetworkRigidbody3D _rigidbody;
		private Collider _collider;
		private Vector3 target;
		private float rotationSpeed = 5f;
		public float scanRange = 10f;
    	public LayerMask playerLayer;
		public LayerMask CollisionLayer;
    	private Collider[] _playersInRange = new Collider[2];
		

		// PUBLIC METHODS

		public void Fire(Player player, Vector3 hit, Quaternion rotation)
		{
			_firePlayer = player;
			// TODO: Is teleport still necessary?
			_rigidbody.Teleport(transform.position, rotation);

			//reset đạn
			target = hit;
			ChaseDelay.StartCooldown();
			_trailRenderer.Clear();
			_visualsRoot.SetActive(true);
			_isDestroyedRender = false;
			_isDestroyed = false;
			_rigidbody.Rigidbody.isKinematic = false;
			_hitEffect = null;

			_flyEffect.Play();

			//bỏ qua va chạm với người bắn
			Physics.IgnoreCollision(_collider, player.GetComponent<Collider>(), true);

			// Set cooldown after which the projectile should be despawned
			if (_lifeTime > 0f)
			{
				_lifeCooldown = TickTimer.CreateFromSeconds(Runner, _lifeTime);
			}
		}

    // NetworkBehaviour INTERFACE
		public override void Spawned(){
			_flyEffect.Play();
		}

    	public override void FixedUpdateNetwork()
		{
			_collider.enabled = _isDestroyed == false;

			ScanForPlayers();

			if(target != null && !ChaseDelay.IsCoolingDown){
				Vector3 direction = target - transform.position;
				direction.Normalize();
				Quaternion lookRotation = Quaternion.LookRotation(direction);
				transform.rotation = Quaternion.Slerp(_rigidbody.Rigidbody.rotation, lookRotation, rotationSpeed * Runner.DeltaTime);
			}

			if(_isDestroyed == false)
				_rigidbody.Rigidbody.velocity = transform.forward * _initialImpulse;

			if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 0.5f, CollisionLayer)){
				if(hit.collider != _firePlayer.GetComponent<Collider>())
					ProcessHit();
			}

			if (_lifeCooldown.IsRunning == true && _lifeCooldown.Expired(Runner) == true)
			{
				ShowDestroyEffect();
				Runner.Despawn(Object);
				
			}
		}

		private void ScanForPlayers()
		{
			int numPlayers = Physics.OverlapSphereNonAlloc(transform.position, scanRange, _playersInRange, playerLayer);
			for (int i = 0; i < numPlayers; i++)
			{
				if (_playersInRange[i] != _firePlayer.GetComponent<Collider>())
				{
					target = _playersInRange[i].transform.position;
					break;
				}
			}
		}

		public override void Render()
		{
			if (_isDestroyed == true && _isDestroyedRender == false)
			{
				_isDestroyedRender = true;
				ShowDestroyEffect();
			}
		}

		// MONOBEHAVIOUR

		protected void Awake()
		{
			_rigidbody = GetComponent<NetworkRigidbody3D>();
			_collider = GetComponentInChildren<Collider>();

			_collider.enabled = false;
		}


		protected void OnCollisionEnter(Collision collision)
		{
			// if(collision.collider == _firePlayer.GetComponent<Collider>()) return;
			
			if(collision.gameObject.CompareTag("Player")){
				Player player =collision.gameObject.GetComponent<Player>();

				if(player.MyTeam != _firePlayer.MyTeam)
					player.RPC_TakeDamage(_damge);
			}
			
			ProcessHit();
		}

		// PRIVATE METHODS

		private void ProcessHit()
		{
			if(_isDestroyed == true) return;
			// Save destroyed flag so hit effects can be shown on other clients as well
			_isDestroyed = true;

			_lifeCooldown = TickTimer.CreateFromSeconds(Runner, _lifeTimeAfterHit);

			// Stop the movement
			Physics.IgnoreCollision(_collider, _firePlayer.GetComponent<Collider>(), false);
			_rigidbody.Rigidbody.isKinematic = true;
			_collider.enabled = false;
		}

		private void ShowDestroyEffect()
		{
			if(_hitEffect != null) return;
			
			if (_hitEffectPrefab != null)
			{
				_hitEffect = Runner.Spawn(_hitEffectPrefab, transform.position, transform.rotation, Object.InputAuthority);
				_hitEffect.GetComponent<ParticleSystem>().Play();
			}

			// Hide projectile visual
			if (_visualsRoot != null)
			{
				_visualsRoot.SetActive(false);
			}

            ApplyAoeDamage();
		}

        private void ApplyAoeDamage()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _aoeDamageRange, playerLayer);
            foreach (var collider in colliders)
            {
                if (collider != _firePlayer.GetComponent<Collider>())
                {
                    Player player = collider.GetComponent<Player>();
                    if (player.MyTeam != _firePlayer.MyTeam)
                    {
                        player.RPC_TakeDamage(_damge);
                    }
                }
            }
        }
	}


