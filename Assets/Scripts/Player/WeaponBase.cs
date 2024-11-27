using UnityEngine;
using Fusion;


	// Common weapon base class for all basic examples
	public abstract class WeaponBase : NetworkBehaviour
	{
		// PUBLIC MEMBERS

		protected Transform FireTransform => _fireTransform;
		protected float Cooldown => _cooldown;
		protected Vector3 HitPoint => _hitPoint;

		// PRIVATE MEMBERS
		[SerializeField] private float _cooldown;
    	[SerializeField] private ParticleSystem _particle;

		[SerializeField]
		private Transform _fireTransform;
		[SerializeField]
		private AudioClip _fireClip;
		[SerializeField]
		private Transform _fireSoundSourcesRoot;

		private AudioSource[] _fireSoundSources;
		private Vector3 _hitPoint;
		

		// PUBLIC METHODS

		public abstract void Fire();

		// PROTECTED METHODS

		protected void PlayFireEffect()
		{
			_particle.Play();
			// In multipeer mode fire sounds are played only for visible runner
			if (Runner.GetVisible() == false)
				return;

			if (_fireSoundSources == null)
			{
				_fireSoundSources = _fireSoundSourcesRoot.GetComponentsInChildren<AudioSource>();
			}

			// Find free audio source and play fire sound
			for (int i = 0; i < _fireSoundSources.Length; i++)
			{
				var source = _fireSoundSources[i];

				if (source.isPlaying == true)
					continue;

				source.clip = _fireClip;
				source.Play();
				return;
			}

			Debug.LogWarning("No free fire sound source", gameObject);
		}

		public void SetRotation(Vector3 hitPoint){
			this._hitPoint = hitPoint;
			Vector3 Diraction = hitPoint - transform.position;
			RPC_Rotation(Diraction);
    	}

		[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    	private void RPC_Rotation(Vector3 Diraction)
    	{
        	transform.forward = Diraction;
    	}
	}

