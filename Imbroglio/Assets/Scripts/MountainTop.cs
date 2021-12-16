using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainTop : MonoBehaviour
{
    private int width = 128;
    private int depth = 128;
    private int height = 16;

    private float randX;
    private float randY;

    private float scale = 5.0f;

    void Start()
    {
        randX = Random.Range(0f, 100f);
        randY = Random.Range(0f, 100f);

        //Get terrain and modify it to make a mountain peak
        TerrainData cave = GetComponent<Terrain>().terrainData;
        cave.heightmapResolution = width;
        cave.size = new Vector3(width, height, depth);

        float[,] peaks = new float[width, depth];
        for (int i=0; i < width; i++) {
            for (int j=0; j<depth; j++) {
                peaks[i, j] = Mathf.PerlinNoise(randX + (float)i/width*scale, randY + (float)j/depth*scale) + (-Mathf.Abs((float)i-63.5f)/50+1) + (-Mathf.Abs((float)j-95.5f)/10+3f);
            }
        }
        cave.SetHeights(0, 0, peaks);
    }
}