cbuffer PerCamera : register(b0)
{
    float4x4 ViewProjection;
};

struct Input
{
    float4 Position : SV_Position;
    float3 Normal   : NORMAL0;
};

struct Output
{
    float4 Position : SV_Position;
    float3 Normal   : NORMAL0;
};

Output VS(Input input)
{
    Output output;

    output.Position = mul(input.Position, ViewProjection);
    output.Normal = input.Normal;

    return output;
}
