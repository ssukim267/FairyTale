#ifndef SRS_TERRAIN_BAKE_SHADER
#define SRS_TERRAIN_BAKE_SHADER
uniform float _MapID;

float3 CalculateDistantNorm(float3 distantNormal, float3 geomN)
{
		float3 geomTangent = normalize(cross(geomN, float3(0, 0, 1)));
	    float3 geomBitangent = normalize(cross(geomTangent, geomN));
		distantNormal = distantNormal.x * geomTangent + distantNormal.y * geomBitangent + distantNormal.z * geomN;  
		distantNormal = distantNormal.xzy;
	return distantNormal;
}
void SelectMap(float4 mixedAlbedo, float3 worldNormal, float3 geomN, float finalMask, out float3 map)
{
	float3 nrm = worldNormal * 0.5 + 0.5;
	map = float3(0, 0, 0);
	map = _MapID == 3.0 ? float3(finalMask, finalMask, finalMask) : map;
	map = _MapID == 2.0 ? float3(mixedAlbedo.a, mixedAlbedo.a, mixedAlbedo.a) : map;
	map = _MapID == 1.0 ? float3(nrm.xz, 1) : map;
	map = _MapID == 0.0 ? mixedAlbedo.rgb : map;
}
#endif