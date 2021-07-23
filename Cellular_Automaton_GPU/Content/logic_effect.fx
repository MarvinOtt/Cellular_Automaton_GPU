#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

Texture2D SpriteTexture;
int worldsizex, worldsizey;

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

float mainlogic(VertexShaderOutput input) : COLOR
{
	uint xcoo = input.TextureCoordinates.x * worldsizex;
	uint ycoo = input.TextureCoordinates.y * worldsizey;

	uint left = xcoo - 1;
	if (xcoo == 0)
		left = worldsizex - 1;
	uint right = (xcoo + 1) % (uint)worldsizex;
	uint top = ycoo - 1;
	if (ycoo == 0)
		top = worldsizey - 1;
	uint bottom = (ycoo + 1) % (uint)worldsizey;

	uint left_type = SpriteTexture[uint2(left, ycoo)].r + 0.5f;
	uint right_type = SpriteTexture[uint2(right, ycoo)].r + 0.5f;
	uint top_type = SpriteTexture[uint2(xcoo, top)].r + 0.5f;
	uint bottom_type = SpriteTexture[uint2(xcoo, bottom)].r + 0.5f;
	uint current_type = SpriteTexture[uint2(xcoo, ycoo)].r + 0.5f;

	uint bot_left_type = SpriteTexture[uint2(left, bottom)].r + 0.5f;
	uint bot_right_type = SpriteTexture[uint2(right, bottom)].r + 0.5f;
	uint top_left_type = SpriteTexture[uint2(left, top)].r + 0.5f;
	uint top_right_type = SpriteTexture[uint2(right, top)].r + 0.5f;

	float OUT = 0;
	float current_strength = 0;
	if (left_type == 2)
	{
		current_strength++;
		OUT = 2;
	}
	if (right_type == 4)
	{
		current_strength++;
		OUT = 4;
	}
	if (top_type == 3)
	{
		current_strength++;
		OUT = 3;
	}
	if (bottom_type == 1)
	{
		current_strength++;
		OUT = 1;
	}

	if (current_type >= 1 && current_type <= 4 && bottom_type >= 5 && top_type >= 5 && right_type >= 5 && left_type >= 5)
	{
		if ((current_type == 1 && top_type == 5) || (current_type == 2 && right_type == 5) || (current_type == 3 && bottom_type == 5) || (current_type == 4 && left_type == 5))
		{
			current_strength++;
			OUT = current_type;
		}
	}
	if (current_type != 5 && bottom_type == 5 && left_type == 5 && right_type == 5 && top_left_type == 5 && top_right_type == 5 && ((bot_left_type >= 1 && bot_left_type <= 4) || bot_left_type >= 9) && ((bot_right_type >= 1 && bot_right_type <= 4) || bot_right_type >= 9) && top_type != 5)
	{
		OUT = 1;
		current_strength += 1;
	}
	if (current_type != 5 && top_type == 5 && left_type == 5 && right_type == 5 && bot_left_type == 5 && bot_right_type == 5 && ((top_left_type >= 1 && top_left_type <= 4) || top_left_type >= 9) && ((top_right_type >= 1 && top_right_type <= 4) || top_right_type >= 9) && bottom_type != 5)
	{
		OUT = 3;
		current_strength += 1;
	}
	if (current_type != 5 && top_type == 5 && right_type == 5 && bottom_type == 5 && top_left_type == 5 && bot_left_type == 5 && ((top_right_type >= 1 && top_right_type <= 4) || top_right_type >= 9) && ((bot_right_type >= 1 && bot_right_type <= 4) || bot_right_type >= 9) && left_type != 5)
	{
		OUT = 4;
		current_strength += 1;
	}
	if (current_type != 5 && top_type == 5 && left_type == 5 && bottom_type == 5 && top_right_type == 5 && bot_right_type == 5 && ((top_left_type >= 1 && top_left_type <= 4) || top_left_type >= 9) && ((bot_left_type >= 1 && bot_left_type <= 4) || bot_left_type >= 9) && right_type != 5)
	{
		OUT = 2;
		current_strength += 1;
	}

	if (current_type < 5 && bottom_type >= 9 && left_type == 5 && right_type == 5 && top_left_type == 5 && top_right_type == 5 && top_type != 5)
	{
		OUT = 1;
		current_strength += 1;
	}
	if (current_type < 5 && top_type >= 9 && left_type == 5 && right_type == 5 && bot_left_type == 5 && bot_right_type == 5 && bottom_type != 5)
	{
		OUT = 3;
		current_strength += 1;
	}
	if (current_type < 5 && right_type >= 9 && bottom_type == 5 && top_type == 5 && top_left_type == 5 && bot_left_type == 5 && left_type != 5)
	{
		OUT = 4;
		current_strength += 1;
	}
	if (current_type < 5 && left_type >= 9 && bottom_type == 5 && top_type == 5 && top_right_type == 5 && bot_right_type == 5 && right_type != 5)
	{
		OUT = 2;
		current_strength += 1;
	}

	

	if (current_type == 5) // Multifunctionblock
	{
		current_strength = 1;
		OUT = 5;
	}
	else if (current_type == 6) // Multifunctionblock
	{
		current_strength = 1;
		OUT = 6;
	}
	else if (current_type >= 7 && current_type <= 10)
	{
		// 7: without energie
		// 8: no energie impuls
		// 9: with energie
		// 10: energie impuls
		int counter = 0;
		if (top_type == 3)
			counter++;
		if (bottom_type == 1)
			counter++;
		if (right_type == 4)
			counter++;
		if (left_type == 2)
			counter++;
		if (counter > 1)
		{
			OUT = 10; // energie impuls
			current_strength = 1;
		}
		else if (counter == 1)
		{
			OUT = 8; // Without Energy
			current_strength = 1;
		}
		else if (current_type == 7) // no energie
		{
			if (right_type == 10 || left_type == 10 || bottom_type == 10 || top_type == 10)
			{
				OUT = 10;
				current_strength = 1;
			}
			else
			{
				OUT = current_type;
				current_strength = 1;
			}
		}
		else if (current_type == 9) // with energie
		{
			if (right_type == 8 || left_type == 8 || bottom_type == 8 || top_type == 8)
			{
				OUT = 8;
				current_strength = 1;
			}
			else
			{
				OUT = current_type;
				current_strength = 1;
			}
		}
		else if (current_type == 8)
		{
			OUT = 7;
			current_strength = 1;
		}
		else if (current_type == 10)
		{
			OUT = 9;
			current_strength = 1;
		}
		else
		{
			OUT = current_type;
			current_strength = 1;
		}
		
	}
	else
	{
		if (bottom_type == 6)
		{
			uint botx2_type = SpriteTexture[uint2(xcoo, ycoo + 2)].r + 0.5f;
			if (botx2_type == 1)
			{
				current_strength += 1;
				OUT = 1;
			}
		}
		if (top_type == 6)
		{
			uint topx2_type = SpriteTexture[uint2(xcoo, ycoo - 2)].r + 0.5f;
			if (topx2_type == 3)
			{
				current_strength += 1;
				OUT = 3;
			}
		}
		if (left_type == 6)
		{
			uint leftx2_type = SpriteTexture[uint2(xcoo - 2, ycoo)].r + 0.5f;
			if (leftx2_type == 2)
			{
				current_strength += 1;
				OUT = 2;
			}
		}
		if (right_type == 6)
		{
			uint rightx2_type = SpriteTexture[uint2(xcoo + 2, ycoo)].r + 0.5f;
			if (rightx2_type == 4)
			{
				current_strength += 1;
				OUT = 4;
			}
		}
	}

	if (current_strength > 1.5f)
		OUT = 0;
	return OUT;
}
float prelogic(VertexShaderOutput input) : COLOR
{
	float OUT = 0;

	uint xcoo = input.TextureCoordinates.x * worldsizex;
	uint ycoo = input.TextureCoordinates.y * worldsizey;

	uint left = xcoo - 1;
	if (xcoo == 0)
		left = worldsizex - 1;
	uint right = (xcoo + 1) % (uint)worldsizex;
	uint top = ycoo - 1;
	if (ycoo == 0)
		top = worldsizey - 1;
	uint bottom = (ycoo + 1) % (uint)worldsizey;

	uint left_type = SpriteTexture[uint2(left, ycoo)].r + 0.5f;
	uint right_type = SpriteTexture[uint2(right, ycoo)].r + 0.5f;
	uint top_type = SpriteTexture[uint2(xcoo, top)].r + 0.5f;
	uint bottom_type = SpriteTexture[uint2(xcoo, bottom)].r + 0.5f;
	uint current_type = SpriteTexture[uint2(xcoo, ycoo)].r + 0.5f;

	uint bot_left_type = SpriteTexture[uint2(left, bottom)].r + 0.5f;
	uint bot_right_type = SpriteTexture[uint2(right, bottom)].r + 0.5f;
	uint top_left_type = SpriteTexture[uint2(left, top)].r + 0.5f;
	uint top_right_type = SpriteTexture[uint2(right, top)].r + 0.5f;

	OUT = current_type;

	if (current_type == 2 && right_type == 5 && bottom_type == 5 && top_type != 5)
		OUT = 1;
	if (current_type == 3 && right_type == 5 && bottom_type == 5 && left_type != 5)
		OUT = 4;

	if (current_type == 2 && right_type == 5 && top_type == 5 && bottom_type != 5)
		OUT = 3;
	if (current_type == 1 && right_type == 5 && top_type == 5 && left_type != 5)
		OUT = 4;

	if (current_type == 4 && left_type == 5 && top_type == 5 && bottom_type != 5)
		OUT = 3;
	if (current_type == 1 && left_type == 5 && top_type == 5 && right_type != 5)
		OUT = 2;

	if (current_type == 3 && left_type == 5 && bottom_type == 5 && right_type != 5)
		OUT = 2;
	if (current_type == 4 && left_type == 5 && bottom_type == 5 && top_type != 5)
		OUT = 1;






	if (current_type == 2 && right_type == 5 && bottom_type == 6 && top_type != 5 && top_type != 6)
		OUT = 1;
	if (current_type == 3 && right_type == 6 && bottom_type == 5 && left_type != 5 && left_type != 6)
		OUT = 4;

	if (current_type == 2 && right_type == 5 && top_type == 6 && bottom_type != 5 && bottom_type != 6)
		OUT = 3;
	if (current_type == 1 && right_type == 6 && top_type == 5 && left_type != 5 && left_type != 6)
		OUT = 4;

	if (current_type == 4 && left_type == 5 && top_type == 6 && bottom_type != 5 && bottom_type != 6)
		OUT = 3;
	if (current_type == 1 && left_type == 6 && top_type == 5 && right_type != 5 && right_type != 6)
		OUT = 2;

	if (current_type == 3 && left_type == 6 && bottom_type == 5 && right_type != 5 && right_type != 6)
		OUT = 2;
	if (current_type == 4 && left_type == 5 && bottom_type == 6 && top_type != 5 && top_type != 6)
		OUT = 1;

	/*if (current_type >= 7 && current_type <= 8 && top_left_type == 5 && top_right_type == 5 && bot_left_type < 5 && bot_right_type < 5)
	{
		if (right_type >= 1 && right_type <= 4)
			OUT = 7;
		else if (left_type >= 1 && left_type <= 4)
			OUT = 8;
	}
	if (current_type >= 7 && current_type <= 8 && top_left_type < 5 && top_right_type == 5 && bot_left_type < 5 && bot_right_type == 5)
	{
		if (bottom_type >= 1 && bottom_type <= 4)
			OUT = 7;
		else if (top_type >= 1 && top_type <= 4)
			OUT = 8;
	}
	if (current_type >= 7 && current_type <= 8 && top_left_type < 5 && top_right_type < 5 && bot_left_type == 5 && bot_right_type == 5)
	{
		if (left_type >= 1 && left_type <= 4)
			OUT = 7;
		else if (right_type >= 1 && right_type <= 4)
			OUT = 8;
	}
	if (current_type >= 7 && current_type <= 8 && top_left_type == 5 && top_right_type < 5 && bot_left_type == 5 && bot_right_type < 5)
	{
		if (top_type >= 1 && top_type <= 4)
			OUT = 7;
		else if (bottom_type >= 1 && bottom_type <= 4)
			OUT = 8;
	}*/


	return OUT;
}

technique pre_logic
{
	pass P1
	{
		PixelShader = compile PS_SHADERMODEL prelogic();
	}
};
technique main_logic
{
	pass P2
	{
		PixelShader = compile PS_SHADERMODEL mainlogic();
	}
};