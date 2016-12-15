Shader "Sprites/Bumped Diffuse with Shadows"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
	}

	SubShader
	{
		Blend OneMinusDstColor One
		Cull Off
		Tags{"Queue"="Transparent"}
		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert_img
			#pragma fragment frag

			uniform sampler2D _MainTex;

			float4 frag(v2f_img v) : COLOR
			{
				return tex2D(_MainTex, v.uv);
			}

			ENDCG
		}
	}
}
