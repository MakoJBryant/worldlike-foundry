Shader "Worldlike Foundry/StarSkybox"
{
    Properties
    {
        _StarDensity ("Star Density", Range(10, 2000)) = 800
        _StarSize ("Star Size", Range(0.0001, 0.05)) = 0.003
        _StarBrightness ("Star Brightness", Range(0, 2)) = 1.0
        _ColoredStarChance ("Colored Star Chance", Range(0, 0.5)) = 0.1
        _BlueStarColor ("Blue Star Color", Color) = (0.6, 0.8, 1, 1)
        _RedStarColor ("Red Star Color", Color) = (1, 0.4, 0.3, 1)
        _Seed ("Seed", Range(0, 100)) = 42
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldDir   : TEXCOORD0;
            };

            float _StarDensity;
            float _StarSize;
            float _StarBrightness;
            float _ColoredStarChance;
            float4 _BlueStarColor;
            float4 _RedStarColor;
            float _Seed;

            float hash(float2 p)
            {
                p = frac(p * float2(443.8975, 397.2973));
                p += dot(p.xy, p.yx + 19.19);
                return frac(p.x * p.y);
            }

            float2 hash2(float2 p)
            {
                return float2(hash(p), hash(p + float2(1.0, 0.0)));
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                // Use inverse view matrix rotation only — strips translation
                // so stars are fixed to world orientation regardless of camera position
                float3x3 viewRotOnly = (float3x3)UNITY_MATRIX_I_V;
                OUT.worldDir = mul(viewRotOnly, IN.positionOS.xyz);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float3 dir = normalize(IN.worldDir);

                // Convert to spherical UV — uniform 2D space, no cube distortion
                float2 uv;
                uv.x = atan2(dir.z, dir.x) / (2.0 * 3.14159265) + 0.5;
                uv.y = asin(clamp(dir.y, -1.0, 1.0)) / 3.14159265 + 0.5;

                float3 color = float3(0, 0, 0);

                float2 scaled = uv * _StarDensity + _Seed * 17.3;
                float2 cell = floor(scaled);
                float2 local = frac(scaled);

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 neighbor = float2(x, y);
                        float2 cellID = cell + neighbor;

                        float2 rnd = hash2(cellID + _Seed);
                        float2 starPos = neighbor + rnd - local;

                        float dist = length(starPos);

                        if (dist < _StarSize)
                        {
                            float colorRoll = hash(cellID + _Seed + 99.1);
                            float3 starColor;

                            if (colorRoll < _ColoredStarChance * 0.5)
                            {
                                starColor = _BlueStarColor.rgb;
                            }
                            else if (colorRoll < _ColoredStarChance)
                            {
                                starColor = _RedStarColor.rgb;
                            }
                            else
                            {
                                float brightness = 0.7 + rnd.y * 0.3;
                                starColor = float3(brightness, brightness, brightness);
                            }

                            color += starColor * _StarBrightness;
                        }
                    }
                }

                return float4(color, 1.0);
            }
            ENDHLSL
        }
    }
}