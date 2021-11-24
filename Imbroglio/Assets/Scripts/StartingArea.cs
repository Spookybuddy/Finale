using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingArea : MonoBehaviour
{
    private int width = 128;
    private int depth = 128;

    private float height;

    private TerrainCode caveSystem;
    private float perlinOffX;
    private float perlinOffY;

    public List<GameObject> tree;

    void Start()
    {
        caveSystem = GameObject.Find("Cave Ground").GetComponent<TerrainCode>();

        height = caveSystem.height;
        perlinOffX = caveSystem.ranX;
        perlinOffY = caveSystem.ranY;

        Terrain enter = GetComponent<Terrain>();
        enter.terrainData.heightmapResolution = width;
        enter.terrainData.size = new Vector3(width, height, depth);

        //Generate walls and trees for starting area
        float[,] test = new float[width, depth];
        for (int a = 0; a < width; a++) {
            for (int b = 0; b < depth; b++) {
                test[a, b] = borderGen(a, b);
            }
        }
        enter.terrainData.SetHeights(0, 0, test);

        transform.position = new Vector3(transform.position.x, 0, -depth/2);
    }

    private float borderGen(int x, int y)
    {
        //Use perlin noise to generate 'Trees' as a natural wall
        float perlin = Mathf.PerlinNoise((float)x/width*15 + perlinOffX, (float)y/depth*15 + perlinOffY);
        float value = 0.0f;

        //Trees get thicker towards the edges
        if (y > depth/2) {
            //Use perlin, the x, and the y values to make a random forest
            if (perlin/10 + ((float)y)/64 + Mathf.Clamp(Mathf.Abs(x - 63.5f)/10 - 5.35f, -0.75f, 1.5f) > 0.95f && Random.Range(0, 20) == 0) {
                fillForest(perlin, x, y, false);
            }
            value = 0.0f;
        }

        //Spawn trees outside the play area to make it look like there's more to the world
        if (y > depth/4) {
            if ((perlin > 0.6f && perlin < 0.615f) || (perlin > 0.3f && perlin < 0.315f)) {
                fillForest(perlin, x, y, true);
            }
        }

        //Left & right walls
        if ((x < 16 || x > width - 17) && y < depth/2) {
            value = Mathf.Clamp01(Mathf.Round(perlin + 2/(y+1) - 0.1f + Mathf.Clamp(Mathf.Abs(x - 63.5f)/10 - 5.35f, -0.75f, 1.5f)));
        } 

        return value;
    }

    //Spawn trees formula
    private void fillForest(float perlin, int x, int y, bool isOut)
    {
        //Random rotation for trees
        Quaternion rotated = new Quaternion(0, 0, 0, 0);
        rotated.eulerAngles = new Vector3(0, perlin * 360, 0);

        //Instantiate trees as a child of forest
        if (isOut) {
            //Trees in the out of bounds area
            if (Mathf.Abs(((float)x - 63.5f)*1.5f) > 64 || y*1.2f - 2 > 128) {
                GameObject sapling = Instantiate(tree[Random.Range(0, tree.Count)], new Vector3(y*1.2f - 2, 0, ((float)x - 63.5f)*1.5f), rotated) as GameObject;
                sapling.transform.parent = GameObject.Find("Forest").transform;
            }
        } else {
            GameObject sapling = Instantiate(tree[Random.Range(0, tree.Count)], new Vector3(y - 1, 0, (float)x - 63.5f), rotated) as GameObject;
            sapling.transform.parent = GameObject.Find("Forest").transform;
        }
        
    }
}