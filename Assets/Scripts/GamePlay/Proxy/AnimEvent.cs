using System;
using UnityEngine;

public class AnimEvent : MonoBehaviour
{
    public event Action StartAction;
    public event Action EndAction;
    public Action<int> animEvent;

    void Start()
    {
    }

    public void OnStartEvent()
    {
        StartAction?.Invoke();
        StartAction = null;
    }

    public void OnEndEvent()
    {
        EndAction?.Invoke();
        EndAction = null;
    }

    private void AnimEventTag(int value)
    {
        animEvent?.Invoke(value);
    }
}
