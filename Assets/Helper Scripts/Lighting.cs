using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* This helper class includes a lighting functions for calculating Lambert,
 * Phong and BRDF lighting. All these functions are static, so can be called as:
 * 
 * Lighting.CalculateLambertLightingColours
 * Lighting.CalculatePhongLightingColours
 * Lighting.CalculateBRDFLookupLightingColours
 * 
 * The function declarations differ depending on the lighting model, but
 * generally take some vertices and vertex normals in model space, some information
 * about the environment, and return an array of colours which correspond to
 * the vertex colours (with the array indicies of colours corresponding to
 * the vertex with the same index).
 * 
 * Since we may use the same vertex for multiple triangles, it's faster
 * to computer the colours of all vertices at once, to save us computing
 * the colour of the same vertices multiple times for each triangle
 *
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class Lighting
{
    // This function calculates the colours of a range of vertices using Lambert
    // shading, based on the vertex normals, light source, and 
    // material diffuse colour
    public static Color[] CalculateLambertLightingColours(Color diffuseColour, Light lightSource, Transform meshTransform, Vector3[] verts, Vector3[] normals)
    {
        // This array will be used to store the colours for the vertices
        // after they are lit
        Color[] colours = new Color[verts.Length];

        // Calculate the light position and colour based on the light properties
        Vector3 lightPos; Color lightColour;
        if (lightSource.type == LightType.Directional)
        {
            lightPos = -lightSource.transform.forward;
            lightColour = lightSource.color * lightSource.intensity;
        }
        else
        {
            lightPos = lightSource.transform.position;
            // For non directional lights, we'll figure out the light
            // Colour per vertex, based on the distance from the light
            // To the vertex
            lightColour = Color.black;

        }

        // Loop through each vertex
        for (int i = 0; i < verts.Length; i++)
        {
            // Calculate the vertices world position and world normal
            //o.posWorld = mul(unity_ObjectToWorld, v.vertex); //Calculate the world position for our point
            //o.normal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz); //Calculate the normal
            Vector3 vertexWorldPosition = meshTransform.TransformPoint(verts[i]);
            Vector3 normalWorldDirection = meshTransform.TransformVector(normals[i]);

            // Normalize the normal direction
            //float3 normalDirection = normalize(i.normal);
            Vector3 normalDirection = normalWorldDirection.normalized;

            // Calculate light direction
            //float3 lightDirection = normalize( _WorldSpaceLightPos0.xyz);
            Vector3 lightDirection = (lightPos).normalized;

            // if our light source is not directional
            if (lightSource.type != LightType.Directional)
            {
                // Calculate the colour based on the distance and lightsource range
                if (Vector3.Distance(vertexWorldPosition, lightPos) < lightSource.range)
                    lightColour = (lightSource.color * lightSource.intensity);
            }

            // Assume light attenuation is 1.0
            //float attenuation = 1.0;
            float attenuation = 1.0f;

            // Calculate Ambient Lighting - the standard Lambert model assumes
            // Not ambient lighting, but we can add it if we want by multiplying
            // The Rendersettings.ambientLight by the diffuse colour
            //float3 ambientLighting = 0;//UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb; //Ambient component
            Color ambientLighting = Color.black;// RenderSettings.ambientLight * _Color;
            //Color ambientLighting = RenderSettings.ambientLight * DiffuseColor;

            // Calculate Diffuse Lighting
            //float3 diffuseReflection = attenuation * _LightColor0.rgb * _Color.rgb * max(0.0, dot(normalDirection, lightDirection)); //Diffuse component
            Color diffuseReflection = attenuation * lightColour * diffuseColour * Mathf.Max(0, Vector3.Dot(normalDirection, lightDirection));

            // The final colour for this vertex is the sum of the ambient, diffuse and specular
            //float3 color = (ambientLighting + diffuseReflection) * tex2D(_Tex, i.uv);
            colours[i] = (ambientLighting + diffuseReflection);
        }

        return colours;
    }

    // This function calculates the colours of a range of vertices using Phong
    // shading, based on the vertex normals, camera world pos, light source, and 
    // material diffuse colour, specular colour and shininess
    public static Color[] CalculatePhongLightingColours(Color diffuseColour, Color specularColour, float Shininess, Vector3 cameraWorldPos, Light lightSource, Transform meshTransform, Vector3[] verts, Vector3[] normals)
    {
        // Create a new array to keep track of all the vertex colours
        Color[] colours = new Color[verts.Length];

        // Calculate the light position and colour based on the light properties
        // LightW is set to 0 if the light is directional, and 1 otherwise
        float lightW; Vector3 lightPos; Color lightColour;
        float attenuation = 0;
        if (lightSource.type == LightType.Directional)
        {
            lightPos = -lightSource.transform.forward; lightW = 0;
            lightColour = lightSource.color * lightSource.intensity;
            attenuation = 1.0f;
        }
        else
        {
            lightPos = lightSource.transform.position; lightW = 1;
            // For non directional lights, we'll figure out the light
            // Colour per vertex, based on the distance from the light
            // To the vertex
            lightColour = Color.black;
        }

        // Loop through each vertex
        for (int i = 0; i < verts.Length; i++)
        {

            // Calculate the vertices world position and world normal
            //o.posWorld = mul(unity_ObjectToWorld, v.vertex); //Calculate the world position for our point
            //o.normal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz); //Calculate the normal
            Vector3 vertexWorldPosition = meshTransform.TransformPoint(verts[i]);
            Vector3 normalWorldDirection = meshTransform.TransformVector(normals[i]);

            // Normalize the normal direction
            //float3 normalDirection = normalize(i.normal);
            Vector3 normalDirection = normalWorldDirection.normalized;

            // Calculate the normalized view direction (the camera position -
            // the vertex world position) 
            //float3 viewDirection = normalize(_WorldSpaceCameraPos - i.posWorld.xyz);
            Vector3 viewDirection = (cameraWorldPos - vertexWorldPosition).normalized;

            // if our light source is not directional
            if (lightSource.type != LightType.Directional)
            {
                // Calculate the distance from the light to the vertex, and 1/distance
                //float3 vert2LightSource = _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
                Vector3 vert2LightSource = lightPos - vertexWorldPosition;
                //float oneOverDistance = 1.0 / length(vert2LightSource);
                //float attenuation = lerp(1.0, oneOverDistance, _WorldSpaceLightPos0.w); //Optimization for spot lights. This isn't needed if you're just getting started.
                attenuation = 1.0f / vert2LightSource.magnitude;

                // Calculate the colour based on the distance and lightsource range
                if (vert2LightSource.magnitude < lightSource.range)
                    lightColour = (lightSource.color * lightSource.intensity);
            }

            // Calculate light direction
            //float3 lightDirection = _WorldSpaceLightPos0.xyz - i.posWorld.xyz * _WorldSpaceLightPos0.w;
            Vector3 lightDirection = lightPos - vertexWorldPosition * lightW;

            // Calculate Ambient Lighting
            //float3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb; //Ambient component
            Color ambientLighting = RenderSettings.ambientLight * diffuseColour;

            // Calculate Diffuse Lighting
            //float3 diffuseReflection = attenuation * _LightColor0.rgb * _Color.rgb * max(0.0, dot(normalDirection, lightDirection)); //Diffuse component
            Color diffuseReflection = attenuation * lightColour * diffuseColour * Mathf.Max(0, Vector3.Dot(normalDirection, lightDirection));

            /*float3 specularReflection;
            if (dot(i.normal, lightDirection) < 0.0) //Light on the wrong side - no specular
            {
                specularReflection = float3(0.0, 0.0, 0.0);
            }
            else
            {
                //Specular component
                specularReflection = attenuation * _LightColor0.rgb * _SpecColor.rgb * pow(max(0.0, dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
            }*/

            // Calculate Specular reflection if the normal is pointing in the
            // Lights direction
            Color specularReflection;
            if (Vector3.Dot(normalWorldDirection, lightDirection) < 0.0)
            {
                specularReflection = Color.black;
            }
            else
            {
                specularReflection = attenuation * lightColour * specularColour * Mathf.Pow(Mathf.Max(0.0f, Vector3.Dot(Vector3.Reflect(-lightDirection, normalDirection), viewDirection)), Shininess);
            }

            // The final colour for this vertex is the sum of the ambient, diffuse and specular
            //float3 color = (ambientLighting + diffuseReflection) * tex2D(_Tex, i.uv) + specularReflection; //Texture is not applient on specularReflection
            colours[i] = (ambientLighting + diffuseReflection) + specularReflection;
        }

        return colours;
    }

    // This function calculates the colours of a range of vertices using a BRDF
    // lookup table, based on the vertex normals, light source, and camera position
    public static Color[] CalculateBRDFLookupLightingColours(Texture2D lookupTexture, float atten, Vector3 cameraWorldPos, Light lightSource, Transform meshTransform, Vector3[] verts, Vector3[] normals)
    {
        // Create a new array to keep track of all the vertex colours
        Color[] colours = new Color[verts.Length];

        //Directional lights: (world space direction, 0). Other lights: (world space position, 1).
        Vector3 lightPos;
        if (lightSource.type == LightType.Directional)
            lightPos = -lightSource.transform.forward;
        else
            lightPos = lightSource.transform.position;

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 vPosWorld = meshTransform.TransformPoint(verts[i]);
            Vector3 normal = meshTransform.TransformVector(normals[i]);
            Vector3 normalDirection = normal.normalized;
            Vector3 viewDirection = (cameraWorldPos- vPosWorld).normalized;

            // Calculate dot product of light direction and surface normal
            // 1.0 = facing each other perfectly
            // 0.0 = right angles
            // -1.0 = parallel, facing same direction
            float NdotL = Vector3.Dot(normalDirection, lightPos);

            // NdotL lies in the range between -1.0 and 1.0
            // To use as a texture lookup we need to adjust to lie in the range 0.0 to 1.0
            // We could simply clamp it, but instead we'll apply softer "half" lighting
            // (which Unity calls "Diffuse Wrap")
            NdotL = NdotL * 0.5f + 0.5f;

            // Calculate dot product of view direction and surface normal
            // Note that, since we only render front-facing normals, this will
            // always be positive
            float NdotV = Vector3.Dot(normalDirection, viewDirection);

            // Lookup the corresponding colour from the BRDF texture map
            Color brdf = lookupTexture.GetPixelBilinear(NdotL, NdotV);

            // For illustrative purpsoes, let's set the pixel colour based entirely on the BRDF texture
            // In practice, you'd normally also have Albedo and lightcolour terms here too. 
            colours[i] = brdf * atten * 2;
        }

        return colours;
    }

}
