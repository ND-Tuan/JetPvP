using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class JetController : MonoBehaviour
{

    [Header("Player")]
    public float MoveSpeed = 2.0f;
    public float SprintSpeed = 5.335f;
    [Range(0.0f, 1.2f)]
    public float RotationSmoothTime = 0.12f;
    public float SpeedChangeRate = 10.0f;
    public GameObject CinemachineCameraTarget;
    public GameObject BackCam;
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    public float CameraAngleOverride = 0.0f;
    public bool LockCameraPosition = false;
    private float angle = 0;


    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;


    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private Animator _animator;
    [SerializeField]private GameObject _mainCamera;
    private Rigidbody _rigibody;
    private StarterAssetsInputs _input;
    private float _speed;
    private const float _threshold = 0.1f;

    private float currentZTiltAngle = 0f; 
    private float currentXTiltAngle = 0f; 
    private float _zRotationVelocity;
    private float _xRotationVelocity;

    private PhotonView _photonView;

     void Start(){
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        _rigibody = GetComponent<Rigidbody>();

        _input = GetComponent<StarterAssetsInputs>();
        _photonView = GetComponentInParent<PhotonView>();
    }

    private void Awake(){
    }

    void Update(){
        if(_photonView.IsMine) Move();
    }


    private void Move() { 
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
        
        Vector3 moveInput =  new Vector3(Input.GetAxisRaw("Horizontal")*2,Input.GetAxisRaw("Vertical")*0.5f , 1);

        RotationJetWithMovement();

        _targetRotation =  _mainCamera.transform.eulerAngles.y;
        
        float smoothYTiltAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
        float smoothZTiltAngle = Mathf.SmoothDampAngle(transform.eulerAngles.z, -currentZTiltAngle, ref _zRotationVelocity, RotationSmoothTime);
        float smoothXTiltAngle = Mathf.SmoothDampAngle(transform.eulerAngles.x, currentXTiltAngle, ref _xRotationVelocity, RotationSmoothTime);

        transform.rotation = Quaternion.Euler(smoothXTiltAngle, smoothYTiltAngle, smoothZTiltAngle);
        SynchronizeWithRotation.Matrix = Matrix4x4.Rotate(Quaternion.Euler(0, smoothYTiltAngle, 0));
        

        _rigibody.velocity = moveInput.normalized.Synchronize() * targetSpeed;
        
        Debug.Log(_rigibody.velocity.x)  ;
    }

    private void RotationJetWithMovement(){
        float targetZTiltAngle = 0f;
        float targetXTiltAngle = 0f;

        if(_input.look.x != 0){
            currentZTiltAngle = _mainCamera.transform.eulerAngles.y - transform.eulerAngles.y;
            return;
        }

        // Kiểm tra đầu vào từ bàn phím
        if (_input.move.x != 0){
            targetZTiltAngle = -(45*Mathf.Sign(_input.move.x));
        }

        if (_input.move.y != 0){
            targetXTiltAngle = -30*Mathf.Sign(_input.move.y);
        }
        
        // Nghiêng máy bay dần dần về góc nghiêng mục tiêu
        currentZTiltAngle = Mathf.Lerp(currentZTiltAngle, targetZTiltAngle, Time.deltaTime *1000000000000);
        currentXTiltAngle = Mathf.Lerp(currentXTiltAngle, targetXTiltAngle, Time.deltaTime*1000);
    }


    private static float ClampAngle(float lfAngle, float lfMin, float lfMax){
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}

public static class SynchronizeWithRotation 
{
    public static Matrix4x4 Matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 Synchronize(this Vector3 input) => Matrix.MultiplyPoint3x4(input);
}
