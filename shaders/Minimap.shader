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

    float4 MainPs( PS_INPUT i ) : SV_Target0
    { 	
        float depth = Depth::GetNormalized( i.vPositionSs.xy );
        float vis = 1 - floor(depth);

        return vis;
    }
}