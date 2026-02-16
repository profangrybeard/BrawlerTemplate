// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader"UIAB/More"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        
        // VISUAL
        [PerRendererData] _ResultTexture_Dimension("Texture Dimension", Vector) = (1,1,0,0)
        [PerRendererData] _UseComputeShader("Use Compute Shader", Int) = 0
        [PerRendererData] _CornerType("Corner Type", Int) = 0
        // FILL COLOR
        [PerRendererData] _FillColor("Fill Color", Color) = (1,1,1,1)
        [PerRendererData] _FillStyle("Fill Style", Int) = 0
        [PerRendererData] _FillColorEnd("Fill Color End", Color) = (1,1,1,1)
        [PerRendererData] _FillCoordinates("Fill Gradient Coordinates", Vector) = (0,0,1,1)
        [PerRendererData] _FillColorMid("Fill Color Mid", Color) = (1,1,1,1)
        [PerRendererData] _FillMidCoordinates("Fill Gradient Mid Coordinates", Float) = 0.5
        // OUTLINE
        [PerRendererData] _Outline_FillColor("Outline Color", Color) = (1,1,1,1)
        [PerRendererData] _Outline_Edges_Type_Has("Outline Edges/Type/Has", Float) = (0,5,1,0)
        [PerRendererData] _Outline_IsDashed_DashRatio_GapRatio_DashOffset("Outline IsDash/Ratio/Offset", Float) = (0,0.1,0,0)
        // MORE
        [PerRendererData] _More_Type_Data("More Type/Data", Vector) = (0,0,0,0)
        // EFFECTS
        [PerRendererData] _Effect_Use("Use Effects", Vector) = (0,0,0,0)
            // INNER SHADOW
        [PerRendererData] _Effect_InnerShadow_Color("Inner Shadow Color", Color) = (0,0,0,0.5)
        [PerRendererData] _Effect_InnerShadow_Distance_Blur_Spread("Inner Shadow Distance/Blur/Spread", Vector) = (10,10,0,10)
            // INNER GLOW
        [PerRendererData] _Effect_InnerGlow_Color("Inner Glow Color", Color) = (1,1,1,0.5)
        [PerRendererData] _Effect_InnerGlow_IsLinear_IsCenter_Choke_Size("Inner Glow IsLinear/IsCenter/Choke/Size", Vector) = (1,0,0.1,10)
            // OUTER GLOW
        [PerRendererData] _Effect_OuterGlow_Color("Outer Glow Color", Color) = (1,1,1,0.5)
        [PerRendererData] _Effect_OuterGlow_IsLinear_Choke_Size("Outer Glow IsLinear/Choke/Size", Vector) = (1, 0, 10, 0)
            // DROP SHADOW
        [PerRendererData] _Effect_DropShadow_Color("Drop Shadow Color", Color) = (0,0,0,0.5)
        [PerRendererData] _Effect_DropShadow_Distance_Blur_Spread("Drop Shadow Distance/Blur/Spread", Vector) = (10,10,0,10)
         // EXTRA SETTINGS
        [PerRendererData] _Extra_Tint("Extra Settings Tint", Vector) = (1,1,1,1)       

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _ColorMask("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Stencil
            {
                Ref[_Stencil]
                Comp[_StencilComp]
                Pass[_StencilOp]
                ReadMask[_StencilReadMask]
                WriteMask[_StencilWriteMask]
            }

            Cull Off
            Lighting Off
            ZWrite Off
            ZTest[unity_GUIZTestMode]
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask[_ColorMask]

            Pass
            {
                Name "Default"
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0

                #define USE_PARTIAL_DERIVATIVE
                #define USE_SRGB_GRADIENT

                #include "UnityCG.cginc"
                #include "UnityUI.cginc"
                #include "/../../CGIncludes/Utils/PointUtils.cginc" 
                #include "/../../CGIncludes/Shapes/More.cginc" 
                #include "/../../CGIncludes/Common/DrawerCommon.cginc" 

                #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
                #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
                    fixed4 color : COLOR;
                    float2 texcoord  : TEXCOORD0;
                    float4 worldPosition : TEXCOORD1;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                sampler2D _MainTex;
                fixed4 _Color;
                fixed4 _TextureSampleAdd;
                float4 _ClipRect;
                float4 _MainTex_ST;
                float4 _ResultTexture_Dimension;
                uniform int _UseComputeShader;

                v2f vert(appdata_t v)
                {
                    v2f OUT;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                    OUT.worldPosition = v.vertex;
                    OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                    OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                    OUT.color = v.color * _Color;
                    return OUT;
                }

                fixed4 frag(v2f IN) : SV_Target
                {
                    fixed4 color = IN.color;
                    if (_UseComputeShader)
                    {
                        color *= (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);
                    }
                    else
                    {
                        float2 p = uvToCenteredObjectiveSpace(IN.texcoord.xy, _ResultTexture_Dimension.xy); 
                        color *= DrawShape(p);
                    }                    

                    #ifdef UNITY_UI_CLIP_RECT
                    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                    #endif

                    #ifdef UNITY_UI_ALPHACLIP
                    clip(color.a - 0.001);
                    #endif

                    return color;
                }
            ENDCG
            }
        }
}