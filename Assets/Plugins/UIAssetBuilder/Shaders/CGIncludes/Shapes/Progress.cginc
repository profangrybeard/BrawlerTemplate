// #include "/../../CGIncludes/Shapes/Progress.cginc" 

#include "/../Utils/DebugUtils.cginc"
#include "/../Common/ShapeCommon.cginc"
#include "/../SDF/ProgressShapes.cginc" 
#include "/../SDF/Primitives.cginc" 

// Progress
uniform float4 _Progress_Type_Size_Thickness_Ratio;
uniform float4 _Progress_Progress;

float DistanceFromProgress(in float2 p, in float4 progress, in float size, in float thickness)
{
    float2 rotP = mul(float2x2(progress.ywxy), p);

    // ARC
    if (_Progress_Type_Size_Thickness_Ratio.x == 0)
        return sdArc (rotP, progress.xy, size, thickness);
    // RING
    else if (_Progress_Type_Size_Thickness_Ratio.x == 1)
        return sdRing (rotP,progress.zw, size, thickness);
    // PIE
    else if (_Progress_Type_Size_Thickness_Ratio.w == 1)
        return sdCircle (rotP, size);
    else
        return sdPie (rotP, progress.xy, size);
}

#ifndef DISTANCE_FROM_SHAPE
#define DISTANCE_FROM_SHAPE
float DistanceFromShape(in float2 p)
{
    if (_Progress_Type_Size_Thickness_Ratio.x == -1)
        return FLT_MAX;
    else
        return DistanceFromProgress(p, _Progress_Progress, _Progress_Type_Size_Thickness_Ratio.y, _Progress_Type_Size_Thickness_Ratio.z);
}

float DistanceFromScaledShape(in float2 p, float sizeOffset)
{    
    return DistanceFromShape(p) - 2 * sizeOffset;
}
float2 DistanceInPerimeter(float2 p, float d)
{
    return p;
}
#endif
