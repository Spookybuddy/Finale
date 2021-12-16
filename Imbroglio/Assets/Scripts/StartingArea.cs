using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingArea : MonoBehaviour
{
    private int width = 128;
    private int depth = 128;

    private float height;

    private TerrainCode caveSystem;
    private TerrainData enter;
    private float perlinOffX;
    private float perlinOffY;

    public GameObject tree;

    void Start()
    {
        caveSystem = GameObject.Find("Cave Ground").GetComponent<TerrainCode>();
        height = caveSystem.height;

        //Set dimensions
        enter = GetComponent<Terrain>().terrainData;
        enter.heightmapResolution = width;
        enter.size = new Vector3(width, height, depth);
        transform.position = new Vector3(-1, 0.01f, -depth/2);

        //Random offset
        perlinOffX = Random.Range(0, 100);
        perlinOffY = Random.Range(0, 100);

        //Generate walls and trees for starting area
        float[,] test = new float[width, depth];
        float[,,] splatmap = new float[enter.alphamapWidth, enter.alphamapHeight, enter.alphamapLayers];
        for (int a = 0; a < width; a++) {
            for (int b = 0; b < depth; b++) {
                test[a, b] = borderGen(a, b);
                //Color the environment
                for (int c = 0; c < enter.alphamapLayers; c++) {
                    splatmap[a, b, c] = paintGen(a, b, c);
                }
            }
        }
        enter.SetHeights(0, 0, test);
        enter.SetAlphamaps(0, 0, splatmap);
    }

    private float borderGen(int x, int y)
    {
        //Use perlin noise to generate 'Trees' as a natural wall
        float perlin = Mathf.PerlinNoise((float)x/width*15.0f + perlinOffX, (float)y/depth*15.0f + perlinOffY);
        float value = 0.0f;

        //Trees get thicker towards the edges
        if (y > depth/2) {
            //1/30 tree chance
            if (perlin/10 + ((float)y)/64 + Mathf.Clamp(Mathf.Abs((float)x - 63.5f)/10 - 5.35f, -0.75f, 1.5f) > 0.95f && Random.Range(0, 30) == 0) {
                fillForest(perlin, x, y, false);
            }
            value = 0.0f;
        }

        //Spawn trees outside the play area to make it look like there's more to the world
        if (y > depth/4) {
            //Spawn trees outside if perlin is within bounds
            if ((perlin > 0.6f && perlin < 0.615f) || (perlin > 0.3f && perlin < 0.315f)) {
                fillForest(perlin, x, y, true);
            }
        }

        //Left & right walls
        if ((x < 16 || x > width - 17) && y < depth/2) {
            value = Mathf.Clamp01(Mathf.Round(perlin/2 + ((-(float)y/48.0f) + 0.6f) + Mathf.Clamp(Mathf.Abs((float)x - 63.5f)/10 - 5.35f, -0.5f, 1.5f)));
        }
        return value;
    }

    //Paint the starting area
    private float paintGen(int x, int y, int z)
    {
        float perlin = Mathf.PerlinNoise((float)x/width * 15 + perlinOffX, (float)y/depth * 15 + perlinOffY);
        float alpha = 1.0f;

        //Assign alpha values based on perlin noise to get a smoother transition
        //Layer 0 = Ground, Layer 1 = Walls
        if (z == 1) {
            alpha = Mathf.Clamp01(perlin/1.428f + (5/((float)y+1)) + ((-(float)y/32.0f) + 1.0f) + (Mathf.Abs((float)x - 63.5f)/10 - 4.85f));
        } else {
            alpha = 1.0f - Mathf.Clamp01(perlin/1.428f + (5/((float)y+1)) + ((-(float)y/32.0f) + 1.0f) + (Mathf.Abs((float)x - 63.5f)/10 - 4.85f));
        }
        return alpha;
    }

    //Spawn trees formula
    private void fillForest(float perlin, int x, int y, bool isOut)
    {
        //Random rotation for trees
        Quaternion rotated = new Quaternion(0, 0, 0, 0);
        rotated.eulerAngles = Vector3.up * perlin * 360;
        Vector3 location = new Vector3(y-1, 0, (float)x-63.5f);

        //Move trees to out of bounds
        if (isOut && (Mathf.Abs(((float)x - 63.5f) * 1.55f) > 64 || y * 1.25f - 2 > 128)) {
            location = Vector3.Scale(location, new Vector3(1.25f, 0, 1.55f));
            treeChild(location, rotated);
        } else if (!isOut) {
            treeChild(location, rotated);
        }
    }

    //Spawn trees and make a child of the forest
    private void treeChild(Vector3 location, Quaternion rotation)
    {
        GameObject sapling = Instantiate(tree, location, rotation) as GameObject;
        sapling.transform.parent = GameObject.Find("Forest").transform;
    }
}