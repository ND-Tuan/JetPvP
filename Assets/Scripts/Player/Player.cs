using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using ObserverPattern;
using Fusion;
using Multiplayer;
using Fusion.Addons.SimpleKCC;
using UnityEngine.Rendering;



public sealed class Player : NetworkBehaviour
{
    
    

    [Header("Move")]
    public SimpleKCC KCC;
	public PlayerInput PlayerInput;
    [SerializeField] private float MoveSpeed = 2.0f;
    [SerializeField] private float SprintSpeed = 5.335f;
    [Range(0.0f, 1.2f)]
    [SerializeField] private float RotationSmoothTime = 0.12f;
	[SerializeField] private float RotationSpeed = 8f;

    private float currentZTiltAngle = 0f; 
    private float currentXTiltAngle = 0f; 
    private float _zRotationVelocity;
    private float _xRotationVelocity;

    [Header("Status")]
    [SerializeField] private int _MaxHealth = 100;
    private int _currentHealth;


    public bool LockCameraPosition = false;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private Animator _animator;
    [SerializeField]private Transform CameraPivot;
    [SerializeField]private Transform CameraHandle; 
    private GameObject _mainCamera;
    private Vector3 _moveVelocity;
    [SerializeField] private Transform _Model;
    [SerializeField] private LayerMask _MapLayer;
    private RaycastHit _hit;



    public override void Spawned()
    {
        _currentHealth = _MaxHealth;
        _animator = GetComponent<Animator>();
        _mainCamera = Camera.main.gameObject;
        if(!Object.HasInputAuthority){
            GetComponentInChildren<Camera>().enabled = false;
        }
        KCC.SetGravity(0);
    }

    public override void FixedUpdateNetwork()
		{
			ProcessInput(PlayerInput.CurrentInput);

			PlayerInput.ResetInput();
		}

		public override void Render(){
            
            if(!Object.HasInputAuthority){
                return;
            }

            RotationJetWithMovement(PlayerInput.CurrentInput);

			float smoothZTiltAngle = Mathf.SmoothDampAngle( _Model.localRotation.eulerAngles.z, -currentZTiltAngle, ref _zRotationVelocity, RotationSmoothTime);
            float smoothXTiltAngle = Mathf.SmoothDampAngle( _Model.localRotation.eulerAngles.x, currentXTiltAngle, ref _xRotationVelocity, RotationSmoothTime);

            _Model.localRotation = Quaternion.Euler(smoothXTiltAngle, 0, smoothZTiltAngle);
		}

		private void Awake()
		{
			AssignAnimationIDs();
		}

		private void LateUpdate()
		{
			// Only local player needs to update the camera
			// Note: In shared mode the local player has always state authority over player's objects.
			if (HasStateAuthority == false)
            return;

			// Update camera pivot and transfer properties from camera handle to Main Camera.
			CameraPivot.rotation = Quaternion.Euler(PlayerInput.CurrentInput.LookRotation);
			_mainCamera.transform.SetPositionAndRotation(CameraHandle.position, CameraHandle.rotation);
		}

		private void ProcessInput(GameplayInput input)
		{
			
			float speed = input.Sprint ? SprintSpeed : MoveSpeed;
            SpeepUpEffect(input.SpeedUpEffect);

            float jumpImpulse = input.HightValue* 0.4f;
            
			var lookRotation = Quaternion.Euler(0f, input.LookRotation.y, 0f);

            var currentRotation = KCC.TransformRotation;
            var targetRotation =lookRotation;
            float smoothYTiltAngle = Mathf.SmoothDampAngle(currentRotation.eulerAngles.y, targetRotation.eulerAngles.y, ref _rotationVelocity, RotationSmoothTime);
           
            var nextRotation = Quaternion.Euler(0, smoothYTiltAngle, 0);

            KCC.SetLookRotation(nextRotation.eulerAngles);

            var moveDirection = nextRotation * new Vector3(input.MoveDirection.x, input.HightValue* 0.4f, input.MoveDirection.y);

            if(CheckCollisonWithTerrian(moveDirection)){
                moveDirection = Vector3.zero;
            }

            var desiredMoveVelocity = moveDirection * speed;

			_moveVelocity = Vector3.Lerp(_moveVelocity, desiredMoveVelocity, 50 * Runner.DeltaTime);

			KCC.Move(desiredMoveVelocity);
		}

    private void RotationJetWithMovement(GameplayInput input){
        float targetZTiltAngle = 0f;
        float targetXTiltAngle = 0f;

        if(input.LookRotationDelta.x !=  0){
            currentZTiltAngle = _mainCamera.transform.eulerAngles.y - transform.eulerAngles.y;
            return;
        }

        // Kiểm tra đầu vào từ bàn phím
        if (input.IsRotateX){
            currentZTiltAngle = 45*Mathf.Sign(Input.GetAxisRaw("Horizontal"));
        }

        if (input.IsRotateY){
            targetXTiltAngle = -18*Mathf.Sign(Input.GetAxisRaw("Vertical"));
        }
        
        // Nghiêng máy bay dần dần về góc nghiêng mục tiêu
        currentZTiltAngle = Mathf.Lerp(currentZTiltAngle, targetZTiltAngle, Runner.DeltaTime *1);
        currentXTiltAngle = Mathf.Lerp(currentXTiltAngle, targetXTiltAngle, Runner.DeltaTime*1);
    }

        private void SpeepUpEffect(bool isSprint){
        if(isSprint){
            //Tạo hiệu ứng tăng tốc
            _mainCamera.GetComponent<RenderFeatureToggler>().ActivateRenderFeatures(0,true);
            _mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(_mainCamera.GetComponent<Camera>().fieldOfView, 105, Time.deltaTime * 10);
            GetComponentInChildren<Volume>().enabled = false;
            
            
            // Hạ thấp camera dần xuống 1,5 đơn vị
            Vector3 targetPosition = CameraHandle.transform.localPosition;
            targetPosition.y = Mathf.Lerp(CameraHandle.transform.localPosition.y, 2f, Time.deltaTime * 10);
            CameraHandle.transform.localPosition = targetPosition + GetCameraShakeOffset();
        } else {
            //bỏ hiệu ứng
            _mainCamera.GetComponent<RenderFeatureToggler>().ActivateRenderFeatures(0,false);
            _mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(_mainCamera.GetComponent<Camera>().fieldOfView, 75, Time.deltaTime * 10);
            GetComponentInChildren<Volume>().enabled = false;
            
            // Giữ nguyên vị trí camera, không tăng dần về 3 đơn vị
            Vector3 targetPosition = new(0,CameraHandle.transform.localPosition.y,-20);
            targetPosition.y = Mathf.Lerp(CameraHandle.transform.localPosition.y, 5, Time.deltaTime * 10);
            CameraHandle.transform.localPosition = targetPosition;
        }
    }

    private bool CheckCollisonWithTerrian(Vector3 diraction){
        if(Physics.Raycast(transform.position, diraction, out _hit, 1, _MapLayer)){
            return true;
        }
        return false;
    }

    private Vector3 GetCameraShakeOffset(){
        float shakeAmount = 0.05f; // Độ rung của camera
        return new Vector3(
            Random.Range(-shakeAmount, shakeAmount),
            Random.Range(-shakeAmount, shakeAmount),
            Random.Range(-shakeAmount, shakeAmount)
        );
    }

	private void AssignAnimationIDs(){


	}

		// Animation event
}
    


public static class SynchronizeWithRotation 
{
    public static Matrix4x4 Matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 Synchronize(this Vector3 input) => Matrix.MultiplyPoint3x4(input);
}
