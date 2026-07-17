using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMgr : Singleton<SettingsMgr>
{
    private string SOUND_KEY = "OpenSound";
    private string MUSIC_KEY = "OpenMusic";
    private string VIBRATE_KEY = "OpenVibrate";

    private bool m_openSound;
    public bool OpenSound
    {
        get
        {
            return m_openSound;
        }
        set
        {
            m_openSound = value;
            LocalSave.SetBool(SOUND_KEY, value);
            SoundMgr.Instance.SetSoundVo(!m_openSound);
        }
    }

    private bool m_openMusic;
    public bool OpenMusic
    {
        get
        {
            return m_openMusic;
        }
        set
        {
            m_openMusic = value;
            LocalSave.SetBool(MUSIC_KEY, value);
            SoundMgr.Instance.SetMusicVol(!m_openMusic);
        }
    }

    private bool m_openVibrate;
    public bool OpenVibrate
    {
        get
        {
            return m_openVibrate;
        }
        set
        {
            m_openVibrate = value;
            LocalSave.SetBool(VIBRATE_KEY, value);
        }
    }

    public void Init()
    {
        m_openSound = LocalSave.GetBool(SOUND_KEY, true);
        m_openMusic = LocalSave.GetBool(MUSIC_KEY, true);
        m_openVibrate = LocalSave.GetBool(VIBRATE_KEY, true);

        SoundMgr.Instance.SetMusicVol(!m_openMusic);
        SoundMgr.Instance.SetSoundVo(!m_openSound);
    }

    public void Clear()
    {

    }

    public void Vibrate()
    {
        if (!OpenVibrate)
            return;
    }

    public bool GetSetting(SettingType type)
    {
        switch (type)
        {
            case SettingType.Sound:
                return OpenSound;
            case SettingType.Music:
                return OpenMusic;
            case SettingType.Vibrate:
                return OpenVibrate;
        }
        return true;
    }

    public void SetSetting(SettingType type, bool value)
    {
        switch (type)
        {
            case SettingType.Sound:
                OpenSound = value;
                break;
            case SettingType.Music:
                OpenMusic = value;
                break;
            case SettingType.Vibrate:
                OpenVibrate = value;
                break;
            default:
                break;
        }
    }
}

public enum SettingType
{
    Sound,
    Music,
    Vibrate
}
