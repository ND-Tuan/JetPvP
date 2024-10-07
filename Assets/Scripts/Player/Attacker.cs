using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacker : MonoBehaviour
{
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _range = 100f;
    [SerializeField] private Cooldown _fireRate;

    [SerializeField] private float _maxAngleDifference = 30f; // Góc lệch tối đa cho phép
    [SerializeField] private float _lerpSpeed = 5f; // Tốc độ chuyển hướng
    [SerializeField] private Transform _FirePoint;
    [SerializeField] GameObject _MuzzleFlash;

    private float _nextTimeToFire = 0f;
    private Vector3 _currentDirection;

    void Start()
    {
        _currentDirection = _FirePoint.forward;
    
        
    }

    void Update()
    {   
        if(!_fireRate.IsCoolingDown && Input.GetMouseButton(0)){
            Shoot();
        }
        
    }

    private void Shoot()
    {   
        _MuzzleFlash.SetActive(true);

        Ray screenRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        Vector3 targetDirection = screenRay.direction;

        // Tính góc giữa hướng từ FirePoint và hướng từ tâm màn hình
        float angleDifference = Vector3.Angle(_FirePoint.forward, targetDirection);

        // Nếu góc lệch quá lớn, bắn thẳng từ FirePoint
        if (angleDifference > _maxAngleDifference){
            targetDirection = _FirePoint.forward;
        }

        // Chuyển hướng từ từ
        _currentDirection = Vector3.Lerp(_currentDirection, targetDirection, Time.deltaTime * _lerpSpeed);

        RaycastHit hit;
        if (Physics.Raycast(_FirePoint.position, _currentDirection, out hit, _range)){

            if(hit.transform.tag != "Player") return;
            
            Player enemy = hit.transform.GetComponent<Player>();
            if (enemy == null) return;

            //enemy.TakeDamage(_damage);    
        }

        _fireRate.StartCooldown();
        Invoke(nameof(DeAcetivateMuzzleFlash), 0.1f);
    }

    private void DeAcetivateMuzzleFlash()
    {
        _MuzzleFlash.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(_FirePoint.position, _currentDirection * _range);
    }
}
