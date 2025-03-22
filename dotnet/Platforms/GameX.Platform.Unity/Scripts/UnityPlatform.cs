using GameX.Platforms;

public class UnityPlatform : UnityEngine.MonoBehaviour
{
    static UnityPlatform() => PlatformX.Activate(GameX.Platforms.UnityPlatform.This);
}