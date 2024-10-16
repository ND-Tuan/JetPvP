using System.Collections;
using UnityEngine;
using Fusion;
using Multiplayer;
using Fusion.Addons.SimpleKCC;
using UnityEngine.Rendering;
using FusionHelpers;

public sealed class Player : NetworkBehaviour
{
    
    [Header("Move")]
    public SimpleKCC KCC;
	public PlayerInput PlayerInput;
    [SerializeField] private float MoveSpeed = 2.0f;
    [SerializeField] private float SprintSpeed = 5.335f;
    [SerializeField] private float _MaxEnergy = 100f;
    [SerializeField] private float _currentEnergy;
    [SerializeField] private float _energyRegenRate = 20f;
    private bool isRegenerating = false; 
    private Vector3 _moveVelocity;
    [SerializeField] private LayerMask _MapLayer;
    [SerializeField] LayerMask _HitMask;
    private RaycastHit _hit;

    //Rotate
    [Range(0.0f, 1.2f)]
    [SerializeField] private float RotationSmoothTime = 0.12f;
	[SerializeField] private float RotationSpeed = 8f;
    private float currentZTiltAngle = 0f; 
    private float currentXTiltAngle = 0f; 
    private float _zRotationVelocity;
    private float _xRotationVelocity;
    private float _rotationVelocity;

    [Header("Status")]
    
    [SerializeField] private int _MaxHealth = 100;
    [Networked] public Stage stage { get; set; }
	private int _currentHealth;
    public Team MyTeam;
    public enum Stage{
		New,
		TeleportOut,
		TeleportIn,
		Active,
		Dead
	}
    public struct DamageEvent : INetworkEvent{
		public int damage;
	}

    [Header("Camera")]
    public bool LockCameraPosition = false;
    [SerializeField]private Transform CameraPivot;
    [SerializeField]private Transform CameraHandle; 
    private GameObject _mainCamera;

    [Header("Attack")]
    [SerializeField] private Transform _RaycastPoint;
    [SerializeField] private Cooldown _FireRate;
    private IAttack[] _attackers;
    private Vector3 _currentDirection;
    
    [Header("Visual")]
    private Animator _animator;
    private PlayerHub _playerHub;
    public HpBarDisplay _HpDisplay {private get; set;}
    [SerializeField] private UIInfoplate infoplate;
    [SerializeField] private Transform _Model;
    private Color Red = new(255,42,0);
	private	Color Blue = new(0,180,255);
    
    private Vector3 _hitPosition;
    private Vector3 _hitNormal;

    [Networked, HideInInspector, Capacity(24), OnChangedRender(nameof(OnNicknameChanged))]
	public string Nickname { get; set; }
    

    public override void Spawned()
    {
        _currentHealth = _MaxHealth;
        _currentEnergy = _MaxEnergy;

        _animator = GetComponent<Animator>();
        _playerHub = GetComponentInChildren<PlayerHub>();
        _attackers = GetComponentsInChildren<IAttack>();
        _mainCamera = Camera.main.gameObject;
        
        Nickname = PlayerPrefs.GetString("PlayerName");
        OnNicknameChanged();

        if(!Object.HasInputAuthority){
            GetComponentInChildren<Camera>().enabled = false;
        }
        KCC.SetGravity(0);
    }

    public override void FixedUpdateNetwork()
	{
		ProcessInput(PlayerInput.CurrentInput);
                
        if(PlayerInput.CurrentInput.Fire && !_FireRate.IsCoolingDown){
            Shoot();
        }
            
		PlayerInput.ResetInput();
	}

