// #include "/../../CGIncludes/Shapes/More.cginc" 

#include "/../Utils/DebugUtils.cginc"
#include "/../Common/ShapeCommon.cginc"
#include "/../SDF/Shapes.cginc" 

// Circle
uniform float4 _More_Type_Data;

float DistanceFromMore(in float2 p, in float type, float3 data)
{
    if (type == 1)
        return sdMoon(p, data.x, data.y, data.z);
    else if (type == 2)
        return sdEgg(p + float2(0,data.z), data.x, data.y);
    else if (type == 3)
        return sdCross(p, data.xy, data.z);
    else if (type == 4)
        return sdRoundedX(p, data.x, data.y);
    else if (type == 5)
        return sdCheckmark(p, data.x, data.y);
    else
        return sdHeart((p + float2(0,data.y)) * data.x) * data.z;
}

#ifndef DISTANCE_FROM_SHAPE
#define DISTANCE_FROM_SHAPE
float DistanceFromShape(in float2 p)
{
    return DistanceFromMore(p, _More_Type_Data.x, _More_Type_Data.yzw);
}

float DistanceFromScaledShape(in float2 p, float sizeOffset)
{    
    return DistanceFromMore(p, _More_Type_Data.x, _More_Type_Data.yzw) - 2.0 * sizeOffset;
}
float2 DistanceInPerimeter(float2 p, float d)
{
    return p;
}
#endif
