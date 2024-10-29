using Fusion;
using UnityEngine;

namespace Projectiles
{
	// DummyFlyingProjectile can be used to show projectile flying through the air with hit
	// effect at the end.
	// This script is standard MonoBehaviour without any networking functionality.
	// Common scenario is to use it together with hitscan projectiles to add some sense
	// of projectile direction and travel time.
	public class DummyFlyingProjectile : NetworkBehaviour, IProjectile
	{
		// PRIVATE METHODS

		[SerializeField]
		private float _speed = 80f;
		[SerializeField]
		private float _maxDistance = 100f;
		[SerializeField] private NetworkObject _hitEffectPrefab;
		private NetworkObject _hitEffect;
		[SerializeField]
		private float _lifeTimeAfterHit = 2f;
		[SerializeField]
		private GameObject _visualRoot;

		private Vector3 _startPosition;
		private Vector3 _targetPosition;
		private bool _showHitEffect;

		private float _startTime;
		private float _duration;
		[Networked] private TickTimer _lifeCooldown { get; set; }
		[SerializeField] private TrailRenderer _trailRenderer;

		// PUBLIC METHODS

		public void Fire(Player player, Vector3 position, Quaternion rotation)
		{
			_targetPosition = position;
			_showHitEffect = position != Vector3.zero;
			_startPosition = transform.position;
			transform.rotation = rotation;

			_trailRenderer.Clear();
			_visualRoot.SetActive(true);

			if (_targetPosition == Vector3.zero)
			{
				_targetPosition = _startPosition + transform.forward * _maxDistance;
			}

			_duration = Vector3.Distance(_startPosition, _targetPosition) / _speed;
			_startTime = Time.timeSinceLevelLoad;

			if (_duration > 0f)
			{
				_lifeCooldown = TickTimer.CreateFromSeconds(Runner, _duration);
			}
		}

		// MONOBEHAVIOUR

		 public override void FixedUpdateNetwork()
		{
			float time = Time.timeSinceLevelLoad - _startTime;

			if (!_lifeCooldown.Expired(Runner))
			{
				transform.position = Vector3.Lerp(_startPosition, _targetPosition, time / _duration);
			}
			else
			{
				transform.position = _targetPosition;

				if (_showHitEffect == true && _hitEffectPrefab != null)
				{
					_lifeCooldown = TickTimer.CreateFromSeconds(Runner, _lifeTimeAfterHit);
					if (_visualRoot != null)
					{
						_visualRoot.SetActive(false);
					}

					_hitEffect = Runner.Spawn(_hitEffectPrefab, transform.position, transform.rotation, Object.InputAuthority);
					_hitEffect.GetComponent<ParticleSystem>().Play();

					_showHitEffect = false;
				}
			}

			if (_lifeCooldown.IsRunning == true && _lifeCooldown.Expired(Runner) == true)
			{
				Runner.Despawn(Object);
			}
		}
	}
}
