using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SoundManager
{
    private const string SOUND_ROOT = "@Sound_Root";
    private static readonly int _audioSourceTypeCount = System.Enum.GetValues(typeof(Define.AudioSourceType)).Length;

    private AudioSource[] _audioSource = new AudioSource[_audioSourceTypeCount];
    private AudioClip _currentBGMCLip;
    private Coroutine _delayedRoutine;
    private Tween _bgmTween;

    public void Init()
    {
        var soundRoot = GameObject.Find(SOUND_ROOT);
        if (soundRoot == null)
        {
            soundRoot = new GameObject(SOUND_ROOT);

            string[] sourceTypes = System.Enum.GetNames(typeof(Define.AudioSourceType));
            for (int i = 0; i < sourceTypes.Length; i++)
            {
                GameObject go = new GameObject(sourceTypes[i]);
                _audioSource[i] = go.AddComponent<AudioSource>();
                go.transform.SetParent(soundRoot.transform, false);
            }
            Object.DontDestroyOnLoad(soundRoot);
        }

        UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
    {
        //씬 전환시 코루틴 정리
        if(_delayedRoutine != null)
        {
            Managers.Coroutine.StopCoroutine(_delayedRoutine);
            _delayedRoutine = null;
        }

    }

    public void Destroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void Play(Define.AudioClip sourceName, Define.AudioSourceType sourceType)
    {
        PlaySound(sourceName, sourceType);
    }

    public void DelayedPlay(Define.AudioClip sourceName, Define.AudioSourceType sourceType, float delayedTime)
    {
        if(_delayedRoutine != null)
        {
            Managers.Coroutine.StopCoroutine(_delayedRoutine);
        }

        _delayedRoutine = Managers.Coroutine.StartCoroutine(DelayedPlayRoutine(sourceName, sourceType, delayedTime));
    }

    private IEnumerator DelayedPlayRoutine(Define.AudioClip sourceName, Define.AudioSourceType sourceType, float delayedTime)
    {
        yield return new WaitForSeconds(delayedTime);

        PlaySound(sourceName, sourceType);
    }

    public void Stop(Define.AudioSourceType sourceType)
    {
        _audioSource[(int)sourceType].Stop();
    }

    public void StopAll()
    {
        foreach(var source in _audioSource)
        {
            source.Stop();
        }
    }

    private void PlaySound(Define.AudioClip sourceName, Define.AudioSourceType sourceType)
    {
        //TODO 사운드 세팅 반영, 효과음/배경음

        var path = $"Sounds/{sourceType}/{sourceName}";
        //실제 clip load 부분
        Managers.Resource.LoadAsync<AudioClip>(path, (audioClip) =>
        {
            AudioSource source = _audioSource[(int)sourceType];

            if(audioClip == null || source == null) return;
            switch(sourceType)
            {
                case Define.AudioSourceType.Bgm:
                    if(_currentBGMCLip == audioClip && source.isPlaying) return;
                    _bgmTween?.Kill();
                    TransitionBgm(audioClip, source);
                    break;
                case Define.AudioSourceType.Sfx:
                    SetVolume(1, sourceType);
                    source.PlayOneShot(audioClip);
                    break;
                case Define.AudioSourceType.LoopSfx:
                    SetVolume(1, sourceType);
                    source.clip = audioClip;
                    source.loop = true;
                    source.Play();
                    break;
            }
        });
    }

    private void SetVolume(float volume, Define.AudioSourceType sourceType)
    {
        _audioSource[(int)sourceType].volume = volume;
    }

    private void TransitionBgm(AudioClip clip, AudioSource source, float duration = 0.25f)
    {
        _bgmTween?.Kill();
        
        Sequence seq = DOTween.Sequence().SetUpdate(true);

        if (source.isPlaying)
        {
            seq.Append(source.DOFade(0f, duration));
            seq.AppendCallback(() => {
                //clip swap
                _currentBGMCLip = clip;
                source.Stop();
                source.clip = clip;
                source.Play();
            });
            seq.Append(source.DOFade(1f, duration));
        }
        else
        {
            _currentBGMCLip = clip;
            source.clip = clip;
            source.volume = 0f;
            source.loop = true;
            source.Play();
            seq.Append(source.DOFade(1f, duration));
        }

        _bgmTween = seq;
    }

}
