// SHADER TEXTURE COMMON

// HasRoundCorners(corners.xyzw)
// GetOutlineCorners(corners.xyzw, thickness)
// #include "/../../CGIncludes/Utils/ShapeUtils.cginc"  

#ifndef USE_SHADERTEXTURE
#define USE_SHADERTEXTURE

// Texture
uniform RWTexture2D<float4> ResultTexture;
uniform float4 _ResultTexture_Dimension;

// Visual
uniform float _Sharpness;
uniform float _Resolution;

#endif