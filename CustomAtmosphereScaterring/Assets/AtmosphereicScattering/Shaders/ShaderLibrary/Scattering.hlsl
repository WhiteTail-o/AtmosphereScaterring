#ifndef CUSTOM_SCATTERING_INCLUDED
#define CUSTOM_SCATTERING_INCLUDED

#define RAY_STEP_COUNT 50
// #define PI            3.14159265359f

TEXTURE2D(_MainTex); 
SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
    float4 _MainTex_ST;
    float _AtmosphereHeight;
    float _PlanetRadius;
    float2 _DensityScaleHeight;
    float3 _ExtinctionR;    //  Rayleigh Extinction Coefficient
    float3 _ExtinctionM;    //  Mie Extinction Coefficient
    float _MieG;    // g in Mie's phase function
    float3 _ScatteringR; // Rayleigh's Scattering Coefficient while height is 0
    float3 _ScatteringM;
    float3 _IncomingLight; //incoming light
CBUFFER_END


float2 RaySphereInterection(float3 rayOrigin, float3 rayDir, float3 sphereCenter, float sphereRadius) {
    rayOrigin -= sphereCenter;
    rayDir = normalize(rayDir);
    float a = dot(rayDir, rayDir);
    float b = 2.0 * dot(rayOrigin, rayDir);
    float c = dot(rayOrigin, rayOrigin) - sphereRadius * sphereRadius;
    float d = b*b - 4*a*c;

    if (d < 0) {
        return -1;
    }
    else {
        d = sqrt(d);
        return float2(-b-d, -b+d) / (2.0 * a);
    }
}

//----- Input
// position			视线采样点P
// lightDir			光照方向

//----- Output : 
// opticalDepthCP:	dcp
bool lightSampling(float3 position, float3 lightDir, out float2 opticalDepth) {
    opticalDepth = 0;
    float3 rayStart = position;
    float3 rayDir = -normalize(lightDir);
    float3 planetCenter = float3(0, -_PlanetRadius, 0);
    float2 intersection = RaySphereInterection(rayStart, rayDir, planetCenter, _PlanetRadius + _AtmosphereHeight);

    float3 step = (intersection.y * rayDir) / RAY_STEP_COUNT;
    float stepSize = length(step);
    float2 density = 0;

    for(int i = 1; i <= RAY_STEP_COUNT; i++) {
        float3 position = rayStart + step * i;
        float height = abs(length(position - planetCenter) - _PlanetRadius);
        float2 localDensity = exp(-(height.xx) / _DensityScaleHeight.xy);

        density += localDensity * stepSize;
    }
    opticalDepth = density;
    return true;
}

bool GetAtmosphereDensityRealtime(float3 position, float3 planetCenter, float3 lightDir, out float2 dcp, out float2 dpa) {
    float height = length(position - planetCenter) - _PlanetRadius;
    dpa = exp(-(height.xx) / _DensityScaleHeight.xy);

    bool bOverGround = lightSampling(position, lightDir, dcp);
    return bOverGround;
}

//----- Input
//localDensity rho(h)
//densityCP
//densityPA

//----- Output
//LocalInScatterR
//LocalInScatterM

void ComputeLocalInScattering(float2 localDensity, float2 densityCP, float2 densityPA, out float3 localInScatterR, out float3 localInScatterM) {
    float3 Tr = (densityCP + densityPA).x * _ExtinctionR;
    float3 Tm = (densityCP + densityPA).y * _ExtinctionM;

    float3 Transmittance = exp(-(Tr + Tm));

    localInScatterR = localDensity.x * Transmittance;
    localInScatterM = localDensity.y * Transmittance;
}


void ApplyPhaseFunction(inout float3 scatterR, inout float3 scatterM, float cosA) {
    //rayleigh
    float phase = 0.75 * (1 + (cosA * cosA));
    scatterR *= phase;

    float g = _MieG;
    // phase = (1.0 / (4.0 * PI)) * ((3.0 * (1.0 - g*g)) / (2.0 * (2.0 + g*g))) * ((1 + cosA * cosA) / pow(1 + g*g - 2.0*g*cosA, 3.0/2.0));
    phase = ((3.0 * (1.0 - g*g)) / (2.0 * (2.0 + g*g))) * ((1 + cosA * cosA) / pow(abs(1 + g*g - 2.0*g*cosA), 3.0/2.0));
    scatterM *= phase;
}

//----- Input
// rayStart		视线起点 A
// rayDir		视线方向
// rayLength		AB 长度
// planetCenter		地球中心坐标
// distanceScale	世界坐标的尺寸
// lightdir		太阳光方向
// sampleCount		AB 采样次数

//----- Output : 
// extinction       T(PA)
// inscattering:	Inscatering
float4 IntegrateInscatteringRealtime(float3 rayStart, float3 rayDir, float rayLength, float3 planetCenter, float distanceScale, float3 lightDir, out float4 extinction)
{
    float2 localDensity;
    float2 densityCP = 0;
    float2 densityPA = 0;
    float3 scatterR = 0, scatterM = 0;

    float2 prevDpa;
    float2 prevLocalDensity;
    float3 prevLocalInScatterR, prevLocalInScatterM;
    
    GetAtmosphereDensityRealtime(rayStart, planetCenter, lightDir, densityCP, prevLocalDensity);
    ComputeLocalInScattering(prevLocalDensity, densityCP, densityPA, prevLocalInScatterR, prevLocalInScatterM);
    float3 step = rayDir * rayLength / RAY_STEP_COUNT;
    float stepSize = length(step);

    //Point P - Current integration point
    //Point A - camera position
    //Point C - top(light direction) of the atmosphere
    [loop]
    for (int i = 1; i <= RAY_STEP_COUNT; i++) {
        float3 p = rayStart + step * i;
        GetAtmosphereDensityRealtime(p, planetCenter, lightDir, densityCP, localDensity);

        densityPA += (localDensity + prevLocalDensity) / 2.0 * stepSize;
        float3 localInScatterR, localInScatterM;
        ComputeLocalInScattering(localDensity, densityCP, densityPA, localInScatterR, localInScatterM);

        scatterR += (localInScatterR + prevLocalInScatterR) / 2.0 * stepSize;
        scatterM += (localInScatterM + prevLocalInScatterM) / 2.0 * stepSize;

        prevLocalInScatterR = localInScatterR;
        prevLocalInScatterM = localInScatterM;

        prevLocalDensity = localDensity;
    }

    ApplyPhaseFunction(scatterR, scatterM, dot(-normalize(rayDir), normalize(lightDir.xyz)));
    // float3 incomingLight = _IncomingLight.xyz;
    float3 incomingLight = _MainLightColor.rgb;
    float3 lightInScatter = (scatterR * _ScatteringR + scatterM * _ScatteringM) / (4.0 * PI) * incomingLight;
    float3 lightExtinction = exp(-(densityCP.x * _ExtinctionR + densityCP.y * _ExtinctionM));

    extinction = float4(lightExtinction, 1.0);
    return float4(lightInScatter, 1.0);
}



#endif