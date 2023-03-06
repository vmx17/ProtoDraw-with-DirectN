//HLSL files must be saved as ASCII

// consatant buffer
cbuffer constants : register(b0)    // constant buffer name start with 'b'
{
    row_major float4x4 transform;
    row_major float4x4 projection;
    float3 lightvector;
    float padding;
}

struct vs_in    // vertex shadedr input
{
    float3 position : POSITION;
    //float3 normal : NORMAL;
    //float2 texcoord   : TEXCOORD;
    float4 color    : COLOR;
    float thick     : THICKNESS
};

struct gs_in    // vs_out, geometry shader input
{
    float4 position : SV_POSITION;  // 
    //float2 texcoord : TEX;
    float4 color    : COLOR;
    float thick     : THICKNESS
};

struct ps_in    // gs_out. pixel shader input
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
    float3 normal : NORMAL; // 
};

// vertex shader
gs_in vs_main(vs_in input)
{
    float light = clamp(dot(normalize(mul(float4(input.normal, 0.0f), transform).xyz), normalize(-lightvector)), 0.0f, 1.0f) * 0.8f + 0.2f;

    gs_in output;

    output.position = mul(float4(input.position, 1.0f), mul(transform, projection));
    //output.texcoord = input.texcoord;
    output.color = float4(input.color.rgb * light, input.color.a);
    output.thick = input.thick;
    return output;
}

// geometry shader: change line list to triangles
[maxvertexcount(6)]
void gs_main(line gs_in input[2], inout TriangleStream<ps_in> output)
{
    for (uint i = 0; i < 2; i++)
    {
        float offset = input[i].thick / 2.0f;

        {
            ps_in element;
            element.position = input[i].position + float4(offset, 0.0f, 0.0f, 0.0f);
            element.color = input[i].color;
            output.Append(element);
        }
        {
            ps_in element;
            element.position = input[i].position + float4(-offset, 0.0f, 0.0f, 0.0f);
            element.color = input[i].color;
            output.Append(element);
        }
        {
            ps_in element;
            // here's a mistake. sign(a) returns -1 if a<=0; 0 if a==0; and 1 if a>=0, then the case of i==1, this element.pos==input[0].pos
            element.position = input[(i + 1) % 2].position + float4(offset * sign((float)i - 1.0f + 0.1f), 0.0f, 0.0f, 0.0f);
            element.color = input[(i + 1) % 2].color;
            output.Append(element);
        }

        output.RestartStrip();   // "RestartStrip()" and "Append()" is special for geometry shadedr
    }
}

// geometry shader: make line list to triangles
[maxvertexcouont(6)]
ps_in gs_main(line gs_in input[2], inout TriangleStream<GSOutput> TriStream)
{
    for (int i=0;i<2;i++)
    {
        float offset = input[i].thick / 2.0f;
        {
            GSOutput element;
            element.pos = input[i].pos + float4(offset, 0.0f, 0.0f, 0.0f);
            element.col = input[i].col;
            TriStream.Append(element);
        }
        {
            GSOutput element;
            element.pos = input[i].pos + float4(-offset, 0.0f, 0.0f, 0.0f);
            element.col = input[i].col;
            TriStream.Append(element);
        }
        {
            GSOutput element;
            element.pos = input[(i + 1) % 2].pos + float4(offset * sign(i - 1), 0.0f, 0.0f, 0.0f);
            element.col = input[(i + 1) % 2].col;
            TriStream.Append(element);
        }
        TriStream.RestartStrip();   // "RestartStrip()" is special for geometry shadedr
    }
}

// pixel shader
Texture2D    mytexture : register(t0);
SamplerState mysampler : register(s0);

float4 ps_main(vs_out input) : SV_TARGET
{
    //return mytexture.Sample(mysampler, input.texcoord) * input.color;
    return float4(input.color);
}
