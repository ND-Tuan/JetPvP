using UnityEngine;

namespace Multiplayer
{
	/// <summary>
	/// Structure holding player input.
	/// </summary>
	public struct GameplayInput
	{
		public Vector2 LookRotation;
		public Vector2 LookRotationDelta;
		public Vector2 MoveDirection;
		public float HightValue;
		public bool Jump;
		public bool Sprint;
		public bool SpeedUpEffect;
		public bool IsRotateX;
		public bool IsRotateY;
	}

	/// <summary>
	/// PlayerInput handles accumulating player input from Unity.
	/// </summary>
	public sealed class PlayerInput : MonoBehaviour
	{
		public GameplayInput CurrentInput => _input;
		private GameplayInput _input;

		public void ResetInput()
		{
			// Reset input after it was used to detect changes correctly again
			_input.MoveDirection = default;
			_input.Jump = false;
			_input.Sprint = false;
			_input.HightValue = 0;
		}

		private void Update()
		{
			// Accumulate input only if the cursor is locked.
			if (Cursor.lockState != CursorLockMode.Locked)
				return;

			// Accumulate input from Keyboard/Mouse. Input accumulation is mandatory (at least for look rotation here) as Update can be
			// called multiple times before next FixedUpdateNetwork is called - common if rendering speed is faster than Fusion simulation.

			_input.LookRotationDelta = new Vector2(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));
			_input.LookRotation = ClampLookRotation(_input.LookRotation + _input.LookRotationDelta);

			var moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), 1);
			_input.IsRotateX = moveDirection.x != 0;

			_input.MoveDirection = moveDirection.normalized;
			
			_input.HightValue = Input.GetAxisRaw("Vertical");
			_input.IsRotateY = _input.HightValue != 0;
			_input.Jump = _input.HightValue != 0;

			
			_input.Sprint = Input.GetMouseButton(1);

			_input.SpeedUpEffect = _input.Sprint;

		}

		private Vector2 ClampLookRotation(Vector2 lookRotation)
		{
			lookRotation.x = Mathf.Clamp(lookRotation.x, -30f, 70f);
			return lookRotation;
		}
	}
}
