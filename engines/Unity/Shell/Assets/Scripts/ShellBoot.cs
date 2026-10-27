using GameX.Engines.Unity;
using UnityEngine;

public class ShellBoot : MonoBehaviour {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() {
        Application.runInBackground = true;
        Screen.fullScreenMode = FullScreenMode.Windowed;
        if (FindAnyObjectByType<ViewInfo>() != null) return;
        var view = new GameObject("View");
        DontDestroyOnLoad(view);
        view.AddComponent<ViewInfo>();
    }
}
