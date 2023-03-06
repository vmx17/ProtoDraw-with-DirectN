cbuffer constants : register(b0)    // constant buffer name start with 'b'
{
    row_major float4x4 transform;
    row_major float4x4 projection;
    row_major float4x4 world;
}

struct vs_in    // vertex shadedr input
{
    float3 position : POSITION;
    float4 color    : COLOR;
    float thick : THICKNESS;
};

struct gs_in    // vs_out, geometry shader input
{
    float4 position : SV_POSITION;  // AXIS
    float4 color    : COLOR;
    float thick : THICKNESS;
};

// vertex shader
gs_in main(vs_in input)
{
    gs_in output;

    output.position = mul(float4(input.position, 1.0f), mul(transform, projection));
    output.color = float4(input.color.rgb, input.color.a);
    output.thick = input.thick;

    return output;
}
