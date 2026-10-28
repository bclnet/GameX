using OpenX;
using UnityEngine;

namespace GameX.Engines.Unity;

static class ShellBind {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() => ShellPipe.OnItem = line => {
        var next = ShellState.FromArgument(line);
        UnityEngine.Object.FindAnyObjectByType<ViewInfo>()?.Show(next);
    };
}
