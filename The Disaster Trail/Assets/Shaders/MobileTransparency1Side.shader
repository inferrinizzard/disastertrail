/*
	A light weight shader for mobile. It is a standard shader without producedural shader properties like normal, metallic maps etc.
*/

Shader "Custom/MobileTransparency1Side" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_EmissionColor ("EmissionColor", Color) = (1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Opacity("Opacity", Range(0,1)) = 1
		_EmissionTex ("Emission Texture", 2D) = "black" {}
	}
	SubShader {
		Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
		LOD 1000

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _EmissionTex;
		sampler2D _AlphaTex;
		float _Opacity;

		struct Input {
			float2 uv_MainTex;
			fixed facing : VFACE;
		};

		fixed4 _Color;
		fixed3 _EmissionColor;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 e = tex2D(_EmissionTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			// Emission comes from a texture tinted by color
			o.Emission = e.rgb * _EmissionColor;
			// Metallic and smoothness come from slider variables
			//o.Metallic = _Metallic;
			//o.Smoothness = _Glossiness;
			o.Alpha = c.a * _Opacity;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
