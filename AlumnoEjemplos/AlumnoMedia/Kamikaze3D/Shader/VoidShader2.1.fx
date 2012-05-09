/*
* VOID
* Shader a fines de generar iluminaciones del DeLorean y de los modelos necesarios aplicando el algoritmo Phong
*/

//-------------------------------------------------------------------//
// Vertex Shader
//-------------------------------------------------------------------//

//Variables utilizadas por el Vertex Shader

//Posicion del ojo y de la fuente de luz para el Phong
float3 fvEyePosition = float3( -100.00, 4.00, 4.00 );
float3 fvLightPosition = float3( 0.00, 100.00, 0.00 );

//Matrices de Posicionamiento
float4x4 matView;
float4x4 matProjection;
float4x4 matWorld;
//Matriz utilizada en la tecnica por defecto
float4x4 matViewProjection;

//Input General para TgcMeshShader del Vertex Shader
struct VS_INPUT 
{
   float4 Position : POSITION0;
   float3 Normal :   NORMAL0;
   float4 Color : COLOR;
   float2 Texcoord : TEXCOORD0;
};

//Output General para TgcMeshShader del Vertex Shader
struct VS_OUTPUT 
{
   float4 Position :        POSITION0;
   float2 Texcoord :        TEXCOORD0;
   float3 ViewDirection :   TEXCOORD1;
   float3 LightDirection :  TEXCOORD2;
   float3 Normal :          TEXCOORD3;
};

/*
* MAIN: Void Vertex Shader Full Transformation
*/

VS_OUTPUT full_vs_main (VS_INPUT Input)
{
   VS_OUTPUT Output;

   //Vertex Position -> World position -> Relativa a la camara -> Projection
   Output.Position         = mul(mul(mul(Input.Position, matWorld),matView),matProjection);
   Output.Texcoord         = Input.Texcoord;
   
   float3 fvObjectPosition = mul( Input.Position, matWorld);
   
   Output.ViewDirection    = fvEyePosition - fvObjectPosition;
   Output.LightDirection   = fvLightPosition - fvObjectPosition;
   
   //Normal projection
   Output.Normal           = mul( Input.Normal, matView );
      
   return( Output );
}

/*
* MAIN: Default Vertex Shader
*/

VS_OUTPUT vs_main (VS_INPUT Input)
{
   VS_OUTPUT Output;
   
   matViewProjection = mul( matView, matProjection );

   //Proyectar posicion
   Output.Position         = mul( Input.Position, matViewProjection );
   
   //Las Texcoord quedan igual
   Output.Texcoord         = Input.Texcoord;
   
   //Obtener direccion a la que mira la camara y direccion de la luz
   float3 fvObjectPosition = mul( Input.Position, matView );
   Output.ViewDirection    = fvEyePosition - fvObjectPosition;
   Output.LightDirection   = fvLightPosition - fvObjectPosition;
   
   //Proyectar normal
   Output.Normal           = mul( Input.Normal, matView );
      
   return( Output );
}

//-------------------------------------------------------------------//
// Pixel Shader
//-------------------------------------------------------------------//

//Variables utilizadas por el Pixel Shader
float4 fvAmbient = float4( 0.37, 0.38, 0.37, 1.00 );
float4 fvDiffuse = float4( 0.89, 0.89, 0.33, 1.00 );
float4 fvSpecular = float4( 0.49, 1.00, 0.49, 1.00 );
float fSpecularPower = float( 16.84 );

//Input del Pixel Shader
struct PS_INPUT 
{
   float2 Texcoord :        TEXCOORD0;
   float3 ViewDirection :   TEXCOORD1;
   float3 LightDirection:   TEXCOORD2;
   float3 Normal :          TEXCOORD3;
   float4 Pos : 			TEXCOORD4;
};

//Textura utilizada por el Pixel Shader
texture base_Tex;
sampler2D baseMap = sampler_state
{
   Texture = (base_Tex);
   ADDRESSU = WRAP;
   ADDRESSV = WRAP;
   MINFILTER = LINEAR;
   MAGFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

// MAIN: Pixel Shader
float4 ps_main( PS_INPUT Input ) : COLOR0
{      
	//Aplicar algoritmo de Diffuse Light
   float3 fvLightDirection = normalize( Input.LightDirection );
   float3 fvNormal         = normalize( Input.Normal );
   float  fNDotL           = dot( fvNormal, fvLightDirection ); 
   
   //Aplicar algoritmo de Specular Light
   float3 fvReflection     = normalize( ( ( 2.0f * fvNormal ) * ( fNDotL ) ) - fvLightDirection ); 
   float3 fvViewDirection  = normalize( Input.ViewDirection );
   float  fRDotV           = max( 0.0f, dot( fvReflection, fvViewDirection ) );
   
   //Obtener el texel de textura
   float4 fvBaseColor      = tex2D( baseMap, Input.Texcoord );
   
   //Sumar Ambient + Diffuse + Specular
   float4 fvTotalAmbient   = fvAmbient * fvBaseColor; 
   float4 fvTotalDiffuse   = fvDiffuse * fNDotL * fvBaseColor; 
   float4 fvTotalSpecular  = fvSpecular * pow( fRDotV, fSpecularPower );
   
   //Saturar para no pasarse del máximo color
   return( saturate( fvTotalAmbient + fvTotalDiffuse + fvTotalSpecular ) );
}

//-------------------------------------------------------------------//
// Techniques
//-------------------------------------------------------------------//

technique VoidFullTransformationTechnique
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 full_vs_main();
	  PixelShader = compile ps_2_0 ps_main();
   }
}

technique DefaultTechnique
{
   pass Pass0
   {
      VertexShader = compile vs_2_0 vs_main();
      PixelShader = compile ps_2_0 ps_main();
   }
}



