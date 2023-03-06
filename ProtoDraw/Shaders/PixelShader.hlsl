struct ps_in
{
    float4 pos : SV_POSITION;
    float4 color : COLOR;
    //float3 nor : NORMAL;
};

float4 main(ps_in input) : SV_TARGET
{
    return float4(input.color.xyz, input.color.w);
}
