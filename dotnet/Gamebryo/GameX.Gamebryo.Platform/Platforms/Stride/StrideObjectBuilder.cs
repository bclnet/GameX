//using GameX.Bethesda.Formats;
//using GameX.Platforms;
//using OpenStack.Gfx;

//namespace GameX.Bethesda.Platforms.Stride;

//public class StrideObjectBuilder : ObjectBuilderBase<Entity, Material, Texture2D>
//{
//    Entity _prefabObject;
//    readonly int _markerLayer;

//    public StrideObjectBuilder(int markerLayer) : base()
//    {
//        _markerLayer = markerLayer;
//    }

//    public override void EnsurePrefab()
//    {
//        if (_prefabObject == null)
//        {
//            _prefabObject = new GameObject("_Prefabs");
//            _prefabObject.SetActive(false);
//        }
//    }

//    public override Entity CreateNewObject(Entity prefab) => GameObject.Instantiate(prefab);

//    public override Entity CreateObject(object source, IMaterialManager<Material, Texture2D> materialManager)
//    {
//        var file = (NiFile)source;
//        // Start pre-loading all the NIF's textures.
//        foreach (var texturePath in file.GetTexturePaths()) materialManager.TextureManager.PreloadTexture(texturePath);
//        var objBuilder = new NifObjectBuilder(file, materialManager, _markerLayer);
//        var prefab = objBuilder.BuildObject();
//        prefab.transform.parent = _prefabObject.transform;
//        // Add LOD support to the prefab.
//        var LODComponent = prefab.AddComponent<LODGroup>();
//        var LODs = new LOD[1] { new LOD(0.015f, prefab.GetComponentsInChildren<Renderer>()) };
//        LODComponent.SetLODs(LODs);
//        return prefab;
//    }
//}