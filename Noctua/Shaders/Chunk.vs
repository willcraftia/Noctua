cbuffer PerObject : register(b0)
{
    float4x4 WorldViewProjection;
};

struct Input
{
    float4 Position     : SV_Position;
    float3 Normal       : NORMAL0;
    float4 Color        : COLOR0;
    float2 TexCoord     : TEXCOORD0;
    int    TileIndex    : TILE_INDEX;
};

struct Output
{
    float4 Position     : SV_Position;
    float3 Normal       : NORMAL0;
    float4 Color        : COLOR0;
    float2 TexCoord     : TEXCOORD0;
    int    TileIndex    : TILE_INDEX;
};

Output VS(Input input)
{
    Output output;

    output.Position = mul(input.Position, WorldViewProjection);
    output.Normal = input.Normal;
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    output.TileIndex = input.TileIndex;

    return output;
}

struct OcclusionInput
{
    float4 Position : SV_Position;
};

struct OcclusionOutput
{
    float4 Position : SV_Position;
};

OcclusionOutput OcclusionVS(OcclusionInput input)
{
    OcclusionOutput output;

    output.Position = mul(input.Position, WorldViewProjection);

    return output;
}

struct WireframeInput
{
    float4 Position : SV_Position;
};

struct WireframeOutput
{
    float4 Position : SV_Position;
};

WireframeOutput WireframeVS(OcclusionInput input)
{
    OcclusionOutput output;

    output.Position = mul(input.Position, WorldViewProjection);

    return output;
}
