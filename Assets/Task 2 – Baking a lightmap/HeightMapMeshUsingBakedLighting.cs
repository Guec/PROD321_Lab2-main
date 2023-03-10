using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class generates a mesh using a height map texture. The texture defines
 * the size of the mesh, and also the height of each vertex in the mesh. 
 * Additionally, a UV map is attached which will be used to texture the mesh
 *
 * You can use this to display the UV Map created in the Manual Light Baking task
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2023, University of Canterbury
 * Written by Adrian Clark
 */

public class HeightMapMeshUsingBakedLighting : MonoBehaviour
{
    // Defines the height map texture used to create the mesh and set the heights
    public Texture2D heightMapTexture;

    // Defines the uv map texture used to texture the mesh
    public Texture2D uvMapTexture;

    // Defines the height scale that we multiply the height of each vertex by
    public float heightScale = 30;

    // Start is called before the first frame update
    void Start()
    {
        // Create a list to store our vertices
        List<Vector3> vertices = new List<Vector3>();

        // Create a list to store our triangles
        List<int> triangles = new List<int>();

        // Create a list to store our UVs
        List<Vector2> uvs = new List<Vector2>();

        // Calculate the Height and Width of our mesh from the heightmap's
        // height and width 
        int height = heightMapTexture.height;
        int width = heightMapTexture.width;

        // Generate our Vertices and UVS
        // Loop through the meshes length and width
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                // Create a new vertex using the x and z positions, and get the
                // y position as the pixel from the height map texture. As it's
                // gray scale we can use any colour channel, in this case red.
                // Multiply the pixel value by the height scale to get the final
                // y value
                vertices.Add(new Vector3(x, heightMapTexture.GetPixel(x, z).r * heightScale, z));

                // The UV coordinates are normalized, so we can just divide
                // the current x and z position by the width and height of the
                // mesh
                uvs.Add(new Vector2((float)x / (float)width, (float)z / (float)height));
            }
        }

        // Generate our triangle Indicies
        // Loop through the meshes length-1 and width-1
        for (int z = 0; z < height - 1; z++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                // Multiply the Z value by the mesh width to get the number
                // of pixels in the rows, then add the value of x to get the
                // final index. Increase the values of X and Z accordingly
                // to get the neighbouring indicies
                int vTL = z * width + x;
                int vTR = z * width + x + 1;
                int vBR = (z + 1) * width + x + 1;
                int vBL = (z + 1) * width + x;

                // Create the two triangles which make each element in the quad
                // Triangle Top Left->Bottom Left->Bottom Right
                triangles.Add(vTL);
                triangles.Add(vBL);
                triangles.Add(vBR);

                // Triangle Top Left->Bottom Right->Top Right
                triangles.Add(vTL);
                triangles.Add(vBR);
                triangles.Add(vTR);
            }
        }

        // Create our mesh object
        Mesh mesh = new Mesh();

        // Assign the vertices, triangle indicies and uvs to the mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        // Use recalculate normals to calculate the vertex normals for our mesh
        mesh.RecalculateNormals();

        // Create a new mesh filter, and assign the mesh from before to it
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // Create a new renderer for our mesh, and use the standard shader
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));

        // Assign the UV mapped texture to the mesh renderers main texture
        meshRenderer.material.mainTexture = uvMapTexture;
    }

}
