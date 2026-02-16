// The MIT License
// Copyright © 2020 Inigo Quilez
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// SHAPES
#ifndef USE_SHAPES
#define USE_SHAPES

float dot2( in float2 v ) { return dot(v,v); }

// Line
float sdSegment( in float2 p, in float2 a, in float2 b )
{
    float2 pa = p-a, ba = b-a;
    float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
    return length( pa - ba*h );
}

// Moon
float sdMoon(float2 p, float d, float ra, float rb )
{
    p.y = abs(p.y);
    float a = (ra*ra - rb*rb + d*d)/(2.0*d);
    float b = sqrt(max(ra*ra-a*a,0.0));
    if( d*(p.x*b-p.y*a) > d*d*max(b-p.y,0.0) )
          {return length(p-float2(a,b));}
    else {return max( (length(p          )-ra),
               -(length(p-float2(d,0))-rb));}
}

// Rounded Cross
float sdRoundedCross( in float2 p, in float h )
{
    float k = 0.5*(h+1.0/h);
    p = abs(p);
    return ( p.x<1.0 && p.y<p.x*(k-h)+h ) ? 
             k-sqrt(dot2(p-float2(1,k)))  :
           sqrt(min(dot2(p-float2(0,h)),
                    dot2(p-float2(1,0))));
}

// Egg
float sdEgg( in float2 p, in float ra, in float rb )
{
    const float k = sqrt(3.0);
    p.x = abs(p.x);
    float r = ra - rb;
    return ((p.y<0.0)       ? length(float2(p.x,  p.y    )) - r :
            (k*(p.x+r)<p.y) ? length(float2(p.x,  p.y-k*r)) :
                              length(float2(p.x+r,p.y    )) - 2.0*r) - rb;
}

// Heart
float sdHeart( in float2 p )
{
    p.x = abs(p.x);

    if( p.y+p.x>1.0 )
        {return sqrt(dot2(p-float2(0.25,0.75))) - sqrt(2.0)/4.0;}
    else {return sqrt(min(dot2(p-float2(0.00,1.00)),
                    dot2(p-0.5*max(p.x+p.y,0.0)))) * sign(p.x-p.y);}
}

// Cross
float sdCross( in float2 p, in float2 b, float r ) 
{
    p = abs(p); p = (p.y>p.x) ? p.yx : p.xy;
    float2  q = p - b;
    float k = max(q.y,q.x);
    float2  w = (k>0.0) ? q : float2(b.y-p.x,-k);
    return sign(k)*length(max(w,0.0)) + r;
}

// X
float sdRoundedX( in float2 p, in float w, in float r )
{
    p = abs(p);
    return length(p-min(p.x+p.y,w)*0.5) - r;
}

// Checkmark
float sdCheckmark( in float2 p, in float h, in float r )
{
    float halfR = 0.5 * r;
    float2 center = float2(h, 2.0 * h + halfR);
    float2 left = float2(center.x + 2.0 * h + halfR, 0);
    float2 right = float2(center.x - 4.0 * h - halfR, - 2.0 * h - halfR);
    return min(sdSegment(p, center, left), sdSegment(p, center, right)) - r;
}

// Bobbly Cross
float sdBlobbyCross( in float2 pos, float he )
{
    pos = abs(pos);
    pos = float2(abs(pos.x-pos.y),1.0-pos.x-pos.y)/sqrt(2.0);

    float p = (he-pos.y-0.25/he)/(6.0*he);
    float q = pos.x/(he*he*16.0);
    float h = q*q - p*p*p;
    
    float x;
    if( h>0.0 ) { float r = sqrt(h); x = pow(q+r,1.0/3.0)-pow(abs(q-r),1.0/3.0)*sign(r-q); }
    else        { float r = sqrt(p); x = 2.0*r*cos(acos(q/(p*r))/3.0); }
    x = min(x,sqrt(2.0)/2.0);
    
    float2 z = float2(x,he*(1.0-2.0*x*x)) - pos;
    return length(z) * sign(z.y);
}

// Stairs
float sdStairs( in float2 p, in float2 wh, in float n )
{
    float2 ba = wh*n;
    float d = min(dot2(p-float2(clamp(p.x,0.0,ba.x),0.0)), 
                  dot2(p-float2(ba.x,clamp(p.y,0.0,ba.y))) );
    float s = sign(max(-p.y,p.x-ba.x) );

    //float dia = length(wh);
    //p = mul(float2x2(wh.x,-wh.y, wh.y,wh.x),p)/dia;
    //float id = clamp(round(p.x/dia),0.0,n-1.0);
    //p.x = p.x - id*dia;
    //p = mul(float2x2(wh.x, wh.y,-wh.y,wh.x),p)/dia;
    
    float dia2 = dot2(wh);
    p = mul(float2x2(wh.x,-wh.y,wh.y,wh.x),p);
    float id = clamp(round(p.x/dia2),0.0,n-1.0);
    p.x = p.x - id*dia2;
    p = mul(float2x2(wh.x,wh.y,-wh.y,wh.x),p/dia2);

    float hh = wh.y/2.0;
    p.y -= hh;
    if( p.y>hh*sign(p.x) ) s=1.0;
    p = (id<0.5 || p.x>0.0) ? p : -p;
    d = min( d, dot2(p-float2(0.0,clamp(p.y,-hh,hh))) );
    d = min( d, dot2(p-float2(clamp(p.x,0.0,wh.x),hh)) );
    
    return sqrt(d)*s;
}

// Circle Wave 
float sdCircleWave( in float2 p, in float tb, in float ra )
{
    tb = 3.1415927*5.0/6.0*max(tb,0.0001);
    float2 co = ra*float2(sin(tb),cos(tb));
    p.x = abs(fmod(p.x,co.x*4.0)-co.x*2.0);
    float2  p1 = p;
    float2  p2 = float2(abs(p.x-2.0*co.x),-p.y+2.0*co.y);
    float d1 = ((co.y*p1.x>co.x*p1.y) ? length(p1-co) : abs(length(p1)-ra));
    float d2 = ((co.y*p2.x>co.x*p2.y) ? length(p2-co) : abs(length(p2)-ra));
    return min(d1, d2); 
}

#endif