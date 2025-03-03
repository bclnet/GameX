using GameX;
using GameX.Platforms;

public class UnityPlatform : UnityEngine.MonoBehaviour
{
    static UnityPlatform() => Platform.Platforms.Add(GameX.Platforms.UnityPlatform.Startup);
}