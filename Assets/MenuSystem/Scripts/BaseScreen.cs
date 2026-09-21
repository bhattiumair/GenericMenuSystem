using UnityEngine;
using System;

public abstract class BaseScreen : MonoBehaviour, IScreen
{
    [SerializeField] private ScreenType _type;
    public ScreenType Type { get => _type; }

    public virtual void Open(Action onComplete = null)
    {
        gameObject.SetActive(true);
        OnBeforeOpen();
        onComplete?.Invoke();
    }
    public virtual void Close(Action onComplete = null)
    {
        gameObject.SetActive(false);
        OnBeforeClose();
        onComplete?.Invoke();
    }

    protected abstract void OnBeforeOpen();
    protected abstract void OnBeforeClose();

}
