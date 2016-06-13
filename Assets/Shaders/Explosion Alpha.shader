//all glory to http://stevencraeynest.wordpress.com/2013/03/29/easy-volumetric-explosion-in-unity3d/

Shader "Custom/Explosion Alpha"
{
    Properties 
    {
    	_ColorTint ("Tint", Color) = (1.0, 0.6, 0.6, 1.0)
        _RampTex ("Color Ramp", 2D) = "white" {}
        _DispTex ("Displacement Texture", 2D) = "gray" {}
        _Displacement ("Displacement", Range(0, 1.0)) = 0.1
        _ChannelFactor ("ChannelFactor (r,g,b)", Vector) = (1,0,0)
        _Range ("Range (min,max)", Vector) = (0,0.5,0)
//        _ClipRange ("ClipRange [0,1]", float) = 0.8
        _ClipRange ("Clip", Range(0, 1.0)) = 0
    }
 
    SubShader 
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        LOD 300

//        ZWrite On
//		Blend SrcAlpha OneMinusSrcAlpha

//	     Pass
//	     {
//	         Lighting Off
//	         SetTexture [_DispTex] { combine texture } 
//	     }
 
        CGPROGRAM
        #pragma surface surf Lambert vertex:vert nolightmap	// finalcolor:mycolor
        #pragma target 3.0
        #pragma glsl
 
        sampler2D _DispTex;
        float _Displacement;
        float3 _ChannelFactor;
        float2 _Range;
        float _ClipRange;


        // Input data
        struct Input 
        {
            float2 uv_DispTex;
        };


        // Vertex shader
        void vert (inout appdata_full v)
        {
            float3 dcolor = tex2Dlod (_DispTex, float4(v.texcoord.xy, 0, 0));
            float d = (dcolor.r * _ChannelFactor.r + dcolor.g * _ChannelFactor.g + dcolor.b * _ChannelFactor.b);
            v.vertex.xyz += v.normal * d * _Displacement;
        }


        // Color tint shader
//		float4 _ColorTint;
//
//        void mycolor (Input IN, SurfaceOutput o, inout fixed4 color)
//      	{
//          color *= _ColorTint;
//      	}


      	// Surface shader
        sampler2D _RampTex;
 
        void surf (Input IN, inout SurfaceOutput o)
        {
            float3 dcolor = tex2D (_DispTex, IN.uv_DispTex);
            float d = (dcolor.r * _ChannelFactor.r + dcolor.g * _ChannelFactor.g + dcolor.b * _ChannelFactor.b) * (_Range.y - _Range.x) + _Range.x;
            clip (d - _ClipRange);
            half4 c = tex2D (_RampTex, float2(d, 0.5));
            o.Albedo = c.rgb;
//            o.Alpha = _ColorTint.a;
            o.Emission = c.rgb * c.a;
        }

        ENDCG
    }
    FallBack "Diffuse"
}