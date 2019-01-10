/*
	A light weight shader for mobile. It is a standard shader without producedural shader properties like normal, metallic maps etc.
*/

Shader "Custom/FloodWater" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_EmissionColor("EmissionColor", Color) = (1,1,1)
		_HeightMap("Height Map", 2D) = "black" {}
		_Opacity("Opacity", Range(0,1)) = 1
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_EmissionTex ("Emission Texture", 2D) = "black" {}
		_Amount("Displace Amount", Range(-2,2)) = 0.1
		_WaterScroll("Water Scroll Amount", Range(-0.5,0.5)) = 0.1
	}
	SubShader {
		Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
		LOD 1000

		Cull Off


		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf BlinnPhong alpha:fade vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _EmissionTex;
		sampler2D _HeightMap;
		sampler2D _AlphaTex;
		float _Opacity;
		float _Amount;
		float _WaterScroll;

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

		void vert(inout appdata_full v) {
			// Scrolls the uvs of the mesh
			v.texcoord.x += _Time[0] * _WaterScroll;
			// Transforms vertices based off of the height map
			float d = tex2Dlod(_HeightMap, float4(v.texcoord.xy, 0, 0)).r;
			v.vertex.xyz += v.normal * d * _Amount/60;
		}

		void surf (Input IN, inout SurfaceOutput o) {
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
			if (IN.facing < 0.5)
			{
				o.Normal *= -1.0;
			}
		}
		ENDCG
	}
	FallBack "Diffuse"
}
