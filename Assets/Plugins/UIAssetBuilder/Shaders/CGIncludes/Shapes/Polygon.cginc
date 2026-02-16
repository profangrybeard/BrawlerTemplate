// #include "/../../CGIncludes/Shapes/Polygon.cginc" 

#include "/../Utils/DebugUtils.cginc"
#include "/../Common/ShapeCommon.cginc"
#include "/../SDF/Polygons.cginc" 

// Circle
uniform float4 _Polygon_Size_Roundness_InnerAngle_OffsetMod;
uniform float4 _Polygon_Helper_Data;

float DistanceFromPolygon(in float2 p, in float size, float roundness)
{
    return sdStar2( p, size - roundness, _Polygon_Size_Roundness_InnerAngle_OffsetMod.z, _Polygon_Helper_Data.xy, _Polygon_Helper_Data.zw ) - roundness;
}

#ifndef DISTANCE_FROM_SHAPE
#define DISTANCE_FROM_SHAPE
float DistanceFromShape(in float2 p)
{
    return DistanceFromPolygon(p, _Polygon_Size_Roundness_InnerAngle_OffsetMod.x, _Polygon_Size_Roundness_InnerAngle_OffsetMod.y);
}

float DistanceFromScaledShape(in float2 p, float sizeOffset)
{
    sizeOffset *= _Polygon_Size_Roundness_InnerAngle_OffsetMod.w;
    return DistanceFromPolygon(p, _Polygon_Size_Roundness_InnerAngle_OffsetMod.x + 2 * sizeOffset, _Polygon_Size_Roundness_InnerAngle_OffsetMod.y);
}
float2 DistanceInPerimeter(float2 p, float d)
{
    return p;
}
#endif
