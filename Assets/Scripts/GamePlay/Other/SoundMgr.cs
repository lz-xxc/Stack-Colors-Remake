using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// 声音管理器 主要用于播放声音
/// 依赖 LocalAssetMgr
/// </summary>
public class SoundMgr : SingletonMonoBehavior<SoundMgr>
{
    public const string PART = "4KdcfgEA";
    private float musicValue = 0.6f;
    private float soundValue = 0.75f;
    private AudioSource soundSource;
    private AudioSource bgSource;
    private Dictionary<string, float> playRecord = new Dictionary<string, float>();
    private Dictionary<GameObject, AudioSource> playSingle = new Dictionary<GameObject, AudioSource>(); // 独立播放
    private float playCD = 0.00001f;
    private float offsetBGMVolume;
    private float offsetSoundVolume;

    protected override void Awake()
    {
        base.Awake();
    }

    // Use this for initialization
    public void Init()
    {
        GameObject bgGo = new GameObject();
        bgGo.name = "musicBg";
        bgGo.transform.SetParent(transform, false);
        bgSource = bgGo.AddMissingComponent<AudioSource>();
        bgSource.loop = true;
        //DontDestroyOnLoad(bgGo);

        GameObject soundGo = new GameObject();
        soundGo.name = "soundGo";
        soundGo.transform.SetParent(transform, false);
        soundSource = soundGo.AddMissingComponent<AudioSource>();
        offsetBGMVolume = musicValue;
        offsetSoundVolume = soundValue;
        //DontDestroyOnLoad(soundGo);
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="_name">音乐名</param>
    public void PlayMusic(string _name)
    {
        AudioClip clip = LocalAssetMgr.Instance.Load_Music(_name);
        if (clip != null)
        {
            bgSource.clip = clip;
            bgSource.volume = SettingsMgr.Instance.OpenMusic ? musicValue : 0;
            bgSource.Play();
            bgSource.DOFade(offsetBGMVolume, 0.2f);
        }
    }

    /// <summary>
    /// 获取背景音乐的时长
    /// </summary>
    /// <returns>时长</returns>
    public float GetMusicLength()
    {
        return bgSource.clip.length;
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="_name">音乐名</param>
    public void PlaySound(string _name, float _pitch = 1, SoundVolumType volumType = SoundVolumType.Loud)
    {
        if (playRecord.ContainsKey(_name))
        {
            if (Time.time - playRecord[_name] < playCD)
                return;
            else
                playRecord[_name] = Time.time;
        }
        else
        {
            playRecord.Add(_name, Time.time);
        }
        if (SettingsMgr.Instance.OpenSound)
        {
            float volum = offsetSoundVolume;
            switch (volumType)
            {
                case SoundVolumType.Small:
                    volum = 0.2f;
                    break;
                case SoundVolumType.Mid:
                    volum = 0.4f;
                    break;
                case SoundVolumType.Loud:
                    volum = offsetSoundVolume;
                    break;
            }
            soundSource.volume = volum;
        }
        AudioClip clip = LocalAssetMgr.Instance.Load_Music(_name);
        if (clip == null)
            return;
        soundSource.pitch = _pitch;
        soundSource.PlayOneShot(clip);
    }

    /// <summary>
    /// 播放GameObject的独立音频
    /// </summary>
    /// <param name="target"></param>
    /// <param name="_name"></param>
    /// <param name="volumType"></param>
    public void PlaySingleSound(GameObject target, string _name, SoundVolumType volumType = SoundVolumType.Loud, bool loop = true)
    {
        AudioSource singleSource;
        if (playSingle.ContainsKey(target))
        {
            singleSource = playSingle[target];
            singleSource.enabled = true;
        }
        else
        {
            singleSource = target.AddMissingComponent<AudioSource>();
            playSingle.Add(target, singleSource);
        }
        if (SettingsMgr.Instance.OpenSound)
        {
            float volum = offsetSoundVolume;
            switch (volumType)
            {
                case SoundVolumType.Small:
                    volum = 0.2f;
                    break;
                case SoundVolumType.Mid:
                    volum = 0.4f;
                    break;
                case SoundVolumType.Loud:
                    volum = offsetSoundVolume;
                    break;
            }
            singleSource.volume = volum;
        }
        else
        {
            singleSource.volume = 0;
        }
        AudioClip clip = LocalAssetMgr.Instance.Load_Music(_name);
        if (clip == null)
            return;
        singleSource.loop = loop;
        singleSource.clip = clip;
        singleSource.Play();
    }

    /// <summary>
    /// 暂停GameObject的独立音频
    /// </summary>
    /// <param name="target"></param>
    public void StopSingleSound(GameObject target)
    {
        if (!playSingle.ContainsKey(target))
        {
            return;
        }
        AudioSource singleSource = playSingle[target];
        singleSource.Stop();
        singleSource.enabled = false;
    }

    public void StopAllSingleSound()
    {
        foreach (AudioSource item in playSingle.Values)
        {
            item.Stop();
        }
    }

    /// <summary>
    /// 释放所有独立音频
    /// </summary>
    public void ClearSingleSound()
    {
        playSingle.Clear();
    }

    /// <summary>
    /// 设置背景音乐的音量
    /// </summary>
    /// <param name="vol">声音大小(0.0 到1.0)</param>
    /// <param name="close">是否静音</param>
    public void SetMusicVol(bool close)
    {
        offsetBGMVolume = close ? 0 : musicValue;
        bgSource.volume = close ? 0 : musicValue;
    }

    /// <summary>
    /// 设置音效的音量
    /// </summary>
    /// <param name="vol">声音大小(0.0 到1.0)</param>
    /// <param name="close">是否静音</param>
    public void SetSoundVo(bool close)
    {
        offsetSoundVolume = close ? 0 : soundValue;
        soundSource.volume = close ? 0 : soundValue;
    }
}

public enum SoundVolumType
{
    Small,
    Mid,
    Loud,
}