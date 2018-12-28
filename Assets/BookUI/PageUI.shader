Shader "UI/PageDeform"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    
    	_Page("Current Page", Float) = 0
    	_Tilt("Tilt when Flip", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        //Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float clipvalue : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            
            float _Page;            
			float4x4 _Canvas2Local;
			float4x4 _Local2Canvas;
            float _Tilt;
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				float4 vertex = mul(_Canvas2Local, v.vertex);
                OUT.clipvalue = 1;
                float t = frac(_Page);
				float t1 = t + vertex.y * _Tilt * 2 * t*(1 - t); //for turning pages
				float t2 = t + vertex.y * _Tilt * 2 * t*(1 - t) / (1 + t1 * (1 - t1)); //for other pages
				/*
                if(vertex.x-0.5 < floor(_Page)){ //is left side
            		vertex.x -= floor(_Page);
					OUT.clipvalue = -vertex.x * 2 + (1 - 2 * t2);
            		if(vertex.x > 0) { //turning page
						OUT.clipvalue += vertex.x * 10;
						vertex.y *= 1 + 2 * t1 * (1 - t1)*vertex.x;
						vertex.x *= 1 - 2 * t1;
            		}
            	} else {
            		vertex.x -= floor(_Page) + 1;
					OUT.clipvalue = vertex.x * 2 - (1 - 2 * t2);
            		if(vertex.x < 0){ //turning page
						OUT.clipvalue += -vertex.x * 10;
						vertex.y *= 1 - 2 * t1 * (1 - t1)*vertex.x;
						vertex.x *= -1 + 2 * t1;
            		}
            	}
				*/
				/*avoid if-----------------------------------------*/
				float LorR = step(vertex.x - 0.5, floor(_Page)) ? -1 : 1;
				vertex.x -= floor(_Page) + 0.5 + LorR * 0.5;
				float isTurning = step(0, -vertex.x * LorR);
				OUT.clipvalue = (vertex.x * 2 - (1 - 2 * t2))*LorR;
				OUT.clipvalue += isTurning * LorR * (-vertex.x * 10);
				vertex.y *= 1 - isTurning * LorR * (2 * t1 * (1 - t1)*vertex.x);
				vertex.x *= lerp(1, LorR * (-1 + 2 * t1), isTurning);
				/*-------------------------------------------------*/
				//collapse outer pages
				vertex.x = sign(vertex.x)*min(abs(vertex.x), 0.5);
				OUT.worldPosition = mul(_Local2Canvas, vertex);
            	
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
            	clip(IN.clipvalue);
            
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

				color.rgb *= min(IN.clipvalue*2+0.5, 1);

                return color;
            }
        ENDCG
        }
    }
}
