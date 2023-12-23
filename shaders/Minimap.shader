HEADER
{
	Description = "Minimap";
}

MODES
{
	Default();
    VrForward();
    ToolsVis( S_MODE_TOOLS_VIS );
}

COMMON
{
	#include "common/shared.hlsl"
}

struct PS_INPUT
{
	#if ( PROGRAM == VFX_PROGRAM_VS )
		float4 vPositionPs : SV_Position;
	#endif

	#if ( ( PROGRAM == VFX_PROGRAM_PS ) )
		float4 vPositionSs : SV_ScreenPosition;
	#endif
};

struct VS_INPUT
{
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
};

VS
{
    PS_INPUT MainVs( VS_INPUT i )
    {
        PS_INPUT o;
        o.vPositionPs = float4( i.vPositionOs.xyz, 1.0f );
        return o;
    }
}

PS
{    
    #define DEPTH_STATE_ALREADY_SET
    #include "vr_common_ps_code.fxc"

    float4 g_flFloorColor < Attribute( "FloorColor" ); >;
	float4 g_flOutlineColor < Attribute( "OutlineColor" ); >;

    float4 MainPs( PS_INPUT i ) : SV_Target0
    { 	
        float depth = Depth::GetNormalized( i.vPositionSs.xy );
        float alpha = 1 - floor(depth);
        float3 floor_color = g_flFloorColor.rgb * alpha;

        // Calculate color by threshold.
        const float THRESHOLD = 0.4;
        float3 result = lerp(floor_color, g_flOutlineColor.rgb, ceil(THRESHOLD - depth));

        return float4(result, alpha);
    }
}