Shader "MagicWindow/CubeSDF"
{
	Properties
	{
      _MainTex("MainTex", 2d) = "white" {}
      _Color("Color", color) = (1,1,1,1)
      _Range("Range", vector) = (1,1,1,1)
	}

	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
      Cull Front
      CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
         };

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
			}

         sampler2D _MainTex;
         float4 _Color;
         float4 _Range;

			half4 frag(v2f i) : COLOR
			{
            float sdf = tex2D(_MainTex, i.uv) - 0.5;
            float v = saturate(sdf * _Range.z + _Range.w);
				return v * _Color;
			}
		ENDCG
		}

      Pass
      {
      Cull Back
      CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag

         struct appdata
         {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
         };

         struct v2f
         {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
         };

         v2f vert(appdata v)
         {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
         }

         sampler2D _MainTex;
         float4 _Color;
         float4 _Range;

         half4 frag(v2f i) : COLOR
         {
            float sdf = tex2D(_MainTex, i.uv) - 0.5;
            float v = saturate(sdf * _Range.x + _Range.y);
            return v * _Color;
         }
      ENDCG
      }
	}
}
