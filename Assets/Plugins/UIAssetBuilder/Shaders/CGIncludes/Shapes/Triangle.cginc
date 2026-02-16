// #include "/../../CGIncludes/Shapes/Triangle.cginc" 

#include "/../Utils/DebugUtils.cginc"
#include "/../Common/ShapeCommon.cginc"
#include "/../SDF/Primitives.cginc" 

// Triangle
uniform float4 _Triangle_Dimension_Corner_Type;

float DistanceFromTriangle(in float2 p, in float2 dimension, in float corners)
{
    if (_Triangle_Dimension_Corner_Type.w == 1) //Equilateral
        return sdEquilateralTriangle( p, dimension.x - corners ) - corners;
    else
        return sdTriangleIsosceles( p, dimension - float2(corners,-2.0 * corners)) - corners;
}

#ifndef DISTANCE_FROM_SHAPE
#define DISTANCE_FROM_SHAPE
float DistanceFromShape(in float2 p)
{
    return DistanceFromTriangle(p, _Triangle_Dimension_Corner_Type.xy, _Triangle_Dimension_Corner_Type.z);
}

float DistanceFromScaledShape(in float2 p, float sizeOffset)
{    
    if (_Triangle_Dimension_Corner_Type.w == 1)
    {
        sizeOffset *= 2.0 * SQRT_3;
        return DistanceFromTriangle(p, _Triangle_Dimension_Corner_Type.xy + sizeOffset, _Triangle_Dimension_Corner_Type.z);        
    }
    else
    {
        float width = _Triangle_Dimension_Corner_Type.x;
        float height = - 0.5 * _Triangle_Dimension_Corner_Type.y;
        float h = length(float2(width * 0.5, height));
        float wOffset = sizeOffset * (2.0 * h + width) / height;
        float hOffset = wOffset * height / width;

        float2 offset = float2 (wOffset, - 2.0 * hOffset);
        return DistanceFromTriangle(p - float2(0, hOffset - 2.0 * sizeOffset), _Triangle_Dimension_Corner_Type.xy + offset, _Triangle_Dimension_Corner_Type.z);
    }
}
float2 DistanceInPerimeter(float2 p, float d)
{
    return p;
}
#endif
