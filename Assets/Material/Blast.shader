Shader "Custom/URPBlastTransparentLit_NoFog"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseMap ("Base Map", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // We no longer include fog macros since the Fog.hlsl file isn’t available.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float3 normal     : NORMAL;
            };

            struct Varyings
            {
                float4 positionH : SV_POSITION;
                float2 uv        : TEXCOORD0;
                float3 normalWS  : TEXCOORD1;
                float3 worldPos  : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Metallic;
                float _Smoothness;
            CBUFFER_END

            sampler2D _BaseMap;
            float4 _BaseMap_ST;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionH = TransformWorldToHClip(worldPos);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normal);
                OUT.worldPos = worldPos;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample the base texture and tint it with _BaseColor.
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                // Basic lighting using the main directional light.
                UnityLight mainLight = GetMainLight();
                float3 normal = normalize(IN.normalWS);
                float3 lightDir = mainLight.direction;
                float ndotl = saturate(dot(normal, -lightDir));
                float3 diffuse = albedo.rgb * ndotl;

                // Simple specular term using smoothness and metallic.
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.worldPos);
                float3 halfDir = normalize(viewDir - lightDir);
                float ndoth = saturate(dot(normal, halfDir));
                float specular = pow(ndoth, 1.0/(_Smoothness + 0.001)) * _Metallic;

                float3 litColor = diffuse + specular;

                half4 output;
                output.rgb = litColor;
                output.a = albedo.a; // Alpha controls transparency.
                return output;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Forward"
}
