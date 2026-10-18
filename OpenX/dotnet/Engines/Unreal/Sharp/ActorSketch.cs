// Unreal is the host process.
// Sketch only. Not in a csproj, and not expected to compile in GameX.sln.
// The UnrealSharp usings are commented. Each generated type is named where it is used,
// so this file can be read without the bindings a UE project would generate.

// using UnrealSharp.Attributes; // [UClass], [UProperty]
// using UnrealSharp.Engine;     // AActor, AStaticMeshActor

using GameX;

namespace OpenX.Unreal.Sharp;

/// <summary>
/// One actor. Unreal constructs it. BeginPlay opens one GameX archive and spawns one static mesh.
/// </summary>
[UClass] // UnrealSharp.Attributes.UClass. The class must be partial.
public partial class AGameXArchiveActor : AActor // UnrealSharp.Engine.AActor
{
    public override void BeginPlay()
    {
        base.BeginPlay();
        Archive archive = FamilyManager.GetFamily("Arkane").GetArchive("game:/#AF");
        // UnrealSharp.Engine.AStaticMeshActor.Spawn(worldContext, actorClass, transform).
        // actorClass is the generated class to spawn. One archive entry is not a UStaticMesh; that builder is not this sketch.
        AStaticMeshActor.Spawn(this, typeof(AStaticMeshActor), GetActorTransform());
        _ = archive.Files[0];
    }
}
