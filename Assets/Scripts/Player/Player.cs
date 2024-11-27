using System.Collections;
using UnityEngine;
using Fusion;
using Multiplayer;
using Fusion.Addons.SimpleKCC;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class Player : NetworkBehaviour
{
    [Header("Move")]
    public SimpleKCC KCC;
	public PlayerInput PlayerInput;
    [SerializeField] private float MoveSpeed = 2.0f;
    [SerializeField] private float SprintSpeed = 5.335f;
    [SerializeField] private float _MaxEnergy = 100f;
    [SerializeField] private float _currentEnergy;
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
    [SerializeField] private float _respawnTime;
    public enum PlayerState{
        Active,
        Death,
        Rest
    }
    [Networked, OnChangedRender(nameof(OnStateChange))] public PlayerState State { get; set; }	= PlayerState.Active;
    [Networked, OnChangedRender(nameof(OnVisualToggle))] private bool VisualToggle { get; set; }
    [Networked] private TickTimer respawnTimer { get; set; }
    [Networked] private TickTimer NoDamageTimer { get; set; }
    [Networked] public Team MyTeam { get; set; }
    [Networked, OnChangedRender(nameof(UpdateHpDisplay))] private int _currentHealth { get; set; }
    [Networked] private bool isTeleportDone{ get; set; } =false;

    public bool IsReady;
    private float _respawnInSeconds = -1;
    
    [Header("Camera")]
    public bool LockCameraPosition = false;
    [SerializeField]private Transform CameraPivot;
    [SerializeField]private Transform CameraHandle; 
    private GameObject _mainCamera;
    
    [Header("Visual")]
    private Animator _animator;
    private PlayerHub _playerHub;
    [Networked] public HpBarDisplay _HpDisplay {get; set;}
    [SerializeField] private UIInfoplate infoplate;
    public Transform _Model;
	private	Color Blue = new(0,152,255);
    [SerializeField] private NetworkObject DeathEffect;
    [SerializeField] private GameObject[] TeleportIn;
    [SerializeField] private GameObject Wheel;
    
    private Vector3 _hitPosition;
    private Vector3 _hitNormal;

    [Networked, HideInInspector, Capacity(24), OnChangedRender(nameof(OnNicknameChanged))]
	public string Nickname { get; set;}

    public override void Spawned()
    {
        //GameManager.Instance.Players.Add(Object.StateAuthority, this);

        //Runner.SetIsSimulated(Object, true);
        _currentHealth = _MaxHealth;
        _currentEnergy = _MaxEnergy;

        Wheel.SetActive(true);

        _animator = GetComponent<Animator>();
       
        _mainCamera = Camera.main.gameObject;
        
        Nickname = PlayerPrefs.GetString("PlayerName");
        OnNicknameChanged();
        _respawnInSeconds = 0;

        if(!Object.HasStateAuthority){
            GetComponentInChildren<Camera>().enabled = false;     
        }
        //ẩn trong màn hình chuẩn bị
        infoplate.gameObject.SetActive(false);
        
        KCC.SetGravity(0);
    }

    public override void FixedUpdateNetwork()
	{
	
        if (Object.HasStateAuthority && GameManager.Instance.State == GameState.Playing){
            ProcessInput(PlayerInput.CurrentInput);
                
		    PlayerInput.ResetInput();
			CheckRespawn();

			if (isTeleportDone && respawnTimer.Expired(Runner)){
                isTeleportDone = false;
				VisualToggle = true;
            }
		}
	}

	public override void Render()
    {
        if(GameManager.Instance.State != GameState.Playing) return;
        Wheel.SetActive(false);

        if(!Object.HasStateAuthority) return;
        // Render hiệu ứng nghiêng máy bay 
        RotationJetWithMovement(PlayerInput.CurrentInput);

		float smoothZTiltAngle = Mathf.SmoothDampAngle( _Model.localRotation.eulerAngles.z, -currentZTiltAngle, ref _zRotationVelocity, RotationSmoothTime);
        float smoothXTiltAngle = Mathf.SmoothDampAngle( _Model.localRotation.eulerAngles.x, currentXTiltAngle, ref _xRotationVelocity, RotationSmoothTime);
       
        _Model.localRotation = Quaternion.Euler(smoothXTiltAngle, 0, smoothZTiltAngle); 

        //Render vị trí CameraHandle
        if(PlayerInput.CurrentInput.SpeedUpEffect && _currentEnergy > 0){
            // Hạ thấp + rung camera 
            Vector3 targetPosition = CameraHandle.transform.localPosition;
            targetPosition.y = Mathf.Lerp(CameraHandle.transform.localPosition.y, 3f, Runner.DeltaTime * 10);
            CameraHandle.transform.localPosition = targetPosition + GetCameraShakeOffset();
        } else {
            // Giữ nguyên vị trí camera, đặt về vị trí mặc định
            Vector3 targetPosition = new(0,CameraHandle.transform.localPosition.y,-25);
            targetPosition.y = Mathf.Lerp(CameraHandle.transform.localPosition.y, 5, Runner.DeltaTime * 10);
            CameraHandle.transform.localPosition = targetPosition;
        }
	}

	private void LateUpdate()
	{
        if (HasStateAuthority == false) return;
        // Xử lý cam
        CameraPivot.rotation = Quaternion.Euler(PlayerInput.CurrentInput.LookRotation);

        //Xử lý hồi năng lượng
        if (!isRegenerating && _currentEnergy < _MaxEnergy && !PlayerInput.CurrentInput.SpeedUpEffect)
        {
            //Bắt đầu hồi năng lượng
            _currentEnergy += 1;
            //Đảm bảo không vượt quá max
            if( _currentEnergy > _MaxEnergy)_currentEnergy = _MaxEnergy;
        }

        //hiển thị
        PlayerHub.Instance.OnUpdateEnergyBar(_currentEnergy, _MaxEnergy, isRegenerating);

		// Truyền trạng thái vị trí cho camera qua CameraHandle
		_mainCamera.transform.SetPositionAndRotation(CameraHandle.position, CameraHandle.rotation);
	}

    //=============Xử lý di chuyển=======================================
	private void ProcessInput(GameplayInput input)
    {
		if(GameManager.Instance.State != GameState.Playing) return;

        //Set tốc độ di chuyển
		float speed = (input.Sprint && _currentEnergy > 1)? SprintSpeed : MoveSpeed;

        //cho phép tăng tốc nếu đủ năng lượng (chỉ bắt đầu hồi khi người chơi nhả nút tăng tốc)
        if(input.SpeedUpEffect && _currentEnergy > 0){
           _currentEnergy -= 0.25f;
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

        //tránh mất tốc đột ngột nếu có hiện tượng lag
        var desiredMoveVelocity = moveDirection * speed;
		_moveVelocity = Vector3.Lerp(_moveVelocity, desiredMoveVelocity, 50 * Runner.DeltaTime);
		KCC.Move(_moveVelocity);
	}

    //Coroutine hồi năng lượng
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
            _currentEnergy += 0.4f;
            if(!isRegenerating == false) break;
            yield return null;  // Chờ 1 frame trước khi tiếp tục hồi
        }
         //Đảm bảo không vượt quá max
        if( _currentEnergy > _MaxEnergy)_currentEnergy = _MaxEnergy;
    }

    //Nghiêng máy bay 
    private void RotationJetWithMovement(GameplayInput input)
    {
        float targetZTiltAngle = 0f;
        float targetXTiltAngle = 0f;

        //nghiêng theo hướng nhìn
        if(input.LookRotationDelta.x !=  0){
            currentZTiltAngle = _mainCamera.transform.eulerAngles.y - transform.eulerAngles.y;
            return;
        }

        //Nghiêng theo hướng lượn trái, phải
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
    private bool CheckCollisonWithTerrian(Vector3 diraction)
    {
        if(Physics.Raycast(transform.position, diraction, out _hit, 1, _MapLayer)){
            return true;
        }
        return false;
    }

    //==================================================================
    //=============Xử lý hiển thị=======================================
    //Hiệu ứng tăng tốc
    private void SpeepUpEffect(bool isSprint)
    {
        if(!Object.HasInputAuthority) return;
        if(isSprint){
            //Tạo hiệu ứng tăng tốc
            _mainCamera.GetComponent<RenderFeatureToggler>().ActivateRenderFeatures(0,true);
            _mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(_mainCamera.GetComponent<Camera>().fieldOfView, 85, Time.deltaTime * 10);
            GetComponentInChildren<Volume>().enabled = true;
           
        } else {
            //bỏ hiệu ứng
            _mainCamera.GetComponent<RenderFeatureToggler>().ActivateRenderFeatures(0,false);
            _mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(_mainCamera.GetComponent<Camera>().fieldOfView, 65, Time.deltaTime * 10);
            GetComponentInChildren<Volume>().enabled = false;
        }
    }

    //Hiệu ứng rung camera
    private Vector3 GetCameraShakeOffset()
    {
        float shakeAmount = 0.05f; 
        return new Vector3(
            UnityEngine.Random.Range(-shakeAmount, shakeAmount),
            UnityEngine.Random.Range(-shakeAmount, shakeAmount),
            UnityEngine.Random.Range(-shakeAmount, shakeAmount)
        );
    }

    //Hiển thị máu
    private void UpdateHpDisplay()
    {
        if(GameManager.Instance.State != GameState.Playing) return;
        infoplate.UpdateHP(_currentHealth, _MaxHealth);
        _HpDisplay.UpdateHP(_currentHealth, _MaxHealth);

        if(Object.HasStateAuthority)
            PlayerHub.Instance.OnUpdateHpBar(_currentHealth, _MaxHealth);
    }

    //Hiển thị tên người chơi
    private void OnNicknameChanged()
    {
		if (HasStateAuthority){
            return; // Chỉ hiển thị tên của người chơi khác
        }
		infoplate.SetNickname(Nickname);
       
	}

    //Tắt hiển thị
    private void OnVisualToggle()
    {
        _Model.gameObject.SetActive(VisualToggle);

        if(!Object.HasStateAuthority)
            infoplate.gameObject.SetActive(VisualToggle);

        foreach(WeaponBase attacker in GetComponent<DroneManager>()._attackers){
            attacker.transform.parent.gameObject.SetActive(VisualToggle);
        }
    }

    //==================================================================
    //=============Xử lý sát thương=====================================
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_TakeDamage(int damage)
    {
        if(State == PlayerState.Death) return;

        _currentHealth -= damage;
        if(_currentHealth <= 0){
            State = PlayerState.Death;
            //Chết
        }
    }

    //===================================================================
    //============Xử lý trạng thái=======================================
    private async void OnStateChange()
    {
        switch(State){
            case PlayerState.Active:
                VisualToggle = false;
                if(Object.HasStateAuthority)
                    PlayerHub.Instance.SetStatusDisplay(true);

                TeleportIn[(int)MyTeam].SetActive(false); //tắt hiệu ứng nếu nó vẫn còn bật
                TeleportIn[(int)MyTeam].SetActive(true);  //bật lại hiệu ứng

                _currentHealth = _MaxHealth;
                _currentEnergy = _MaxEnergy;

                
                respawnTimer = TickTimer.CreateFromSeconds(Runner, 1);
		        NoDamageTimer = TickTimer.CreateFromSeconds(Runner, 1);

                await Task.Delay(500);
                VisualToggle = true;

                break;

            case PlayerState.Death:
                VisualToggle = false;
                if(Object.HasStateAuthority)
                    PlayerHub.Instance.SetStatusDisplay(false);

                GetComponentInChildren<FlagCapturer>().DropFlag();

                NetworkObject Explosion = Runner.Spawn(DeathEffect, transform.position, transform.rotation, Object.InputAuthority);
                Explosion.GetComponent<ParticleSystem>().Play();

                _respawnInSeconds = _respawnTime;

                break;
        }
    }

    private void CheckRespawn()
	{
		if (_respawnInSeconds >= 0)
		{
			_respawnInSeconds -= Runner.DeltaTime;
            PlayerHub.Instance.UpdateRespawnTime((int)_respawnInSeconds);

			if (_respawnInSeconds <= 0)
			{
                //lấy điểm spawn
				Vector3 spawnpt = MyTeam == Team.Blue? GameManager.Instance.BlueTeamSpawnPoint : GameManager.Instance.RedTeamSpawnPoint;
                Quaternion spawnRot = Quaternion.Euler(0, MyTeam == Team.Blue? 0 : 180, 0);
				if (spawnpt == null){
					_respawnInSeconds = Runner.DeltaTime;
					return;
				}
                
				_respawnInSeconds = -1;
				
                //đưa lại người chơi về điểm spawn
                Teleport( spawnpt + new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y) * 10, spawnRot );

                //tái kích hoạt
                State = PlayerState.Active;
                isTeleportDone = true;
			}
		}
	}

    public void Respawn(){
        _respawnInSeconds = 1;
        VisualToggle = false;
        GetComponentInChildren<FlagCapturer>().DropFlag();
        State = PlayerState.Rest;

    }

    // đưa người chơi vào trạng thái sẵn sàng
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetReady(bool ReadyOrNot)
    {
        IsReady = ReadyOrNot;

        string maxHP = !ReadyOrNot?  "X" : "O";
        Color color = !ReadyOrNot? Color.red : Color.green;
        object[] data = {maxHP, Nickname, color};

        _HpDisplay.SetInfo(data);
    }

    //đưa người chơi vào trạng thái chơi
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_StartGame(Team team){
        //set team
        MyTeam = team;

        //Set up chức năng cướp cờ
        GetComponentInChildren<FlagCapturer>().SetCapturer(team);
        
        string maxHP = _MaxHealth.ToString();
        Color color = MyTeam == Team.Red? Color.red : Blue;
        object[] data = {maxHP, Nickname, color};

        _HpDisplay.SetInfo(data);

        //tái hiển thị
        VisualToggle = true;
      
        infoplate.SetTeamColor(color);

        if(Object.HasStateAuthority || (!Object.HasStateAuthority && MyTeam != GameManager.Instance._player.MyTeam))
            _HpDisplay.gameObject.SetActive(false);
    }

    public void Teleport(Vector3 position, Quaternion rotation){
        KCC.SetPosition(position);
        KCC.SetLookRotation(rotation);
    }

    // Xử lý Animation (Tạm chưa có j)
}

public enum Team{
    Red,
    Blue
}
