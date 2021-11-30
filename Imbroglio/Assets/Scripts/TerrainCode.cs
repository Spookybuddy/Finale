using UnityEngine;

public class TerrainCode : MonoBehaviour
{
    private int width = 512;
    private int depth = 512;

    public float height;

    private float ranX;
    private float ranY;

    private float scale = 15.0f;

    private bool entranceMade;

    public Vector2 leftOpening;
    public Vector2 rightOpening;

    void Start()
    {
        entranceMade = false;
        CaveGen();
    }

    void Update()
    {
        if (!entranceMade) {
            CaveGen();
        }
    }

    private void CaveGen()
    {
        ranX = Random.Range(0f, 100f);
        ranY = Random.Range(0f, 100f);

        //Get terrain and modify it to make caves
        TerrainData cave = GetComponent<Terrain>().terrainData;
        cave.heightmapResolution = width;
        cave.size = new Vector3(width, height, depth);
        cave.SetHeights(0, 0, HeightGeneration());

        //Pick a random area for a hole
        float[,] hole = new float[width, depth];
        leftOpening = new Vector2(Random.Range(207, 244), 0);
        leftOpening.y = leftOpening.x + 12;

        rightOpening= new Vector2(Random.Range(256, 293), 0);
        rightOpening.y = rightOpening.x + 12;

        //Carve a hole into the wall for an entrance
        float avg = 0;
        for (int a = 0; a < width; a++) {
            for (int b = 0; b < depth; b++) {
                hole[a, b] = cave.GetHeight(a, b);
                //Find average of area to determine if it is a good spot
                if ((a == leftOpening.x +1 || a == rightOpening.x + 1) && b == 493) {
                    avg = 0;
                    for (int c = 0; c < 12; c++) {
                        for (int d = 0; d < 16; d++) {
                            avg += cave.GetHeight(a + c, b + d);
                        }
                    }
                }
                if (((a > leftOpening.x && a < leftOpening.y) || (a > rightOpening.x && a < rightOpening.y)) && (b > 492) && avg < 1000) {
                    hole[a, b] = 0.0f;
                    entranceMade = true;
                }
            }
        }

        //Clear out section at entrance
        cave.SetHeights(0, 0, hole);

        //Paint the cave
        cave.SetAlphamaps(0, 0, PaintGen(cave));
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

    //Paint the terrain
    private float[,,] PaintGen(TerrainData cave)
    {
        float[,,] splatter = new float[cave.alphamapWidth, cave.alphamapHeight, cave.alphamapLayers];
        for (int p = 0; p < cave.alphamapWidth; p++) {
            for (int q = 0; q < cave.alphamapHeight; q++) {
                for (int r = 0; r < cave.alphamapLayers; r++) {
                    splatter[p, q, r] = SomePaints(q, p, r);
                }
            }
        }
        return splatter;
    }

    //Maths to make cave generation better
    private float SomeMaths(int x, int y)
    {
        float perlin = Mathf.PerlinNoise((float)x/width * scale + ranX, (float)y/depth * scale + ranY);

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
                deposit = Mathf.Max(formula(perlin, x), formula(perlin, y));
            } else {
                deposit = formula(perlin, y);
            }
        }
        return deposit;
    }

    //Formula for cave height gen
    private float formula(float perlin, int coord)
    {
        return Mathf.Clamp01(Mathf.Round(perlin + Mathf.Abs(coord - 255.5f)/10 - 25.05f));
    }

    //Maths to make the painting
    private float SomePaints(int x, int y, int z)
    {
        float perlin = Mathf.PerlinNoise((float)x/width * scale + ranX, (float)y/depth * scale + ranY);
        float alpha = 1.0f;

        if (z == 0) {
            alpha = Mathf.Clamp01(perlin);
        } else {
            alpha = Mathf.Clamp01(1.0f - perlin);
        }
        return alpha;
    }
}