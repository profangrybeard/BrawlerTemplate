
#ifndef USE_COLORUTILS
#define USE_COLORUTILS

static const bool USE_PREMULTIPLIED_ALPHA = false;
static const float ALPHA_CUTOFF = 1.0/255.0;

// ca is the color in the 'front'
// cb is the color in the 'back'
float4 composeColor(float4 ca, float4 cb, bool premultipliedAlpha = USE_PREMULTIPLIED_ALPHA)
{
    if (ca.a < ALPHA_CUTOFF)
        return cb;
    else
    {
        float4 color = cb;
    
        // Composing alpha
        color.a = ca.a + cb.a * (1.0 - ca.a);
        // Composing color
        if (premultipliedAlpha)
        {
            color.rgb = ca.rgb + cb.rgb * (1.0 - ca.a);
        }
        else if (color.a > 0.0)
        {
            color.rgb = ca.rgb * ca.a + cb.rgb * cb.a * (1.0 - ca.a);
            color.rgb /= color.a;
        }
        return color;
    }
}

float4 mixColor(float4 ca, float4 cb)
{
    if (ca.a < ALPHA_CUTOFF)
        return cb;
    
    float4 color = cb;
    
    // Composing alpha
    color.a = ca.a + cb.a;
    // Composing color
    if (color.a > 0.0)
    {
        color.rgb = ca.rgb * ca.a + cb.rgb * cb.a;
        color.rgb /= color.a;
    }
    return color;
}

float4 colorBlend_Screen(float4 a, float4 b)
{
    return 1.0 - (1.0-a) * (1.0-b);
}

float4 LerpSRGB(in float4 color1, in float4 color2, in float ratio)
{
    float exp = 2.2;
    float3 colorRGB = lerp(pow(color1.rgb, 1.0 / exp), pow(color2.rgb, 1.0 / exp), ratio);
    float alpha = lerp(color1.a, color2.a, ratio);
    return float4(pow(colorRGB, exp), alpha);
}

float GetSmoothedAlpha(float d, float smoothDistance, bool isOuterSmoothing = false)
{
    if (isOuterSmoothing) return smoothstep(0, smoothDistance, d);
    return smoothstep(-smoothDistance, 0, d);
}
float GetCenteredSmoothedAlpha(float d, float smoothDistance)
{
    return smoothstep(-smoothDistance, smoothDistance, d);
}
#ifdef USE_PARTIAL_DERIVATIVE
float GetAlphaFromDistance(float d, float sharpness = 0, bool isOuterSmoothing = false)
{
    float pd = fwidth(d);
    if (!isOuterSmoothing) return 1 - saturate(-d / pd);
    return saturate(d / pd);
}
#else 
// THERE IS SOME ISSUE ON COMPILATION BECAUSE OF THIS!
float GetAlphaFromDistance(float d, float sharpness = 0, bool isOuterSmoothing = false)
{
    return GetCenteredSmoothedAlpha(d, (1-sharpness));
    //return GetCenteredSmoothedAlpha(d - 1, 1.5 * (1-sharpness)); // << very close to frag shader image
}
#endif


#endif