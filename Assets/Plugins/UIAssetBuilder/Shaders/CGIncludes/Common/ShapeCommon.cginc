// SHAPE COMMON

// HasRoundCorners(corners.xyzw)
// GetOutlineCorners(corners.xyzw, thickness)
// #include "/../../CGIncludes/Common/ShapeCommon.cginc"  

#ifndef USE_SHAPECOMMON
#define USE_SHAPECOMMON

#include "/../Utils/ColorUtils.cginc"
#include "/../Utils/Constants.cginc"
#include "/../SDF/Primitives.cginc"

// Fill
uniform float4 _FillColor;
// Fill Gradient
uniform float _FillStyle;
uniform float4 _FillColorEnd;
uniform float4 _FillCoordinates;
uniform float4 _FillColorMid;
uniform float _FillMidCoordinates;

// Outline
uniform float4 _Outline_FillColor;
uniform float4 _Outline_Edges_Type_Has; //x:inner edge, y:outer edge, z: rounded(0)|sharp(1), w: has?
//uniform float4 _Outline_Style; //x:inner edge, y:outer edge, z: rounded(0)|sharp(1), w: has?
uniform float4 _Outline_IsDashed_DashRatio_GapRatio_DashOffset;

// Extra Settings
uniform float4 _Extra_Tint;

float4 GetColor(in float2 p)
{
    if (_FillStyle < 1.0) return _FillColor;
    else
    {
        float2 gradientVector = _FillCoordinates.zw - _FillCoordinates.xy;
        p = 0.5 * p - _FillCoordinates.xy;
        float colorRatio = _FillStyle == 1 ||  _FillStyle == 3 // Linear
            ? dot(gradientVector, p) / dot(gradientVector, gradientVector)
            : length(p) / length(gradientVector);
        
        colorRatio = clamp(colorRatio, 0.0, 1.0);
        float4 color1 = _FillColor;
        float4 color2 = _FillColorEnd;

        if (_FillStyle > 2) // 3-colors
        {
            if (colorRatio < _FillMidCoordinates)
            {
                color2 = _FillColorMid;
                colorRatio /= _FillMidCoordinates;
            }
            else
            {
                color1 = _FillColorMid;
                colorRatio = (colorRatio - _FillMidCoordinates) / (1.0 - _FillMidCoordinates);
            }
        }

#ifdef USE_SRGB_GRADIENT
        return LerpSRGB(color1, color2, colorRatio);
#endif 
        return lerp(color1, color2, colorRatio);
    }
}

bool IsRoundEdges()
{
	return _Outline_Edges_Type_Has.z == 0;
}

bool IsDashedOutline()
{
    return _Outline_IsDashed_DashRatio_GapRatio_DashOffset.x == 1; 
}

bool HasRoundCorners(in float4 corners)
{
    return corners.x > CUTOFF || corners.y > CUTOFF || corners.z > CUTOFF || corners.w > CUTOFF;
}

bool IsValidOutline(in float4 color, in float thickness)
{
    return color.a > CUTOFF && thickness > CUTOFF;
}

float GetOutlineStyleMask(in float2 pd)
{
    if (IsDashedOutline())
    {
        // Dash Length -> given by user, relative to shape's edge
        // CPU calculates ratio (dashLength / perimeter) and send it
        float dashLength = _Outline_IsDashed_DashRatio_GapRatio_DashOffset.y * pd.y;
        float gapLength = _Outline_IsDashed_DashRatio_GapRatio_DashOffset.z * pd.y;
        // Number of dashes = floor(perimeter / 2*dashLength)
        float nDashes = floor(pd.y / (dashLength + gapLength));
        // Gap length = (perimeter - numberOfDashes * dashLength) / numberOfDashes
        gapLength = (pd.y - nDashes * dashLength) / nDashes;

        pd.x += pd.y * (1.0 - _Outline_IsDashed_DashRatio_GapRatio_DashOffset.w);
        pd.x %= (dashLength + gapLength);
        return max(- dashLength + pd.x - 0.5 * gapLength, - pd.x + 0.5 * gapLength);
    }
    else
    {
        return -FLT_MAX;
    }
}

bool HasOutline()
{
    return _Outline_Edges_Type_Has.w == 1 && IsValidOutline(_Outline_FillColor, _Outline_Edges_Type_Has.x - _Outline_Edges_Type_Has.y);
}

#endif