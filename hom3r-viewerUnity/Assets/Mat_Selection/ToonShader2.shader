// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ToonShader2" {
	// ToonShaderのパラメータを宣言
	Properties {
		// 一番暗い色の指定
		_Ambient ("Ambient Color", Color) = (0.1,0.1, 0.4,1)
		// 拡散反射で制御される陰影領域(陰影の明るい部分)
		// Border: 陰影領域の境界を制御
		// BorderBlur: 陰影領域の境界のぼけ具合の制御
		_Diffuse ("Diffuse Color", Color) = (0.3,0.3,1,1)
		_DiffuseBorder ("Diffuse border", Range (0.01, 1)) = 0.2
		_DiffuseBorderBlur ("Diffuse border blur", Range (0.01, 0.2)) = 0.01
		// 鏡面反射で制御される陰影領域(ハイライト部分)
		// Border: 陰影領域の境界を制御
		// BorderBlur: 陰影領域の境界のぼけ具合の制御
		_Specular ("Spec Color", Color) = (1,1,1,1)
		_SpecularBorder ("Specular border", Range (0.01, 1)) = 0.5
		_SpecularBorderBlur ("Specular border blur", Range (0.01, 0.2)) = 0.01
		_Shininess ("Shininess", Range (0.01, 1)) = 0.7
	}
	SubShader {
	pass{
		// Unityのライトオブジェクトを使ったLightMode
		Tags { "LightMode" = "ForwardBase" }
		// Cgプログラムを使用する宣言
		// 頂点処理とピクセル処理を行うことを宣言
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"
		// Cgプログラムで使う変数
		// Propertiesブロックと対応付ける
		float4	_Ambient;
		float4	_Diffuse;
		float 	_DiffuseBorder;
		float 	_DiffuseBorderBlur;
		float4	_Specular;
		float 	_SpecularBorder;
		float 	_SpecularBorderBlur;
		float _Shininess;
		// 頂点からピクセルに転送されるデータ
		struct vertexOutput {
			float4 pos : SV_POSITION; // 座標変換後の位置
			float3 L   : TEXCOORD0; // ライトベクトル
			float3 N   : TEXCOORD1; // 法線ベクトル
			float3 RV   : TEXCOORD2; // 視線の正反射ベクトル
		};
		// 頂点毎の処理
		// pos, L, N, RVのデータを計算する．
		vertexOutput vert(appdata_base v) : POSITION
		{
			vertexOutput output;
			// 座標変換後の位置
			output.pos = UnityObjectToClipPos (v.vertex);
			// 法線ベクトル
			float3 N = v.normal;
			output.N = N;
			// ライトベクトル
			output.L = ObjSpaceLightDir(v.vertex);
			// 視線の正反射ベクトル
			float3 V = ObjSpaceViewDir(v.vertex);
			output.RV = reflect(-V, N);
			return output;
		}
		// ピクセル毎の処理
		// 1. 通常の物理的なライティング計算を行う
		// 2. 照明結果を基に減色処理を行う
		float4 frag(vertexOutput input) : COLOR
		{
			// 頂点処理で計算したベクトルデータを正規化して取り出す
			float3 L = normalize( input.L );
			float3 N = normalize( input.N );
			float3 RV = normalize( input.RV );
			// 拡散反射の度合I_dを計算
			float LdN = clamp( dot(L, N), 0, 1 );
			float I_d = LdN;
			// 鏡面反射の度合I_sを計算
			float LdRV = clamp( dot(L, RV), 0, 1 );
			float shininess = pow(500.0, _Shininess);
			float I_s = pow( LdRV, shininess);
			// 得られたI_d, I_sを基に減色処理を行う
			// 一番暗い色からスタートする
			float4 c_a = _Ambient;
			// 拡散反射の度合I_dを基に閾値処理を行い，
			// I_d > _DiffuseBorderであれば，_Diffuseの色で塗る
			// _DiffuseBorderBlurにより，境界部分のぼけ具合を制御している
			float t_d = smoothstep( _DiffuseBorder - _DiffuseBorderBlur, _DiffuseBorder + _DiffuseBorderBlur, I_d);
			float4 c_d = lerp(c_a, _Diffuse, t_d);
			// 拡散反射と同様に，鏡面反射についても閾値処理を行う
			float t_s = smoothstep(_SpecularBorder - _SpecularBorderBlur, _SpecularBorder + _SpecularBorderBlur, I_s);
			float4 c = lerp(c_d, _Specular, t_s);
			return c;
		}
		ENDCG
		}
	}
	FallBack "Diffuse"
}
