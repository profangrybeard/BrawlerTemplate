#include "/ShapeCommon.cginc" 
#include "/EffectCommon.cginc"
#include "/../Utils/ColorUtils.cginc"

#ifndef DISTANCE_FROM_SHAPE
#define DISTANCE_FROM_SHAPE
float DistanceFromShape(float2 p)
{
    return p.x;
}
float DistanceFromScaledShape(float2 p, float sizeOffset)
{
    return DistanceFromShape(p + sizeOffset, mirrorSharpCorner);
}
float2 DistanceInPerimeter(float2 p, float d) //actually distance along perimeter
{
    return p;
}
#endif

float2 DistancesFromOutline(float2 p, float d)
{
    float2 distances = d;

    if (IsRoundEdges())
    {
        distances.x -= 2.0 * (_Outline_Edges_Type_Has.x);
        distances.y -= 2.0 * (_Outline_Edges_Type_Has.y);
    }
    else
    {
        distances.x = DistanceFromScaledShape(p, _Outline_Edges_Type_Has.x);
        distances.y = DistanceFromScaledShape(p, _Outline_Edges_Type_Has.y);
    }

    float mask = GetOutlineStyleMask(DistanceInPerimeter(p, _Outline_Edges_Type_Has.y));
    distances.x = max(mask.x, distances.x);
    distances.y = -max(mask.x,-distances.y);
    
    return distances;
}

float4 DrawShape(float2 p, float sharpness = 0)
{
    // Init
    float4 color = GetColor(p);
    float d = 0;
    bool hasOutline = HasOutline();
    float2 outlineDs = 0;

    // Fill
    d = DistanceFromShape(p); //Overwritten by specific shape

    float fillAlpha = 1 - GetAlphaFromDistance(d, sharpness);
    
    // Inner Effects
    
    // Inner Shadow
    if (_Effect_Use.x == 1)
    {
        // For sharper edges on blur, you can remove _Effect_InnerShadow_Distance_Blur_Spread.z form the equation below
        ApplyInnerShadow(color, DistanceFromScaledShape(p - 2.0 * _Effect_InnerShadow_Distance_Blur_Spread.xy, - _Effect_InnerShadow_Distance_Blur_Spread.w - _Effect_InnerShadow_Distance_Blur_Spread.z), sharpness);
    }
    // Inner Glow
    if (_Effect_Use.z == 1)
    {
        ApplyInnerGlow(color, DistanceFromScaledShape(p, -_Effect_InnerGlow_IsLinear_IsCenter_Choke_Size.w), sharpness);
    }

    // Outline
    if (hasOutline)
    {
        float4 outlineCol = _Outline_FillColor;
        outlineDs = DistancesFromOutline(p, d); //Overwritten by specific shape

        // This guarantee the anti-alias doesn't leave unwanted gaps between the fill and the outline
        if (!IsDashedOutline() && abs(_Outline_Edges_Type_Has.y) < CUTOFF) // if inner edge touches fill
        {
            outlineCol.a *= 1 - GetAlphaFromDistance(outlineDs.x, sharpness);
            color = lerp(outlineCol, color, fillAlpha);
        }
        // This helps to avoid any fill-color mixing at the edges
        else if (abs(_Outline_Edges_Type_Has.x) < CUTOFF) // if outer edge touches fill
        {
            outlineCol.a *= GetAlphaFromDistance(outlineDs.y, sharpness);
            color = composeColor(outlineCol, color);

            color.a *= fillAlpha;
        }
        else
        {
            color.a *= fillAlpha;
            outlineCol.a *= 1 - max(GetAlphaFromDistance(outlineDs.x, sharpness), 1 - GetAlphaFromDistance(outlineDs.y, sharpness));
            color = composeColor(outlineCol, color);
        }
    }
    else
    {
        color.a *= fillAlpha;
    }

    // Outter Effects
    
    // Outer Glow
    if (_Effect_Use.w == 1)
    {
        ApplyOuterGlow(color, _FillColor, d, sharpness);
        if (hasOutline)
            ApplyOuterGlow(color, _Outline_FillColor, max(outlineDs.x, -outlineDs.y), sharpness);
    }
    // Drop Shadow
    if (_Effect_Use.y == 1)
    {
        float effectBlur = _Effect_DropShadow_Distance_Blur_Spread.z;
        float effectSpread = _Effect_DropShadow_Distance_Blur_Spread.w;
        float2 pOffset = 2.0 * _Effect_DropShadow_Distance_Blur_Spread.xy;
        float dropShadowD = DistanceFromScaledShape(p - pOffset, - effectSpread - effectBlur);
        float dropShadowFillD = FLT_MAX;

        if (_FillColor.a > CUTOFF)
            dropShadowFillD = dropShadowD;

        if (hasOutline)
        {
            float dropShadowOutlineDX;
            float dropShadowOutlineDY;
            if (IsRoundEdges())
            {
                dropShadowOutlineDX = dropShadowD - 2.0 * (_Outline_Edges_Type_Has.x - effectSpread);
                dropShadowOutlineDY = dropShadowD - 2.0 * (_Outline_Edges_Type_Has.y - effectSpread);
            }
            else
            {
                dropShadowOutlineDX = DistanceFromScaledShape(p - pOffset, _Outline_Edges_Type_Has.x - 2.0 * effectSpread - effectBlur); 
                dropShadowOutlineDY = DistanceFromScaledShape(p - pOffset, _Outline_Edges_Type_Has.y - 2.0 * effectSpread - effectBlur);
            }
            
            if (abs(_Outline_Edges_Type_Has.y) < CUTOFF) dropShadowOutlineDY += (1-sharpness);
            
            float shadowMask = GetOutlineStyleMask(DistanceInPerimeter(p - pOffset, _Outline_Edges_Type_Has.y - 2.0 * effectSpread - effectBlur));
            dropShadowD = min(dropShadowFillD, max(max(dropShadowOutlineDX, -dropShadowOutlineDY), shadowMask));
        }
        ApplyDropShadow(color, dropShadowD, sharpness);
    }
    
    return color * _Extra_Tint;
}

