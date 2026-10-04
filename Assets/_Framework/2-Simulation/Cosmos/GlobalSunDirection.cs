using UnityEngine;

[ExecuteAlways]
public class GlobalSunDirection : MonoBehaviour
{
    public Color sunColor = Color.white;

    static readonly int SunPosID = Shader.PropertyToID("_SunPosition");
    static readonly int SunColorID = Shader.PropertyToID("_SunColor");

    void Update()
    {
        Shader.SetGlobalVector(SunPosID, transform.position);
        Shader.SetGlobalColor(SunColorID, sunColor);
    }
}