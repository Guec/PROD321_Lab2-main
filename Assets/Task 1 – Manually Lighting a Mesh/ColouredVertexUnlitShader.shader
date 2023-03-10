/* This vertex/fragment shader sets the output fragment colour as the colour defined
 * in the vertex colours.
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

Shader "Custom/ColouredVertexUnlitShader"
{
    Properties {}
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 colour : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 colour : COLOR;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.colour = v.colour;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.colour;
            }
            ENDCG
        }
    }
}
