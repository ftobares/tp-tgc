// Sombras en el image space con la tecnica de Shadows Map
#define SMAP_SIZE 1024
#define EPSILON 0.05f

float time = 0;

//Colores
float k_la = 0.3;							// luz ambiente global
float k_ld = 0.9;							// luz difusa
float k_ls = 0.4;							// luz specular
float fSpecularPower = 16.84;				// exponente de la luz specular

// Transformaciones
float4x4 matWorld;
float4x4 matWorldView;
float4x4 matWorldViewProj;
float4x4 matWorldInverseTranspose;

float4x4 g_mViewLightProj;
float4x4 g_mProjLight;
float3   g_vLightPos;  // posicion de la luz (en World Space) = pto que representa patch emisor Bj 
float3   g_vLightDir;  // Direcion de la luz (en World Space) = normal al patch Bj

// Textura basica:
texture base_Tex;
sampler2D baseMap =
sampler_state
{
   Texture = (base_Tex);
   MINFILTER = LINEAR;
   MAGFILTER = LINEAR;
   MIPFILTER = LINEAR;
};


texture  g_txShadow;	// textura para el shadow map
sampler2D g_samShadow =
sampler_state
{
    Texture = <g_txShadow>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};


//Output del Vertex Shader
struct VS_OUTPUT 
{
   float4 Position :        POSITION0;
   float2 Texcoord :        TEXCOORD0;
   float3 Norm :			TEXCOORD1;		// Normales
   float3 Pos :   			TEXCOORD2;		// Posicion real 3d
};

//-----------------------------------------------------------------------------
// Vertex Shader que implementa un shadow map
//-----------------------------------------------------------------------------
void VertShadow( float4 Pos : POSITION,
                 float3 Normal : NORMAL,
                 out float4 oPos : POSITION,
                 out float2 Depth : TEXCOORD0 )
{
	// transformacion estandard 
    oPos = mul( Pos, matWorld);					// uso el del mesh
    oPos = mul( oPos, g_mViewLightProj );		// pero visto desde la pos. de la luz
    
    // devuelvo: profundidad = z/w 
    Depth.xy = oPos.zw;
}

//-----------------------------------------------------------------------------
// Pixel Shader para el shadow map, dibuja la "profundidad" 
//-----------------------------------------------------------------------------
void PixShadow( float2 Depth : TEXCOORD0,out float4 Color : COLOR )
{
	// parche para ver el shadow map
	//float k = Depth.x/Depth.y;
	//Color = (1-k);
    Color = Depth.x/Depth.y;

}

technique RenderShadow
{
    pass p0
    {
        VertexShader = compile vs_3_0 VertShadow();
        PixelShader = compile ps_3_0 PixShadow();
    }
}


//-----------------------------------------------------------------------------
// Vertex Shader para dibujar la escena pp dicha con sombras
//-----------------------------------------------------------------------------
void VertScene( float4 iPos : POSITION,
                float2 iTex : TEXCOORD0,
                float3 iNormal : NORMAL,
				out float4 oPos : POSITION,                
                out float2 Tex : TEXCOORD0,
				out float4 vPos : TEXCOORD1,
                out float3 vNormal : TEXCOORD2,
                out float4 vPosLight : TEXCOORD3 
                )
{
    // transformo al screen space
    oPos = mul( iPos, matWorldViewProj );
        
	// propago coordenadas de textura 
    Tex = iTex;
    
	// propago la normal
    vNormal = mul( iNormal, (float3x3)matWorldView );
    
    // propago la posicion del vertice en World space
    vPos = mul( iPos, matWorld);
    // propago la posicion del vertice en el espacio de proyeccion de la luz
    vPosLight = mul( vPos, g_mViewLightProj );
	
}


//-----------------------------------------------------------------------------
// Pixel Shader para dibujar la escena
//-----------------------------------------------------------------------------
float4 PixScene(	float2 Tex : TEXCOORD0,
					float4 vPos : TEXCOORD1,
					float3 vNormal : TEXCOORD2,
					float4 vPosLight : TEXCOORD3
					):COLOR
{
    float3 vLight = normalize( float3( vPos - g_vLightPos ) );
	float cono = dot( vLight, g_vLightDir);
	float4 K = 0.0;
	float ld = 0;		// luz difusa
	float le = 0;		// luz specular
	N = normalize(N);
	if( cono > 0.7)
    {
    
    	// coordenada de textura CT
        float2 CT = 0.5 * vPosLight.xy / vPosLight.w + float2( 0.5, 0.5 );
        CT.y = 1.0f - CT.y;

		// sin ningun aa. conviene con smap size >= 512 
		float I = (tex2D( g_samShadow, CT) + EPSILON < vPosLight.z / vPosLight.w)? 0.0f: 1.0f; 
		
		float3 LD = normalize(vPosLight-float3(Pos.x,Pos.y,Pos.z));
		ld += saturate(dot(N, LD))*k_ld;
	
		// 2- calcula la reflexion specular
		float3 D = normalize(float3(Pos.x,Pos.y,Pos.z));
		float ks = saturate(dot(reflect(LD,N), D));
		ks = pow(ks,fSpecularPower);
		le += ks*k_ls;

		//Obtener el texel de textura
		float4 fvBaseColor = tex2D( baseMap, Texcoord );
			
		// suma luz diffusa, ambiente y especular
		float4 RGBColor = 0;
		RGBColor.rgb = saturate(fvBaseColor*(saturate(k_la+ld)) + le);
		
	    if( cono < 0.8)
			I*= 1-(0.8-cono)*10;
		
		K = I;
    }     
		
	//float4 color_base = tex2D( baseMap, Tex);
	//color_base.rgb *= 0.5 + 0.5*K;
	return RGBColor;	
}	

technique RenderScene
{
    pass p0
    {
        VertexShader = compile vs_3_0 VertScene();
        PixelShader = compile ps_3_0 PixScene();
    }
}
