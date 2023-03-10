// This BRDF shader was downloaded from
// https://alastaira.wordpress.com/2013/11/26/lighting-models-and-brdf-maps/

Shader "Custom/BRDF Ramp" {
  Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _BRDF ("BRDF Ramp", 2D) = "gray" {}
  }
  SubShader {
    Tags { "RenderType" = "Opaque" }
    CGPROGRAM
    #pragma surface surf Ramp
 
    sampler2D _BRDF;
 
    half4 LightingRamp (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
         
        // Calculate dot product of light direction and surface normal
        // 1.0 = facing each other perfectly
        // 0.0 = right angles
        // -1.0 = parallel, facing same direction
        half NdotL = dot (s.Normal, lightDir);
         
        // NdotL lies in the range between -1.0 and 1.0
        // To use as a texture lookup we need to adjust to lie in the range 0.0 to 1.0
        // We could simply clamp it, but instead we'll apply softer "half" lighting
        // (which Unity calls "Diffuse Wrap")
        NdotL = NdotL * 0.5 + 0.5;
         
        // Calculate dot product of view direction and surface normal
        // Note that, since we only render front-facing normals, this will
        // always be positive
        half NdotV = dot(s.Normal, viewDir);
         
        // Lookup the corresponding colour from the BRDF texture map
        half3 brdf = tex2D (_BRDF, float2(NdotL, NdotV)).rgb;
         
        half4 c;
         
        // For illustrative purpsoes, let's set the pixel colour based entirely on the BRDF texture
        // In practice, you'd normally also have Albedo and lightcolour terms here too. 
        c.rgb = brdf * (atten * 2);
        c.a = s.Alpha;
        return c;
    }
 
    struct Input {
        float2 uv_MainTex;
    };
    sampler2D _MainTex;
    void surf (Input IN, inout SurfaceOutput o) {
        o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
    }
    ENDCG
  }
  Fallback "Diffuse"
}