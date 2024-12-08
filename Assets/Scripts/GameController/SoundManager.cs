using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SoundManager : MonoBehaviour
{
    [Header("Sound FX")]
    [SerializeField] private AudioClip _TakeOffFx;
    [SerializeField] private AudioClip _CollisionWarningFx;

    [Header("Music")]
    [SerializeField] private AudioClip _MainTheme;
    [SerializeField] private AudioClip _BackgroundMusic;

    [Header("Audio Source")]
    [SerializeField] GameObject _GlobalFXSourceRoot;
    private List<AudioSource> _FXSource = new List<AudioSource>();
    private AudioSource[] _GlobalFXSource;
    [SerializeField] private AudioSource _MusicSource;

    public static SoundManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);

        } else if (Instance != this){
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _GlobalFXSource = _GlobalFXSourceRoot.GetComponentsInChildren<AudioSource>();

        _MusicSource.clip = _MainTheme;
        _MusicSource.Play();
    }

    private void PlayFX(AudioClip clip){
       for (int i = 0; i < _GlobalFXSource.Length; i++)
		{
			var source = _GlobalFXSource[i];

			if (source.isPlaying == true || source.loop == true)
				continue;

			source.clip = clip;
			source.Play();
		}
    }
    
    public void PlayTakeOff(){
        PlayFX(_TakeOffFx);
    }


    public void PlayCollisionWarning(bool isPlay){
        _GlobalFXSource[0].clip = _CollisionWarningFx;
        if(!isPlay){
            _GlobalFXSource[0].Stop();
            return;
        }

        _GlobalFXSource[0].clip = _CollisionWarningFx;
        _GlobalFXSource[0].loop = true;

        if(!_GlobalFXSource[0].isPlaying)
            _GlobalFXSource[0].Play();
    }

    public void SetFXVolume(float volume){
        _FXSource.Clear();
        _FXSource = GameObject.FindGameObjectsWithTag("FXSoundSource").Select(x => x.GetComponent<AudioSource>()).ToList();
        foreach (var source in _FXSource){
           source.volume = volume;
        }
    }

    public void SetMusicVolume(float volume){
        _MusicSource.volume = volume;
    }

    public void SetMute(bool isMute){
        _MusicSource.mute = isMute;
        _FXSource.Clear();
        _FXSource = GameObject.FindGameObjectsWithTag("FXSoundSource").Select(x => x.GetComponent<AudioSource>()).ToList();
        foreach (var source in _FXSource){
           source.mute = isMute;
        }
    }
}
