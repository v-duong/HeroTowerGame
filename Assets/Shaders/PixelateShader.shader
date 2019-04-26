// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Pixelate/PixelateShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_Palette("Palette1", Color) = (1, 1, 1, 1)
			_Palette1("Palette2", Color) = (1, 1, 1, 1)
			_Palette2("Palette3", Color) = (1, 1, 1, 1)
			_Palette3("Palette4", Color) = (1, 1, 1, 1)
			_Palette4("Palette5", Color) = (1, 1, 1, 1)
			_Palette5("Palette6", Color) = (1, 1, 1, 1)
			_Palette6("Palette7", Color) = (1, 1, 1, 1)
			_Palette7("Palette8", Color) = (1, 1, 1, 1)
		_PaletteSize("Palette Size", int) = 16
	}
		SubShader
		{
			Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
			LOD 100
			Lighting Off
			Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
					float texcoordBlend : TEXCOORD1;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
					float4 color : COLOR;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _Color;

				int _PaletteSize;
				float3 	_Palette;
					float3 _Palette1;
					float3 _Palette2;
					float3 _Palette3;
					float3 _Palette4;
					float3 _Palette5;
					float3 _Palette6;
					float3 _Palette7;
				int indexMatrix4x4[16] = { 0, 8, 2, 10,
					12, 4, 14, 6,
					3, 11, 1, 9,
					15, 7, 13, 5 };

				const float lightnessSteps = 4.0;
				float Epsilon = 1e-10;

				float3 RGBtoHCV(in float3 RGB)
				{
					// Based on work by Sam Hocevar and Emil Persson
					float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
					float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
					float C = Q.x - min(Q.w, Q.y);
					float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
					return float3(H, C, Q.x);
				}

				float indexValue(float2 uv) {
					int x = int(uv.x % 4);
					int y = int(uv.y % 4);
					return indexMatrix4x4[(x + y * 4)] / 16.0;
				}

				float hueDistance(float h1, float h2) {
					float diff = abs((h1 - h2));
					return min(abs((1.0 - diff)), diff);
				}

				float3 RGBtoHSL(in float3 RGB)
				{
					float3 HCV = RGBtoHCV(RGB);
					float L = HCV.z - HCV.y * 0.5;
					float S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
					return float3(HCV.x, S, L);
				}

				void closestColors(float hue, inout float3 ret[2]) {
					float3 closest = float3(-2, 0, 0);
					float3 secondClosest = float3(-2, 0, 0);
					float3 temp;
					float3 _PalleteCom[8] = { RGBtoHSL(_Palette), RGBtoHSL(_Palette1) ,RGBtoHSL(_Palette2) , RGBtoHSL(_Palette3) ,
						RGBtoHSL(_Palette4) ,RGBtoHSL(_Palette5),RGBtoHSL(_Palette6),RGBtoHSL(_Palette7) };
					for (int i = 0; i < _PaletteSize; ++i) {
						temp = _PalleteCom[i];
						float tempDistance = hueDistance(temp.x, hue);
						if (tempDistance < hueDistance(closest.x, hue)) {
							secondClosest = closest;
							closest = temp;
						}
						else {
							if (tempDistance < hueDistance(secondClosest.x, hue)) {
								secondClosest = temp;
							}
						}
					}
					ret[0] = closest;
					ret[1] = secondClosest;
				}

				float lightnessStep(float l) {
					/* Quantize the lightness to one of `lightnessSteps` values */
					return floor((0.5 + l * lightnessSteps)) / lightnessSteps;
				}

				float3 HUEtoRGB(in float H)
				{
					float R = abs(H * 6 - 3) - 1;
					float G = 2 - abs(H * 6 - 2);
					float B = 2 - abs(H * 6 - 4);
					return saturate(float3(R, G, B));
				}

				float3 HSLtoRGB(in float3 HSL)
				{
					float3 RGB = HUEtoRGB(HSL.x);
					float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
					return (RGB - 0.5) * C + HSL.z;
				}

				float3 dither(float3 color, float2 uv) {
					float3 hsl = RGBtoHSL(color);

					float3 cs[2];
					closestColors(hsl.x, cs);
					float3 c1 = cs[0];
					float3 c2 = cs[1];
					float d = indexValue(uv);
					float hueDiff = hueDistance(hsl.x, c1.x) / hueDistance(c2.x, c1.x);

					float l1 = lightnessStep(max((hsl.z - 0.125), 0.0));
					float l2 = lightnessStep(min((hsl.z + 0.124), 1.0));
					float lightnessDiff = (hsl.z - l1) / (l2 - l1);

					float3 resultColor = (hueDiff < d) ? c1 : c2;
					resultColor.z = (lightnessDiff < d) ? l1 : l2;
					return HSLtoRGB(resultColor);
				}
				float3 dither2(float3 color, float2 uv) {
					float3 hsl = RGBtoHSL(color);
					float3 cs[2];
					closestColors(hsl.x, cs);
					float3 closestColor = cs[0];
					float3 secondClosestColor = cs[1];
					float d = indexValue(uv);
					float hueDiff = hueDistance(hsl.x, closestColor.x) /
						hueDistance(secondClosestColor.x, closestColor.x);
					return HSLtoRGB(hueDiff < d ? closestColor : secondClosestColor);
				}

				v2f vert(appdata v)
				{
					v2f o;
					//o.vertex = UnityObjectToClipPos(v.vertex);
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					//o.color = float4(dither2(v.color * _Color, v.uv),1);
					o.color = v.color * _Color;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					// sample the texture
					float x = 1.0f / 128;
					float y = 1.0f / 128;

					//fixed2 uv = fixed2((int)(i.uv.x / x) * x, (int)(i.uv.y / y) * y);
					float2 uv = i.uv;
					float4 col = tex2D(_MainTex, uv);

					float4 color = float4(dither2(col, i.uv), 1);
					return col * color;
				}

				ENDCG
			}
		}
}