using System;
using UnityEngine;

public interface IScreen
{
    ScreenType Type { get; }

    void Open(Action onComplete = null);
    void Close(Action onComplete = null);
    
}