Shader"Custom/Water"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Specular ("Specular", Color) = (0.2, 0.2, 0.2)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf StandardSpecular alpha vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        #pragma multi_compile _ HEX_MAP_EDIT_MODE
		#include "HexCellData.cginc"
        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
    float3 worldPos;
    float2 visibility;
};

		half _Glossiness;
fixed3 _Specular;
		fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
void vert(inout appdata_full v, out Input data)
{
    UNITY_INITIALIZE_OUTPUT(Input, data);

    float4 cell0 = GetCellData(v, 0);
    float4 cell1 = GetCellData(v, 1);
    float4 cell2 = GetCellData(v, 2);

    data.visibility.x = cell0.x * v.color.x + cell1.x * v.color.y + cell2.x * v.color.z;
    data.visibility.x = lerp(0.25, 1, data.visibility);
    data.visibility.y = cell0.y * v.color.x + cell1.y * v.color.y + cell2.y * v.color.z;
}

void surf(Input IN, inout SurfaceOutputStandardSpecular o)
{
    float2 uv = IN.worldPos.xz + float2(_Time.y, _Time.y);
    float4 noise = tex2D(_MainTex, uv * 0.005);
    
    float blendWave = sin((IN.worldPos.x + IN.worldPos.z) * 0.1 + (noise.y + noise.z) + _Time.y);
    blendWave *= blendWave;
    
    float waves = lerp(noise.z, noise.w, blendWave) + lerp(noise.x, noise.y, blendWave);
    waves = smoothstep(0.75, 2, waves);
    
    fixed4 c = saturate(_Color + waves);
    
float explored = IN.visibility.y;
			o.Albedo = c.rgb * IN.visibility.x;
			o.Specular = _Specular * explored;
			o.Smoothness = _Glossiness;
			o.Occlusion = explored;
			o.Alpha = c.a * explored;
}
		ENDCG
	    }
        FallBack"Diffuse"
}
