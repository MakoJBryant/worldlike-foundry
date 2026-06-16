Shader "Worldlike Foundry/SunCorona"
{
    Properties
    {
        _CoronaColor ("Corona Color", Color) = (1, 0.6, 0.1, 1)
        _Intensity ("Intensity", Range(0, 3)) = 1
        _Falloff ("Edge Falloff", Range(0.1, 5)) = 2
        _PulseSpeed ("Pulse Speed", Range(0, 2)) = 0.3
        _PulseAmount ("Pulse Amount", Range(0, 0.2)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend One One
        ZWrite Off
        Cull Front

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float3 viewDirWS  : TEXCOORD1;
            };

            float4 _CoronaColor;
            float _Intensity;
            float _Falloff;
            float _PulseSpeed;
            float _PulseAmount;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDirWS = GetWorldSpaceNormalizeViewDir(posWS);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);
                float3 viewDir = normalize(IN.viewDirWS);

                // Fresnel — bright at edges, fade toward center
                float fresnel = 1.0 - saturate(dot(normal, viewDir));
                fresnel = pow(fresnel, _Falloff);

                // Subtle pulse
                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;

                float3 color = _CoronaColor.rgb * fresnel * _Intensity * pulse;
                return float4(color, fresnel);
            }
            ENDHLSL
        }
    }
}