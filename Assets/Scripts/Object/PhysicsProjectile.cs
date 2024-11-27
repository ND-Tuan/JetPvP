﻿using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;
using Unity.VisualScripting;


[RequireComponent(typeof(NetworkRigidbody3D))]
	public class PhysicsProjectile : NetworkBehaviour, IProjectile
	{
		// PRIVATE MEMBERS

		[SerializeField] private float _initialImpulse = 100f;
		[SerializeField] private int _damge = 5;
		[SerializeField] private float _lifeTime = 4f;
		[SerializeField] private GameObject _visualsRoot;
		[SerializeField] private NetworkObject _hitEffectPrefab;
		private NetworkObject _hitEffect;
		[SerializeField] private TrailRenderer _trailRenderer;
		[SerializeField] private float _lifeTimeAfterHit = 2f;

		[Networked] private TickTimer _lifeCooldown { get; set; }
		[Networked] private NetworkBool _isDestroyed { get; set; }
		[Networked] private Player _firePlayer{ get; set; }

		private bool _isDestroyedRender;

		private NetworkRigidbody3D _rigidbody;
		private Collider _collider;

		// PUBLIC METHODS

		public void Fire(Player player, Vector3 position, Quaternion rotation)
		{
			_firePlayer = player;
			// TODO: Is teleport still necessary?
			_rigidbody.Teleport(position, rotation);

			//reset đạn
			_trailRenderer.Clear();
			_visualsRoot.SetActive(true);
			_isDestroyedRender = false;
			_isDestroyed = false;
			_rigidbody.Rigidbody.isKinematic = false;

			//bỏ qua va chạm với người bắn
			Physics.IgnoreCollision(_collider, player.GetComponent<Collider>(), true);

			//thêm lực
			_rigidbody.Rigidbody.AddForce(transform.forward * _initialImpulse, ForceMode.Impulse);

			// Set cooldown after which the projectile should be despawned
			if (_lifeTime > 0f)
			{
				_lifeCooldown = TickTimer.CreateFromSeconds(Runner, _lifeTime);
			}
		}

    // NetworkBehaviour INTERFACE

    	public override void FixedUpdateNetwork()
		{
			_collider.enabled = _isDestroyed == false;

			if (_lifeCooldown.IsRunning == true && _lifeCooldown.Expired(Runner) == true)
			{
				Runner.Despawn(Object);
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
		}
	}