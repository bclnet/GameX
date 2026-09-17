//using System;
//using UnityEngine;

//namespace GameEstate.Estates.Tes.Components
//{
//    public static class TestAsset
//    {
//        static Estate Estate = EstateManager.GetEstate("Tes");
//        static UnityArchive Archive = new UnityArchive(Estate.OpenArchive(new Uri("game:/Morrowind.bsa#Morrowind")));
//        //static TesUnityArchive Archive = new TesUnityArchive(Estate.OpenArchive(new Uri("http://192.168.1.3/ASSETS/Morrowind/Morrowind.bsa#Morrowind")));
//        //static TesUnityArchive Archive = new TesUnityArchive(Estate.OpenArchive(new Uri("game:/Skyrim*#SkyrimVR")));
//        //static TesUnityArchive Archive = new TesUnityArchive(Estate.OpenArchive(new Uri("game:/Fallout4*#Fallout4VR")));

//        public static void Awake() { }
//        public static void Start()
//        {
//            // Morrowind
//            //MakeObject("meshes/i/in_dae_room_l_floor_01.nif");
//            //MakeObject("meshes/w/w_arrow01.nif");

//            //MakeObject("meshes/x/ex_common_balcony_01.nif");
//            //MakeTexture("");

//            // Skyrim
//            //var nifFileLoadingTask = await Asset.LoadObjectInfoAsync("meshes/actors/alduin/alduin.nif");
//            //MakeObject("meshes/markerx.nif");
//            //MakeObject("meshes/w/w_arrow01.nif");
//            //MakeObject("meshes/x/ex_common_balcony_01.nif");
//        }
//        public static void OnDestroy() => Archive.Dispose();
//        public static void Update() { }

//        static GameObject MakeObject(string path) => Archive.CreateObject(path);
//        static GameObject MakeTexture(string path)
//        {
//            var materialManager = Archive.MaterialManager;
//            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube); // GameObject.Find("Cube"); // CreatePrimitive(PrimitiveType.Cube);
//            var meshRenderer = obj.GetComponent<MeshRenderer>();
//            var materialProp = new MaterialProp
//            {
//                Textures = new MaterialTextures { MainFilePath = path },
//            };
//            meshRenderer.material = materialManager.BuildMaterialFromProperties(materialProp);
//            return obj;
//        }
//        static void MakeCursor(string path) => Cursor.SetCursor(Archive.LoadTexture(path), Vector2.zero, CursorMode.Auto);
//    }
//}
