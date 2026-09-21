using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject menuCanvasParent;
    private Stack<BaseScreen> screenStack = new Stack<BaseScreen>();
    public List<BaseScreen> screens;
    private BaseScreen GetBaseScreen(ScreenType type) => screens.Find(item => item.Type == type);
    private BaseScreen InstaintiateScreen(ScreenType type)
    {
        var screenPrefab = GetBaseScreen(type);
        if (screenPrefab != null)
        {
            var screenInstance = Instantiate(screenPrefab, menuCanvasParent.transform);
            return screenInstance;
        }
        return null;
    }

    public void OpenScreen(ScreenType type, System.Action onComplete = null)
    {
        BaseScreen screen = InstaintiateScreen(type);
        if (screen != null)
        {
            screen.Open(onComplete);
            screenStack.Push(screen);
        }
    }

    public void CloseScreen(System.Action onComplete = null)
    {
        if (screenStack.Count > 0)
        {
            BaseScreen screen = screenStack.Pop();
            screen.Close(onComplete);
            Destroy(screen.gameObject);
        }
    }
}
