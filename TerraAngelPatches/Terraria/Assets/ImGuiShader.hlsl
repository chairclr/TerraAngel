texture2D Texture;

sampler TextureSampler : register(s0) = sampler_state
{
    Texture = (Texture);
};

float4x4 WorldViewProj : register(vs, c15);

struct VS_IN
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR;
};
struct VS_OUT
{
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float4 Position : SV_Position;
};

struct PS_IN
{
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

VS_OUT VS(VS_IN vin)
{
    VS_OUT output;
    
    output.Position = mul(vin.Position, WorldViewProj);
    output.TexCoord = vin.TexCoord;
    output.Color = vin.Color;
    
    return output;
}

float4 PS(PS_IN input) : SV_Target0
{
    float4 finalColor;
    
    finalColor = tex2D(TextureSampler, input.TexCoord);
    finalColor *= input.Color;
    
    return finalColor;
}

Technique ImGuiShader
{
    Pass
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}