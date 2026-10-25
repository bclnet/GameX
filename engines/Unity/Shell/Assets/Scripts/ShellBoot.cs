using GameX.Engines.Unity;
using UnityEngine;

public class ShellBoot : MonoBehaviour {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() {
        if (FindAnyObjectByType<ViewInfo>() != null) return;
        var view = new GameObject("View");
        DontDestroyOnLoad(view);
        view.AddComponent<ViewInfo>();
    }
}
