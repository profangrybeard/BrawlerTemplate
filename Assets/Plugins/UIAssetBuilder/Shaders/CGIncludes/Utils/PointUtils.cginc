static const bool USE_DISTANCE_FROM_LINE = false;

float2 pointToClipPosition(float2 p, float2 dimension)
{
    return (2 * p - dimension) / dimension;
}

float2 pointToClipPosition(float2 p, float2 dimension, bool normalizeWidth)
{
    if (normalizeWidth) return (2 * p - dimension) / dimension.x;
    return (2 * p - dimension) / dimension.y;
}

float2 pointToCenterOrigin(float2 p, float2 dimension)
{
    return 1 + 2.0 * p - dimension;
}

float2 uvToCenteredObjectiveSpace(float2 uv, float2 dimension)
{
    return (2 * uv - 1.0) * dimension;
}