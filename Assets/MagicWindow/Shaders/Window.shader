Shader "MagicWindow/Window"
{
	Properties
	{
      _StencilMask("Stencil", Int) = 0

      [KeywordEnum(Always, Never, Less, Equal, LEqual, Greater, NotEqual, GEqual, Always)]
      _StencilComp("StencilComp", Int) = 0 // Always

      [MaterialToggle] _ZWrite("ZWrite", Int) = 0
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry-2"}
		ColorMask 0
		ZWrite [_ZWrite]
		Stencil
		{
			Ref [_StencilMask]
			Comp [_StencilComp]
			Pass replace
         ZFail keep
		}

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				return half4(1,0,0,1);
			}
		ENDCG
		}
	}
}
