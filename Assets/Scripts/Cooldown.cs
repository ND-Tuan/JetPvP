using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cooldown
{
    #region Variables

    [SerializeField] private float _cooldownTime;

    private float _nextFireTime;

    #endregion

    public float getCD()
    {
        return _cooldownTime;
    }

    public bool IsCoolingDown => Time.time < _nextFireTime;

    public void StartCooldown() => _nextFireTime = Time.time + _cooldownTime;
}