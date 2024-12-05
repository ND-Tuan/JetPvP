using System.Collections;
using System.Collections.Generic;
using Multiplayer;
using UnityEngine;

public class PlayerSoundFx : MonoBehaviour
{
    [SerializeField] private AudioSource _JetFX;
    [SerializeField] private AudioSource _JetThrustFX;
    [SerializeField] private AudioSource _CollisionWarningFx;

    private void Awake()
    {
        _JetThrustFX.enabled = false;
        _JetFX.enabled = false;
        _CollisionWarningFx.enabled = false;
    }

    public void JetBootsUp(bool isJetBootsUp){
        _JetThrustFX.enabled = isJetBootsUp;
        _JetFX.enabled = !isJetBootsUp;
    }

    public void PlayCollisionWarning(bool isPlay){
        _CollisionWarningFx.enabled = isPlay;
        PlayerHub.Instance.SetCaution(isPlay);

    }
}
