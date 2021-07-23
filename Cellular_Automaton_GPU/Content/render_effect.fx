#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

Texture2D SpriteTexture;
texture2D logictex;
int mode;
float zoom;
float2 coos;
int Screenwidth, Screenheight, worldsizex, worldsizey, mousepos_X, mousepos_Y;
int Selection_type;
int selection_start_X, selection_start_Y, selection_end_X, selection_end_Y;
bool IsDisplayHighlighted;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};
float4 getcoloratpos(float x, float y)
{
	uint ux = (uint)x;
	uint uy = (uint)y;
	float4 OUT = float4(0, 0, 0, 1);
	float type = logictex[uint2(x, y)].r;
	if (type < 0.5f) { /*Do Nothing*/ }
	else if (type > 0.5f && type < 4.5f)
		OUT = float4(1, 1, 1, 1);
	else if (type < 5.5f)
		OUT = float4(0.24f, 0.47f, 0, 1);
	else if (type < 6.5f)
		OUT = float4(0.2f, 0.4f, 0.82f, 1);
	else if (type < 8.5f)
		OUT = float4(0.196f, 0, 0, 1);
	else if (type < 10.5f)
		OUT = float4(0.8f, 0, 0, 1);
	else if (type < 11.5f)
		OUT = float4(1.0f, 1.0f, 0, 1);
	else if (type < 12.5f)
		OUT = float4(0.294f, 0.196f, 0, 1);
	else if (type < 13.5f)
		OUT = float4(0.392f, 0.0f, 0.49f, 1);
		

	if (IsDisplayHighlighted && type <= 6.5f)
	{
		OUT *= 0.15f;
	}

	if (zoom > 2)
	{
		float factor = 0.8f / zoom;
		if ((x % 10.0f >= 10-factor || x % 10.0f <= factor) || (y % 10.0f >= 10-factor || y % 10.0f <= factor))
			OUT = float4(0.15f, 0.15f, 0.15f, 1);
		else if(zoom > 4 && ((x % 1 >= 1-factor || x % 1 <= factor) || (y % 1 >= 1-factor || y % 1 <= factor)))
			OUT = float4(0.04f, 0.04f, 0.04f, 1);
	}
	if (Selection_type > 0 || Selection_type < 0)
	{
		if (x >= selection_start_X && x <= selection_end_X + 1 && y >= selection_start_Y && y <= selection_end_Y + 1)
		{
			if(Selection_type > 0)
				OUT = OUT * 0.8f + float4(1, 1, 1, 1) * 0.2f;
			if (Selection_type < 0)
			{
				OUT = OUT * 0.85f + float4(1, 0, 0, 1) * 0.15f;
			}
		}
	}
	else if ((x >= mousepos_X && x <= mousepos_X+1) || (y >= mousepos_Y && y <= mousepos_Y + 1))
	{
		OUT = OUT * 0.85f + float4(1, 1, 1, 1) * 0.15f;
	}
	return OUT;
}
float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 OUT = float4(0, 0, 0, 1);
	uint xcoo = input.TextureCoordinates.x * Screenwidth;
	uint ycoo = input.TextureCoordinates.y * Screenheight;
	if (mode == 0)
	{
		if (xcoo >= coos.x && xcoo <= coos.x + worldsizex * zoom && ycoo >= coos.y && ycoo <= coos.y + worldsizey * zoom)
		{
			OUT = getcoloratpos((xcoo - coos.x) / zoom, (ycoo - coos.y) / zoom);
		}
		else
			OUT = float4(0.25f, 0.25f, 0.25f, 1);
	}
	else if (mode == 1)
	{
		return getcoloratpos(input.TextureCoordinates.x * worldsizex, input.TextureCoordinates.y * worldsizey);
	}
	else if (mode == 2)
	{
		if (xcoo >= coos.x && xcoo <= coos.x + worldsizex * zoom && ycoo >= coos.y && ycoo <= coos.y + worldsizey * zoom)
		{
			OUT = logictex[uint2(xcoo - coos.x, ycoo - coos.y)];
		}
		else
			OUT = float4(0.25f, 0.25f, 0.25f, 1);
	}
	return OUT + tex2D(SpriteTextureSampler, input.TextureCoordinates);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};