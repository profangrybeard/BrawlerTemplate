// #include "/../../CGIncludes/Shapes/Circle.cginc" 

#include "/../Utils/DebugUtils.cginc"
#include "/../Common/ShapeCommon.cginc"
#include "/../SDF/Primitives.cginc" 

// Circle
uniform float4 _Circle_Dimension_Type;

float DistanceFromCircle(in float2 p, in float2 dimension)
{
    if (_Circle_Dimension_Type.z == 1)
        return sdCircle (p, dimension.x);
    else
        return sdEllipse( p, dimension );
}

float CalculateArcLength(float a, float b, float angleInRadians)
{
    // Compute the eccentricity
    float e = sqrt(1 - (b * b) / (a * a));
        
    // Ramanujan's approximation for arc length
    float arcLength = angleInRadians * (1 + (3 * e * e) / (10 + sqrt(4 - 3 * e * e)));
        
    return arcLength * a;
}

float2 pdCircle(in float2 p, in float2 dimension)
{    
    //Ellipse
    float2 q = abs(p/sqrt(dimension));
    float angle = atan2(q.y,q.x);
    float r = length(p);
    float perimeter = 2.0 * PI * r;
    float distance = (p.x > 0 ? 0.5 : p.y > 0 ? 0.0 : 1.0) * perimeter - sign (p.y * p.x) * r * angle;
    return float2(distance, perimeter);
}

float2 toEllipse(float2 p, float2 ab) {
    float2 signP = sign(p);
    // symmetry
	p = abs( p );

    // find root with Newton solver
    float2 q = ab*(p-ab);
	float w = (q.x<q.y)? 1.570796327 : 0.0;
    for( int i=0; i<5; i++ )
    {
        float2 cs = float2(cos(w),sin(w));
        float2 u = ab*float2( cs.x,cs.y);
        float2 v = ab*float2(-cs.y,cs.x);
        w = w + dot(p-u,v)/(dot(p-u,u)+dot(v,v));
    }
    
    // compute final point and distance
    return signP * ab * float2(cos(w),sin(w));
}

#ifndef DISTANCE_FROM_SHAPE
#define DISTANCE_FROM_SHAPE
float DistanceFromShape(in float2 p)
{
    return DistanceFromCircle(p, _Circle_Dimension_Type.xy);
}

float DistanceFromScaledShape(in float2 p, float sizeOffset)
{    
    return DistanceFromCircle(p, _Circle_Dimension_Type.xy) - 2.0 * sizeOffset;
}
float2 DistanceInPerimeter(float2 p, float d)
{
    //float dist = DistanceFromScaledShape(p, d);
    float2 dimension = _Circle_Dimension_Type.xy + d;
    return pdCircle(toEllipse(p, dimension), dimension);
}
#endif
