using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SoundManager : MonoManager<SoundManager>
{
    [System.Serializable]
    public class SoundEntry
    {
        public SoundId id;
        public AudioClip clip;

    }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private SoundEntry[] entries;

    private Dictionary<SoundId, AudioClip> _map;

    protected override void OnManagerAwake()
    {
        _map = new Dictionary<SoundId, AudioClip>(entries.Length);
        foreach (var e in entries)
        {
            if (e != null && e.clip != null) _map[e.id] = e.clip;
        }
    }


    // SFX 재생(단일 재생)
    public void PlaySFX(SoundId id)
    {
        if (sfxSource == null) return;

        if (_map.TryGetValue(id, out var clip) && clip != null)
        {
            sfxSource?.PlayOneShot(clip);
        }
    }

    // SFX 재생(반복 재생)
    public void StartLoopSFX(SoundId id)
    {
        Debug.Log("재생1");
        if (sfxSource == null) return;

        if (_map.TryGetValue(id, out var clip) && clip != null)
        {
            Debug.Log("재생2");
            // 이미 같은 클립이 루프 중이면 재시작 방지
            if (sfxSource.loop && sfxSource.clip == clip && sfxSource.isPlaying) return;

            sfxSource.clip = clip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
    }

    // SFX 반복 재생 정지
    public void StopLoopSFX()
    {
        if (sfxSource == null) return;

        if (sfxSource.loop)
        {
            sfxSource.Stop();
            sfxSource.loop = false;
            sfxSource.clip = null;
        }
    }

    // BGM 재생
    public void PlayBGM()
    {
        // 씬에 맞는 배경음악 인스펙터창에서 할당 후 반복 재생 되도록 하기
    }


}
