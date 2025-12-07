using Khronos.Collada;

namespace GameX.Formats.Collada;

partial class ColladaFileWriter {
    /// <summary>
    /// Adds the Scene element to the Collada document.
    /// </summary>
    void SetScene()
        => daeObject.Scene = new Collada_Scene {
            Visual_Scene = new Collada_Instance_Visual_Scene { URL = "#SceneBase", Name = "SceneBase" }
        };
}