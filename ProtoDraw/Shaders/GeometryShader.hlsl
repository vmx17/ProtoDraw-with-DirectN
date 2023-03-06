//HLSL files must be saved as ASCII

cbuffer constants : register(b0)    // constant buffer name start with 'b'
{
    row_major float4x4 transform;
    row_major float4x4 projection;
    row_major float4x4 world;
};

struct gs_in    // vs_out, geometry shader input
{
    float4 position : SV_POSITION;  // AXIS
    float4 color    : COLOR;
    float thick : THICKNESS;
};

struct ps_in    // gs_out. pixel shader input
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
    //float3 normal : NORMAL; // 
};

// geometry shader: make line list to triangles
[maxvertexcount(6)]
void main(line gs_in input[2], inout TriangleStream<ps_in> output)
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
            element.position = input[(i + 1) % 2].position + float4(offset * sign(i - 1), 0.0f, 0.0f, 0.0f);
            element.color = input[(i + 1) % 2].color;
            output.Append(element);
        }

        output.RestartStrip();   // "RestartStrip()" and "Append()" is special for geometry shadedr
    }
}