	public override void Render(){
         if(!Object.HasInputAuthority){
            infoplate.UpdateHP(_currentHealth, _MaxHealth);
            return;
        }
            
        if(PlayerInput.CurrentInput.SpeedUpEffect && _currentEnergy > 0){
            Vector3 targetPosition = CameraHandle.transform.localPosition;
            targetPosition.y = Mathf.Lerp(CameraHandle.transform.localPosition.y, 0.5f, Runner.DeltaTime * 10);
            CameraHandle.transform.localPosition = targetPosition + GetCameraShakeOffset();
        } else {
            // Giữ nguyên vị trí camera, đặt về vị trí mặc định
            Vector3 targetPosition = new(0,CameraHandle.transform.localPosition.y,-23);
            targetPosition.y = Mathf.Lerp(CameraHandle.transform.localPosition.y, 3, Runner.DeltaTime * 10);
            CameraHandle.transform.localPosition = targetPosition;
        }

        if(Physics.Raycast(_RaycastPoint.position, CameraHandle.forward, out _hit, 200, _HitMask)){
            _currentDirection = _hit.point - CameraHandle.position;
        } else {
            _currentDirection = CameraHandle.forward;
        }

        foreach (var attacker in _attackers)
        {
            attacker.SetRotation(_currentDirection);
        }
            
        RotationJetWithMovement(PlayerInput.CurrentInput);

		float smoothZTiltAngle = Mathf.SmoothDampAngle( _Model.localRotation.eulerAngles.z, -currentZTiltAngle, ref _zRotationVelocity, RotationSmoothTime);
        float smoothXTiltAngle = Mathf.SmoothDampAngle( _Model.localRotation.eulerAngles.x, currentXTiltAngle, ref _xRotationVelocity, RotationSmoothTime);

        _Model.localRotation = Quaternion.Euler(smoothXTiltAngle, 0, smoothZTiltAngle); 
	}

	private void LateUpdate()
	{
        CameraPivot.rotation = Quaternion.Euler(PlayerInput.CurrentInput.LookRotation);
		if (HasStateAuthority == false){
            infoplate.UpdateHP(_currentHealth, _MaxHealth);
            return;
        } 

        
        if (!isRegenerating && _currentEnergy < _MaxEnergy && !PlayerInput.CurrentInput.SpeedUpEffect)
        {
            StartCoroutine(RegenerateEnergy());  // Bắt đầu coroutine để hồi thể lực
        }
        _playerHub.OnUpdateEnergyBar(_currentEnergy, _MaxEnergy, isRegenerating);
		// Update camera pivot and transfer properties from camera handle to Main Camera.
		_mainCamera.transform.SetPositionAndRotation(CameraHandle.position, CameraHandle.rotation);
	}

    //=============Xử lý di chuyển=======================================

	private void ProcessInput(GameplayInput input){
		
        //Set tốc độ di chuyển
		float speed = (input.Sprint && _currentEnergy > 1)? SprintSpeed : MoveSpeed;

        if(input.SpeedUpEffect && _currentEnergy > 0){
           _currentEnergy -= 8 * Time.deltaTime;
           if(_currentEnergy < 0) _currentEnergy = 0;
           isRegenerating = true;

        } else {
           input.SpeedUpEffect = false;
           isRegenerating = false;
        } 

        //Hiệu ứng
        SpeepUpEffect(input.SpeedUpEffect && _currentEnergy > 0);
        
        //xoay máy bay theo hướng di chuyển
		var lookRotation = Quaternion.Euler(0f, input.LookRotation.y, 0f);

        var currentRotation = KCC.TransformRotation;
        var targetRotation =lookRotation;

        //làm mượt góc quay
        float smoothYTiltAngle = Mathf.SmoothDampAngle(currentRotation.eulerAngles.y, targetRotation.eulerAngles.y, ref _rotationVelocity, RotationSmoothTime);
        var nextRotation = Quaternion.Euler(0, smoothYTiltAngle, 0);

        KCC.SetLookRotation(nextRotation.eulerAngles);

        //Tính hướng di chuyển
        var moveDirection = nextRotation * new Vector3(input.MoveDirection.x, input.HightValue* 0.4f, input.MoveDirection.y);

        //Kiểm tra va chạm
        if(CheckCollisonWithTerrian(moveDirection)){
            moveDirection = Vector3.zero;
        }

        var desiredMoveVelocity = moveDirection * speed;
		_moveVelocity = Vector3.Lerp(_moveVelocity, desiredMoveVelocity, 50 * Runner.DeltaTime);
		KCC.Move(desiredMoveVelocity);
	}

