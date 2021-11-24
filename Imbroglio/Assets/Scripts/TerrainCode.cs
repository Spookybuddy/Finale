using UnityEngine;

public class TerrainCode : MonoBehaviour
{
    private int width = 512;
    private int depth = 512;

    public float height;

    public float ranX;
    public float ranY;

    private float scale = 15.0f;

    void Start()
    {
        CaveGen();
    }

    void Update()
    {
        //EDITOR >>> Change cave layout
        if (Input.GetKeyDown(KeyCode.X)) {
            CaveGen();
        }
    }

    private void CaveGen()
    {
        ranX = Random.Range(0f, 100f);
        ranY = Random.Range(0f, 100f);

        //Get terrain and modify it to make caves
        Terrain cave = GetComponent<Terrain>();
        cave.terrainData.heightmapResolution = width;
        cave.terrainData.size = new Vector3(width, height, depth);
        cave.terrainData.SetHeights(0, 0, HeightGeneration());

        /*Find heights around entrance
        float[,] test = new float[width, depth];
        for (int a = 0; a < width; a++) {
            for (int b = 0; b < depth; b++) {
                if (a > 192 && a < 320 && b > depth - depth/64) {
                    float perlin = Mathf.PerlinNoise((float)a / width * scale + ranX, (float)b / depth * scale + ranY);
                    test[a, b] = formula(perlin, a, b);
                } else {
                    test[a, b] = cave.terrainData.GetHeight(a, b);
                }
            }
        }

        //Clear out section at entrance
        cave.terrainData.SetHeights(0, 0, test);
        */

        //coloring(cave);
    }

    //Fill a double array with the heights
    private float[,] HeightGeneration()
    {
        float[,] heights = new float[width, depth];
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < depth; j++) {
                heights[i, j] = SomeMaths(i, j);
            }
        }
        return heights;
    }

    //Maths to make cave generation better
    private float SomeMaths(int x, int y)
    {
        float perlin = Mathf.PerlinNoise((float)x / width * scale + ranX, (float)y / depth * scale + ranY);

        //Base rounding of ground to floor
        float deposit = Mathf.Clamp01(Mathf.Round(perlin - 0.05f));

        //Left & right walls
        if (x < 6 || x > width - 7) {
            deposit = formula(perlin, x);
        }

        //Back & front walls
        if (y < 6 || y > depth - 7) {
            //Overlaping walls take larger values
            if (x < 6 || x > width - 7) {
                deposit = formula(perlin, x, y);
            } else {
                deposit = formula(perlin, y);
            }
        }

        //Front walls
        if (y > depth - 7) {
            if (x < 192 || x > 319) {
                if (x < 6 || x > width - 7) {
                }
            }
        }
        return deposit;
    }

    //Overload to take highest value for corner overlaps
    private float formula(float perlin, int x, int y)
    {
        return Mathf.Max(formula(perlin, x), formula(perlin, y));
    }

    //Formula for cave height gen
    private float formula(float perlin, int coord)
    {
        return Mathf.Clamp01(Mathf.Round(perlin + Mathf.Abs(coord - 255.5f) / 10 - 25.05f));
    }

    //Color mapping????
    private void coloring(Terrain cave)
    {
        //Coloring???
        float[,,] colormap = new float[cave.terrainData.alphamapWidth, cave.terrainData.alphamapHeight, cave.terrainData.alphamapLayers];

        for (int x = 0; x < cave.terrainData.alphamapWidth; x++) {
            for (int y = 0; y < cave.terrainData.alphamapHeight; y++) {
                float tall = cave.terrainData.GetHeight(x, y);
                Vector3 splat = Vector3.up;
                if (tall >= 0.5f) {
                    splat = Vector3.Lerp(splat, Vector3.right, (tall - 0.5f) * 2);
                } else {
                    splat = Vector3.Lerp(splat, Vector3.forward, tall * 2);
                }
                // now assign the values to the correct location in the array
                splat.Normalize();
                colormap[x, y, 0] = splat.x;
                colormap[x, y, 1] = splat.y;
                colormap[x, y, 2] = splat.z;
            }
        }
        cave.terrainData.SetAlphamaps(0, 0, colormap);
    }
}