using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// 基础界面信息
/// </summary>
[SerializeField]
public class WindowInfo : MonoBehaviour
{
    public WindowType windowType = WindowType.Normal;
    public OpenAnimType openAnimType = OpenAnimType.None;
    public OpenAnimType closeAnimType = OpenAnimType.None;
    public float animTime = 0.3f;
    public Ease openEase = Ease.Linear;
    public Ease closeEase = Ease.Linear;
    public bool closeOnEmpty = false;
    public bool mask = false;
    public float maskAlpha = 0.38f;
    public Vector3 defaultPos = Vector3.zero;
    public Vector3 openPos = Vector3.zero;
    public int group = 0;

}