    private IEnumerator RegenerateEnergy()
    {
        isRegenerating = true;

        // Nếu thể lực về 0, chờ một thời gian trước khi hồi thể lực
        if (_currentEnergy == 0)
        {
            yield return new WaitForSeconds(0.7f);  // Chờ thời gian delay
        }

        // Bắt đầu hồi thể lực
        while (_currentEnergy < _MaxEnergy)
        {
            _currentEnergy += _energyRegenRate * Time.deltaTime;
            
            if(!isRegenerating == false) break;

            yield return null;  // Chờ 1 frame trước khi tiếp tục hồi
        }
         //Đảm bảo không vượt quá max
        if( _currentEnergy > _MaxEnergy)_currentEnergy = _MaxEnergy;
    }

    //Nghiêng máy bay 
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

    //Kiểm tra va chạm (chưa fix được bug collider của KCC không hoạt động T_T, dùng tạm )
    private bool CheckCollisonWithTerrian(Vector3 diraction){
        if(Physics.Raycast(transform.position, diraction, out _hit, 1, _MapLayer)){
            return true;
        }
        return false;
    }

    //==================================================================
    //=============Xử lý hiệu ứng=======================================

    //Hiệu ứng tăng tốc
    private void SpeepUpEffect(bool isSprint){
        if(!Object.HasInputAuthority) return;
        if(isSprint){
            //Tạo hiệu ứng tăng tốc
            _mainCamera.GetComponent<RenderFeatureToggler>().ActivateRenderFeatures(0,true);
            _mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(_mainCamera.GetComponent<Camera>().fieldOfView, 115, Time.deltaTime * 10);
            GetComponentInChildren<Volume>().enabled = false;
           
        } else {
            //bỏ hiệu ứng
            _mainCamera.GetComponent<RenderFeatureToggler>().ActivateRenderFeatures(0,false);
            _mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(_mainCamera.GetComponent<Camera>().fieldOfView, 80, Time.deltaTime * 10);
            GetComponentInChildren<Volume>().enabled = false;
        }
    }

    //Hiệu ứng rung camera
    private Vector3 GetCameraShakeOffset(){
        float shakeAmount = 0.05f; 
        return new Vector3(
            Random.Range(-shakeAmount, shakeAmount),
            Random.Range(-shakeAmount, shakeAmount),
            Random.Range(-shakeAmount, shakeAmount)
        );
    }

    //==================================================================
    //=============Xử lý tấn công=======================================

    private void Shoot()
    {   
        foreach (var attack in _attackers)
        {
            attack.Attack(MyTeam);
        }

        _FireRate.StartCooldown();

    }


    //==================================================================
    //=============Xử lý sát thương=====================================

    public void TakeDamage(int damage){
        _currentHealth -= damage;

        infoplate.UpdateHP(_currentHealth, _MaxHealth);
        _HpDisplay.UpdateHP(_currentHealth, _MaxHealth);
        if(_currentHealth <= 0){
            //Chết
        }
    }

    //Hiển thị tên người chơi
    private void OnNicknameChanged(){

		if (HasStateAuthority){
            infoplate.gameObject.SetActive(false);
            return; // Chỉ hiển thị tên của người chơi khác
        }

		infoplate.SetNickname(Nickname);
        infoplate.SetTeamColor(MyTeam, MyTeam == Team.Red? Red:Blue);
	}

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(_RaycastPoint.position, CameraHandle.forward * 200);
    }

    public object[] GetInfo(){
        int maxHP = _MaxHealth;
        Color color = MyTeam == Team.Red? Red:Blue;
        object[] data = {maxHP, Nickname, color};

        return data;
    }

    // Animation event
}
    


public static class SynchronizeWithRotation 
{
    public static Matrix4x4 Matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 Synchronize(this Vector3 input) => Matrix.MultiplyPoint3x4(input);
}

public enum Team
{
    Red,
    Blue
}
