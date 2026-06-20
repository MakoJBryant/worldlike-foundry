#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

// Set once per frame from C# via Shader.SetGlobalVector / SetGlobalColor.
float3 _SunPosition;
float3 _SunColor;

void CalculateMainLight_float(float3 WorldPos, out float3 Direction, out float3 Color,
        out half DistanceAtten, out half ShadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
        Direction = float3(0.5, 0.5, 0);
        Color = 1;
        DistanceAtten = 1;
        ShadowAtten = 1;
#else
    Direction = normalize(_SunPosition - WorldPos);
    Color = _SunColor;
    DistanceAtten = 1;
    ShadowAtten = 1;
#endif
}

void AddAdditionalLights_float(float Smoothness, float3 WorldPosition, float3 WorldNormal, float3 WorldView,
    float MainDiffuse, float MainSpecular, float3 MainColor,
    out float Diffuse, out float Specular, out float3 Color)
{
    Diffuse = MainDiffuse;
    Specular = MainSpecular;
    Color = MainColor * (MainDiffuse + MainSpecular);
#ifndef SHADERGRAPH_PREVIEW
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, WorldPosition);
        half NdotL = saturate(dot(WorldNormal, light.direction));
        half atten = light.distanceAttenuation * light.shadowAttenuation;
        half thisDiffuse = atten * NdotL;
        half thisSpecular = LightingSpecular(thisDiffuse, light.direction, WorldNormal, WorldView, 1, Smoothness);
        Diffuse += thisDiffuse;
        Specular += thisSpecular;
        Color += light.color * (thisDiffuse + thisSpecular);
    }
#endif
    half total = Diffuse + Specular;
    Color = total <= 0 ? MainColor : Color / total;
}
#endif