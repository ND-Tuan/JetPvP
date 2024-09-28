using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using UnityEngine.Rendering.PostProcessing;

public class JetController : MonoBehaviour
{

    [Header("Move")]
    [SerializeField] private float MoveSpeed = 2.0f;
    [SerializeField] private float SprintSpeed = 5.335f;
    [Range(0.0f, 1.2f)]
    [SerializeField] private float RotationSmoothTime = 0.12f;
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
    [SerializeField]private GameObject _mainCamera;
    private Rigidbody _rigibody;
    private StarterAssetsInputs _input;


    public static PhotonView _photonView { get; private set; }

     void Start(){
        _rigibody = GetComponent<Rigidbody>();

        _input = GetComponent<StarterAssetsInputs>();
        _photonView = GetComponentInParent<PhotonView>();
        _animator = GetComponent<Animator>();

        _currentHealth = _MaxHealth;
    }

    void Update(){
        if(_photonView.IsMine) Move();
    }


    private void Move() { 
        float targetSpeed = _input.sprint? SprintSpeed : MoveSpeed;
        SpeepUpEffect(_input.sprint);
    
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal") * 2, Input.GetAxisRaw("Vertical") * 0.8f, 1);
    
        RotationJetWithMovement();
    
        _targetRotation = _mainCamera.transform.eulerAngles.y;
        
        // Xử lý nghiêng máy bay theo hướng di chuyển
        float smoothYTiltAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
        float smoothZTiltAngle = Mathf.SmoothDampAngle(transform.eulerAngles.z, -currentZTiltAngle, ref _zRotationVelocity, RotationSmoothTime);
        float smoothXTiltAngle = Mathf.SmoothDampAngle(transform.eulerAngles.x, currentXTiltAngle, ref _xRotationVelocity, RotationSmoothTime);

        // Gán góc nghiêng cho máy bay
        transform.rotation = Quaternion.Euler(smoothXTiltAngle, smoothYTiltAngle, smoothZTiltAngle);
        SynchronizeWithRotation.Matrix = Matrix4x4.Rotate(Quaternion.Euler(0, smoothYTiltAngle, 0));
        
        _rigibody.velocity = moveInput.normalized.Synchronize() * targetSpeed;
    }
    

    // Hàm xử lý nghiêng máy bay theo hướng di chuyển
    private void RotationJetWithMovement(){
        float targetZTiltAngle = 0f;
        float targetXTiltAngle = 0f;

        if(_input.look.x != 0){
            currentZTiltAngle = _mainCamera.transform.eulerAngles.y - transform.eulerAngles.y;
            return;
        }

        // Kiểm tra đầu vào từ bàn phím
        if (_input.move.x != 0){
            targetZTiltAngle = 45*Mathf.Sign(_input.move.x);
        }

        if (_input.move.y != 0){
            targetXTiltAngle = -18*Mathf.Sign(_input.move.y);
        }
        
        // Nghiêng máy bay dần dần về góc nghiêng mục tiêu
        currentZTiltAngle = Mathf.Lerp(currentZTiltAngle, targetZTiltAngle, Time.deltaTime *20);
        currentXTiltAngle = Mathf.Lerp(currentXTiltAngle, targetXTiltAngle, Time.deltaTime*20);
    }

    private void SpeepUpEffect(bool isSprint){
        if(isSprint){
            //Tạo hiệu ứng tăng tốc
            _mainCamera.GetComponent<CustomPostProcessing>().enabled = true;
            _mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(_mainCamera.GetComponent<Camera>().fieldOfView, 100, Time.deltaTime * 10);
            //_mainCamera.GetComponent<PostProcessLayer>().enabled = true;
            
            
            // Hạ thấp camera dần xuống 1,5 đơn vị
            Vector3 targetPosition = _mainCamera.transform.localPosition;
            targetPosition.y = Mathf.Lerp(_mainCamera.transform.localPosition.y, 2f, Time.deltaTime * 10);
            _mainCamera.transform.localPosition = targetPosition + GetCameraShakeOffset();
        } else {
            //bỏ hiệu ứng
            _mainCamera.GetComponent<CustomPostProcessing>().enabled = false;
            _mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(_mainCamera.GetComponent<Camera>().fieldOfView, 75, Time.deltaTime * 10);
            //_mainCamera.GetComponent<PostProcessLayer>().enabled = false;
            
            // Giữ nguyên vị trí camera, không tăng dần về 3 đơn vị
            Vector3 targetPosition = new(0,_mainCamera.transform.localPosition.y,-15);
            targetPosition.y = Mathf.Lerp(_mainCamera.transform.localPosition.y, 5, Time.deltaTime * 10);
            _mainCamera.transform.localPosition = targetPosition;
        }
    }

    private Vector3 GetCameraShakeOffset()
{
    float shakeAmount = 0.05f; // Độ rung của camera
    return new Vector3(
        Random.Range(-shakeAmount, shakeAmount),
        Random.Range(-shakeAmount, shakeAmount),
        Random.Range(-shakeAmount, shakeAmount)
    );
}

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax){
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }


    public void TakeDamage(int damage){
        _currentHealth -= damage;
        if(_currentHealth <= 0){
            _currentHealth = 0;
            Die();
        }
    }

    private void Die(){
        Debug.Log("Die");
    }


}

public static class SynchronizeWithRotation 
{
    public static Matrix4x4 Matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 Synchronize(this Vector3 input) => Matrix.MultiplyPoint3x4(input);
}
