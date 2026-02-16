
#ifndef USE_EFFECTUTILS
#define USE_EFFECTUTILS

#include "/../Utils/DebugUtils.cginc"
#include "/../Utils/ColorUtils.cginc"

static const float EXP_DECAY_RATE = 6; 

// (innerShadow, dropShadow, innerGlow, outerGlow)
uniform float4 _Effect_Use;

uniform float4 _Effect_InnerGlow_Color;
uniform float4 _Effect_InnerGlow_IsLinear_IsCenter_Choke_Size;

void ApplyInnerGlow(inout float4 color, in float d, in float sharpness)
{
    float exponentialDecrease = _Effect_InnerGlow_IsLinear_IsCenter_Choke_Size.x;
    bool isCenter = _Effect_InnerGlow_IsLinear_IsCenter_Choke_Size.y == 1;
    float choke = _Effect_InnerGlow_IsLinear_IsCenter_Choke_Size.z;
    float size = 2 * _Effect_InnerGlow_IsLinear_IsCenter_Choke_Size.w;

    if (size < CUTOFF) return;
    
    float effectAlpha = saturate(d / ((1 - choke) * size)); 

    effectAlpha = pow(effectAlpha, exponentialDecrease);

    if (isCenter)
        effectAlpha = 1 - effectAlpha;

    effectAlpha = effectAlpha * _Effect_InnerGlow_Color.a;

    color.rgb = composeColor(float4(_Effect_InnerGlow_Color.rgb, effectAlpha), color).rgb;
    color.a = max(color.a, effectAlpha);
    
}

uniform float4 _Effect_OuterGlow_Color;
uniform float4 _Effect_OuterGlow_IsLinear_Choke_Size;

void ApplyOuterGlow(inout float4 color, in float4 fillColor, in float d, in float sharpness)
{
    float exponentialDecrease = _Effect_OuterGlow_IsLinear_Choke_Size.x;
    float choke = _Effect_OuterGlow_IsLinear_Choke_Size.y;
    float size = 2 * _Effect_OuterGlow_IsLinear_Choke_Size.z;
    float effectAlpha = 0;

    if (size < CUTOFF) return;

    effectAlpha = 1 - saturate((d - (choke) * size) / ((1 - choke) * size)); 

    effectAlpha = pow(saturate(effectAlpha), exponentialDecrease);
    effectAlpha *= _Effect_OuterGlow_Color.a;

    if (fillColor.a == 1) 
        color = composeColor(color, float4(_Effect_OuterGlow_Color.xyz, effectAlpha));
    else 
        color = composeColor(color, float4(_Effect_OuterGlow_Color.xyz, effectAlpha * GetAlphaFromDistance(d, sharpness)));
}

uniform float4 _Effect_InnerShadow_Color;
uniform float4 _Effect_InnerShadow_Distance_Blur_Spread;

void ApplyInnerShadow(inout float4 color, in float d, in float sharpness)
{
    if (color.a < CUTOFF) return;
    
    //float blur = 2 * max(_Effect_InnerShadow_Distance_Blur_Spread.z, 1-sharpness);
    float blur = 2 * _Effect_InnerShadow_Distance_Blur_Spread.z;
    float spread = 0.5 * _Effect_InnerShadow_Distance_Blur_Spread.w;
    float effectAlpha = 0;

    if (blur < CUTOFF)
        effectAlpha = GetAlphaFromDistance(d + spread, sharpness);
    else
        effectAlpha = GetCenteredSmoothedAlpha(d + spread - blur, blur);
    
    effectAlpha *= _Effect_InnerShadow_Color.a;

    color.rgb = composeColor(float4(_Effect_InnerShadow_Color.rgb, effectAlpha), color).rgb;
}

uniform float4 _Effect_DropShadow_Color;
uniform float4 _Effect_DropShadow_Distance_Blur_Spread;

void ApplyDropShadow(inout float4 color, in float d, in float sharpness)
{
    float blur = 2.0 * _Effect_DropShadow_Distance_Blur_Spread.z;//2 * max(, 1-sharpness);
    float spread = 0.5 * _Effect_DropShadow_Distance_Blur_Spread.w;
    float effectAlpha = 0;

    if (blur < CUTOFF)
        effectAlpha = 1 - GetAlphaFromDistance(d + spread, sharpness);
    else
        effectAlpha = 1 - GetCenteredSmoothedAlpha(d + spread - blur, blur);
    
    effectAlpha *= _Effect_DropShadow_Color.a;

    color = composeColor(color, float4(_Effect_DropShadow_Color.rgb, effectAlpha));
}
#endif