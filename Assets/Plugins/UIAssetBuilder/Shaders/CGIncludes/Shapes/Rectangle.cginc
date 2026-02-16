
// #include "/../../CGIncludes/Shapes/Rectangle.cginc" 

// HasRoundCorners(corners.xyzw)
// GetOutlineCorners(corners.xyzw, thickness)
#include "/../Common/ShapeCommon.cginc" 
// sdBox (in pixel.xy, in dimension.xy)
// sdRoundedBox (in pixel.xy, in dimension.xy, in corners(top-right, bottom-right, top-left, bottom-left))
#include "/../SDF/Primitives.cginc" 

// Rectangle
uniform float _Rectangle_Width;
uniform float _Rectangle_Height;
uniform float4 _Rectangle_Corners;

float DistanceFromRectangle(float2 p, float2 dimension, float4 corners)
{
    bool hasRoundCorners = HasRoundCorners(corners);

    return (hasRoundCorners) 
        ? sdRoundedBox (p, dimension, 2 * corners)
        : sdBox (p, dimension);
}


float4 GetOutlineCorners(float4 corners, float thickness)
{
    if (IsRoundEdges() || thickness < 0)
    {
        return float4
        (
            max(0.0, corners.x + thickness),
            max(0.0, corners.y + thickness),
            max(0.0, corners.z + thickness),
            max(0.0, corners.w + thickness)
        );
    }
    else
        return float4
        (
            corners.x < CUTOFF ? 0.0 : corners.x + thickness,
            corners.y < CUTOFF ? 0.0 : corners.y + thickness,
            corners.z < CUTOFF ? 0.0 : corners.z + thickness,
            corners.w < CUTOFF ? 0.0 : corners.w + thickness
        );    
}

float4 CalculatePerimiter( in float2 b, in float4 r )
{
    float c = (2.0 - 1.570796);
    return float4(
        1.0 * (b.x + b.y) - c * (r.z),
        2.0 * (b.x + b.y) - c * (r.z + r.x),
        3.0 * (b.x + b.y) - c * (r.z + r.x + r.y),
        4.0 * (b.x + b.y) - c * (r.x + r.y + r.z + r.w)
    );
}

float2 pdRoundBox(in float2 p, in float2 b, in float4 r)
{    
    //Adjusted to be used in formula
    r *= 2.0; 

    float4 per = CalculatePerimiter(b, r);
    
    r.xy = (p.x > 0.0) ? r.xy : r.zw;
    r.x  = (p.y > 0.0) ? r.x  : r.y;
    
    float2 q = abs(p) - b + r.x;
        
    per.x = (p.y > 0.0) ? per.x : per.z;
    per.x += sign(p.y * p.x) * (b.x + (q.x > 0.0 ? (r.x * 0.570796 - (q.y > 0.0 ? r.x * atan2(q.y,q.x) : q.y)) : (q.x > q.y ? b.y - abs(p.y) : abs(p.x) - b.x)));

    return float2(per.x, per.w);
}

#ifndef DISTANCE_FROM_SHAPE
#define DISTANCE_FROM_SHAPE
float DistanceFromShape(float2 p)
{
    return DistanceFromRectangle(p, float2(_Rectangle_Width, _Rectangle_Height), _Rectangle_Corners);
}
float DistanceFromScaledShape(float2 p, float sizeOffset)
{
    float2 dimension = float2(_Rectangle_Width, _Rectangle_Height) + 2 * sizeOffset;
    return DistanceFromRectangle(p, dimension, GetOutlineCorners(_Rectangle_Corners, sizeOffset));
}
float2 DistanceInPerimeter(in float2 p, in float sizeOffset)
{    
    float2 offsetDimension = float2(_Rectangle_Width, _Rectangle_Height) + 2.0 * sizeOffset;
    float4 offsetCorners = GetOutlineCorners(_Rectangle_Corners, sizeOffset);
    
    return pdRoundBox(p, offsetDimension, offsetCorners);
}
#endif