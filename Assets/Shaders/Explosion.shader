Shader "Explosion" {
	Properties {
		_RampTex ("Ramp", Rect) = "white"
		_MainTex ("Noise", 2D) = "grey"
		_Heat ("Heat", Float) = 1
		_Radius ("Radius", Float) = 1
		_Frequency ("Noise Frequency", Float) = 1
		_ScrollSpeed ("Noise Scroll Speed", Float) = 1
		_Alpha ("Alpha", Float) = 1
	}
	SubShader {
		Tags {"Queue"="Transparent+100" "RenderType"="Transparent" "IgnoreProjector"="True"}
		LOD 1000
		
		//render a smaller object inside, for the purpose of early-out Z testing
		Pass {
			ColorMask 0
			ZWrite On
			CGPROGRAM
			#pragma target 3.0
			#pragma glsl
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct v2f{
				float4 vertex : SV_POSITION;
			};
			
			v2f vert (appdata_base v) {
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex - float4(v.normal * .25, 0));
				return o;
			}
			
			half4 frag (v2f i) : COLOR {
				return half4(1, 1, 1, 1);
			}
			
			ENDCG
		}
		
		//Render the actual fireball
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma glsl
			#pragma exclude_renderers d3d11_9x	
			#include "UnityCG.cginc"

			#pragma multi_compile QUALITY_HIGH QUALITY_LOW QUALITY_MED
			#pragma multi_compile OCTAVES_4 OCTAVES_1 OCTAVES_2 OCTAVES_3 OCTAVES_5
			#pragma multi_compile SCATTERING_ON SCATTERING_OFF
	
#if QUALITY_LOW
			#define THRESHOLD .15
			#define PRIMARY 9
			#define SECONDARY 5
			#define HEATSTEP .2
#elif QUALITY_MED
			#define THRESHOLD .05
			#define PRIMARY 15
			#define SECONDARY 8
			#define HEATSTEP .15
#elif QUALITY_HIGH
			#define THRESHOLD .02
			#define PRIMARY 25
			#define SECONDARY 10
			#define HEATSTEP .1
#endif
			sampler2D _RampTex;
			sampler2D _MainTex;
			float _Heat;
			float _Radius;
			float _Frequency;
			float _ScrollSpeed;
			float _Alpha;
	
			struct v2f {
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float3 viewVec : TEXCOORD1;
				float4 sphere : TEXCOORD2;
			};
			
			v2f vert (appdata_base v) {
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.worldPos = mul(_Object2World, v.vertex).xyz;
				o.viewVec = WorldSpaceViewDir(v.vertex);
				o.sphere.xyz = mul(_Object2World, float4(0, 0, 0, 1)).xyz;
				return o;
			}
			
			float noise(float3 p) {
				float f = frac(p.y);
				float i = floor(p.y);
				float2 rg = tex2Dlod(_MainTex, float4(p.xz+float2(37, 13)*i, 0, 0)/64).yx;
				return lerp(rg.x, rg.y, f);
			}
			
			float fbm (float3 p) {
				p *= _Frequency;
				float v = 0;
				float4 offset = _Time * _ScrollSpeed;
				v += noise(p + offset.y);
#if OCTAVES_2 | OCTAVES_3 | OCTAVES_4 | OCTAVES_5
				p *= 2;
				v += noise(p + offset.z)/2; p *= 2;
#endif
#if OCTAVES_3 | OCTAVES_4 | OCTAVES_5
				v += noise(p + offset.z)/4; p *= 2;
#endif
#if OCTAVES_4 | OCTAVES_5
				v += noise(p + offset.w)/8; p *= 2;
#endif
#if OCTAVES_5
				v += noise(p + offset.w)/16; p *= 2;
#endif
				return v;
			}
			
			float distf (float4 sphere, float3 p) {
				return distance(p, sphere.xyz) - _Radius - fbm(p);
			}
			
//			float3 norm(float3 p) {
//				float3 delta;
//				#define nabla .08
//				float d = distf(p);
//				delta.x = d - distf(p + float3(nabla, 0, 0));
//				delta.y = d - distf(p + float3(0, nabla, 0));
//				delta.z = d - distf(p + float3(0, 0, nabla));
//				return normalize(delta);
//			}
			
			float4 march (float4 sphere, float3 p, float3 v) {
				float dist;
				for (int i = 0; i < PRIMARY; ++i) {
					dist = distf(sphere, p);
					if (dist < THRESHOLD) return float4(p, 0);
					p -= v * (dist + .02);
				}
				return float4(-100, -100, -100, -100);
			}
			
			float2 heat (float4 sphere, float3 p, float3 d) {
				float heat = 0;
				float dens = 0;
				float fac = .5;
				d *= HEATSTEP;
				for (int i = 0; i < SECONDARY; ++i) {
					float dis = distf(sphere, p);
					if (dis <= THRESHOLD) {
						heat += pow((_Radius - distance(p, sphere.xyz) + 2.5)*fac*_Heat, 3);
						fac *= .25;
						dens += HEATSTEP * 2;
						p -= d;
					} else {
						p -= d*3;
					}
					
				}
				return float2(heat, dens);
			}
	
			half4 frag (v2f i) : COLOR {
				float4 m = march(i.sphere, i.worldPos, normalize(i.viewVec));
				
#if SCATTERING_OFF
				float heatfac = smoothstep(_Radius + .5, _Radius + 1.5, distance(m.xyz, i.sphere.xyz)) / _Heat;
				half4 col = tex2Dlod(_RampTex, float4(1 - heatfac, 0, 0, 0));
				col.w = saturate(_Alpha);
#elif SCATTERING_ON
				float2 hd = heat(i.sphere, m.xyz, normalize(i.viewVec));
				half4 col = tex2Dlod(_RampTex, float4(hd.x, 0, 0, 0));
				col.w = saturate(saturate(hd.y) * _Alpha);			
#endif
				clip(m.w);
				return col;
			}
			ENDCG
		}
	}
	CustomEditor "CJPyroclasticMaterialEditor"
}
