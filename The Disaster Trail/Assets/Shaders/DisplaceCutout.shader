﻿Shader "Custom/DisplaceCutout"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Cutoff("Alpha cutoff", Range(0.15,0.85)) = 0.4
		_MipScale("Mip Level Alpha Scale", Range(0,1)) = 0.25
		_HeightMap("Height Map", 2D) = "black" {}
		_Amount("Displace Amount", Range(-1,1)) = 0.5
	}
		SubShader
		{
			Pass
			{
				Tags { "RenderQueue" = "AlphaTest" "RenderType" = "TransparentCutout" "LightMode" = "ForwardBase"}
				AlphaToMask On

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "Lighting.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					half3 normal : NORMAL;
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					half3 worldNormal : NORMAL;
				};

				sampler2D _MainTex;
				sampler2D _HeightMap;
				float4 _MainTex_ST;
				float4 _MainTex_TexelSize;

				float _Amount;
				fixed _Cutoff;
				half _MipScale;

				float CalcMipLevel(float2 texture_coord)
				{
					float2 dx = ddx(texture_coord);
					float2 dy = ddy(texture_coord);
					float delta_max_sqr = max(dot(dx, dx), dot(dy, dy));

					return max(0.0, 0.5 * log2(delta_max_sqr));
				}

				v2f vert(appdata v)
				{
					// Transforms vertices based off of the height map
					float d = tex2Dlod(_HeightMap, float4(v.uv.xy, 0, 0)).r;
					v.vertex.xyz += v.normal * d * _Amount;

					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.worldNormal = UnityObjectToWorldNormal(v.normal);
					return o;
				}

				fixed4 frag(v2f i, fixed facing : VFACE) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.uv);
					// rescale alpha by mip level (if not using preserved coverage mip maps)
					col.a *= 1 + max(0, CalcMipLevel(i.uv * _MainTex_TexelSize.zw)) * _MipScale;
					// rescale alpha by partial derivative
					col.a = (col.a - _Cutoff) / max(fwidth(col.a), 0.0001) + 0.5;

					half3 worldNormal = normalize(i.worldNormal * facing);

					fixed ndotl = saturate(dot(worldNormal, normalize(_WorldSpaceLightPos0.xyz)));
					fixed3 lighting = ndotl * _LightColor0;
					lighting += ShadeSH9(half4(worldNormal, 1.0));

					col.rgb *= lighting;

					return col;
				}
				ENDCG
			}
		}
}