static const bool USE_DISTANCE_FROM_LINE = false;

float distanceFromLine(float2 p, float2 origin, float2 target)
{
    return abs(((target.x - origin.x) * (origin.y - p.y) - (origin.x - p.x) * (target.y - origin.y)) / distance(origin, target));
}

float distanceFromSegment(float2 p, float2 origin, float2 target)
{
    float2 pointLine = p - origin;
    float2 targetLine = target - origin;
    
    float h = clamp(dot(pointLine, targetLine) / dot(targetLine, targetLine), 0, 1);
    return length(pointLine - targetLine * h);
}

float distanceFromNormalLine(float2 p, float2 normal)
{
    return (normal.x) * (- p.y) - (- p.x) * (normal.y);
}

float2 getNormalizedLine(float2 target, float2 origin)
{
    return normalize(float2(target.x - origin.x, target.y - origin.y));
}

float4 drawLine(float2 id, float4 lineColor, float2 origin, float2 target, float lineWidth)
{
    float4 color = lineColor;
    float d = 0;
    if (USE_DISTANCE_FROM_LINE)
    {
        d = distanceFromLine(id, origin, target);
    }
    else
    {
        d = distanceFromSegment(id, origin, target);
    }
    
    float alpha = clamp(0.5 + lineWidth * 0.5 - d, 0, 1);
    // Sharpness - note: added here to avoid cutting off when lineColor is transparent
    alpha = smoothstep(0 + 0.5 * sharpness, 1 - 0.5 * sharpness, alpha);
    // Final color
    color.a *= alpha;
    return color;
}