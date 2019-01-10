// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/*
	A light weight shader for mobile. It is a standard shader without producedural shader properties like normal, metallic maps etc.
*/

Shader "Custom/Car" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_Lights("Lights", Color) = (1,1,1)
		_LightsColor("Lights Color", Color) = (1,1,1)
		_BrakesColor("Brakes Color", Color) = (1,1,1)

		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_LightsTex("Lights Texture", 2D) = "black" {}
		_BrakesTex("Brakes Texture", 2D) = "black" {}
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque"}
		LOD 200

		CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;
			sampler2D _LightsTex;
			sampler2D _BrakesTex;

			struct Input {
				float2 uv_MainTex;
			};

			fixed4 _Color;
			fixed3 _LightsColor;
			fixed3 _BrakesColor;

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandard o) {
				// Albedo comes from a texture tinted by color
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				fixed4 L = tex2D(_LightsTex, IN.uv_MainTex);
				fixed4 B = tex2D(_BrakesTex, IN.uv_MainTex);
				o.Albedo = c.rgb;
				// Emission comes from a texture tinted by color
				o.Emission = L.rgb * _LightsColor + B.rgb * _BrakesColor;
				// Metallic and smoothness come from slider variables
				//o.Metallic = _Metallic;
				//o.Smoothness = _Glossiness;
				//o.Alpha = c.a;
			}
		ENDCG		
		
	}
	FallBack "Diffuse"
}
