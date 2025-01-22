using UnityEngine;
using Fusion;

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
			//chạy hiệu ứng bắn
			_particle.Play();
			
			// tìm nguồn phát nếu chưa có
			if (_fireSoundSources == null)
			{
				_fireSoundSources = _fireSoundSourcesRoot.GetComponentsInChildren<AudioSource>();
			}

			// Tìm nguồn trống và phát âm thanh bắn
			for (int i = 0; i < _fireSoundSources.Length; i++)
			{
				var source = _fireSoundSources[i];

				if (source.isPlaying == true)
					continue;

				source.clip = _fireClip;
				source.Play();
				return;
			}

		}

		public void SetRotation(Vector3 hitPoint){
			// Lưu hitpoint
			this._hitPoint = hitPoint;

			//xoay về hướng hitpoint
			Vector3 Diraction = hitPoint - transform.position;
			RPC_Rotation(Diraction);
    	}


		//rpc để đồng bộ hướng xoay trên tất cả client
		[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    	private void RPC_Rotation(Vector3 Diraction)
    	{
        	transform.forward = Diraction;
    	}
	}

