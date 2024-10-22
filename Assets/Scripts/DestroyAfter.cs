using Fusion;
using UnityEngine;


	/// <summary>
	/// Simple component that destroys gameobject after specified time.
	/// </summary>
	public class DestroyAfter : NetworkBehaviour
	{
		public float DestroyTime = 0.5f;
		[Networked]
    	private TickTimer life { get; set; }


		public override void Spawned()
    	{
        	life = TickTimer.CreateFromSeconds(Runner, DestroyTime);
    	}

    	public override void FixedUpdateNetwork()
    	{
        	if (life.Expired(Runner))
        	{
            	Runner.Despawn(Object);
        	}
		}
	}
