Shader "Custom/LightRayShader"
{
    Properties
    {
        _Color ("Ray Color", Color) = (1,1,1,0.5)
        _Intensity ("Ray Intensity", Range(0, 10)) = 1
        _Attenuation ("Attenuation", Range(0, 1)) = 0.1
        _Scattering ("Scattering Amount", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
                float4 color : COLOR;
            };
            
            float4 _Color;
            float _Intensity;
            float _Attenuation;
            float _Scattering;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.color = v.color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Base color and intensity falloff along ray
                fixed4 col = _Color * _Intensity;
                col.a *= i.color.a; // Use vertex alpha for ray intensity
                
                // Calculate distance-based attenuation
                float distFromSource = length(i.uv.y);
                col.a *= lerp(1, 0.2, distFromSource * _Attenuation);
                
                // Apply scattering effect
                col.rgb += _Scattering * fixed3(0.1, 0.1, 0.1) * i.color.r;
                
                return col;
            }
            ENDCG
        }
    }
}