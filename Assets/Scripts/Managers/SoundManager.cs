using System.Collections.Generic;
using UnityEngine;

public class SoundManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Sound;

    private Transform _root;
    private AudioSource _bgmSource;
    private List<AudioSource> _sfxSources = new List<AudioSource>();

    // SFX 풀 초기 크기
    private const int SFX_POOL_SIZE = 10;

    // [추가] 저장 키 및 BGM 밸런스 비율
    private const string PLAYER_PREFS_VOLUME_KEY = "MasterVolume";
    private const float BGM_VOLUME_RATIO = 0.4f;

    private float _volume = 1.0f;

    /// <summary>
    /// 전체 볼륨 (0.0 ~ 1.0)
    /// </summary>
    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Mathf.Clamp01(value);
            // [추가] 볼륨 변경 시 저장
            PlayerPrefs.SetFloat(PLAYER_PREFS_VOLUME_KEY, _volume);
            PlayerPrefs.Save();

            ApplyVolume();
        }
    }

    public void Init()
    {
        if (_root == null)
        {
            GameObject go = GameObject.Find("@Sound_Root");
            if (go == null)
            {
                go = new GameObject("@Sound_Root");
                Object.DontDestroyOnLoad(go);
            }
            _root = go.transform;
        }

        string bgmName = "@BGM_Source";
        GameObject bgmGo = new GameObject(bgmName);
        bgmGo.transform.SetParent(_root);
        _bgmSource = bgmGo.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;

        for (int i = 0; i < SFX_POOL_SIZE; i++)
        {
            CreateSFXSource();
        }

        // [추가] 저장된 볼륨 로드 (기본값 0.5)
        _volume = PlayerPrefs.GetFloat(PLAYER_PREFS_VOLUME_KEY, 0.5f);
        ApplyVolume();

        Debug.Log($"{ManagerType} Manager Init 완료. (Initial Volume: {_volume})");
    }

    public void Update() { }

    public void Clear()
    {
        StopBGM();
        foreach (var source in _sfxSources)
        {
            if (source.isPlaying) source.Stop();
        }
    }

    /// <summary>
    /// BGM을 재생합니다. (비동기 로드)
    /// </summary>
    public async void PlayBGM(string key)
    {
        AudioClip clip = await Managers.Resource.LoadAsync<AudioClip>(key);
        if (clip == null) return;

        if (_bgmSource.isPlaying)
            _bgmSource.Stop();

        _bgmSource.clip = clip;
        // [수정] BGM 비율 적용
        _bgmSource.volume = _volume * BGM_VOLUME_RATIO;
        _bgmSource.Play();
    }

    /// <summary>
    /// BGM 재생을 중지합니다.
    /// </summary>
    public void StopBGM()
    {
        if (_bgmSource != null && _bgmSource.isPlaying)
            _bgmSource.Stop();
    }

    /// <summary>
    /// 효과음(SFX)을 재생합니다. (비동기 로드)
    /// </summary>
    public async void PlaySFX(string key, float pitch = 1.0f)
    {
        AudioClip clip = await Managers.Resource.LoadAsync<AudioClip>(key);
        if (clip == null) return;

        AudioSource source = GetAvailableSFXSource();
        source.clip = clip;
        source.volume = _volume; // SFX는 100% 비율 사용
        source.pitch = pitch;
        source.Play();
    }

    private AudioSource GetAvailableSFXSource()
    {
        foreach (var source in _sfxSources)
        {
            if (!source.isPlaying) return source;
        }
        return CreateSFXSource();
    }

    private AudioSource CreateSFXSource()
    {
        string sfxName = $"@SFX_Source_{_sfxSources.Count}";
        GameObject go = new GameObject(sfxName);
        go.transform.SetParent(_root);
        AudioSource source = go.AddComponent<AudioSource>();
        source.loop = false;
        source.playOnAwake = false;

        _sfxSources.Add(source);
        return source;
    }

    /// <summary>
    /// 변경된 볼륨을 모든 오디오 소스에 적용합니다.
    /// </summary>
    private void ApplyVolume()
    {
        if (_bgmSource != null)
        {
            // [수정] BGM 비율 적용
            _bgmSource.volume = _volume * BGM_VOLUME_RATIO;
        }

        foreach (var source in _sfxSources)
        {
            source.volume = _volume;
        }
    }
}