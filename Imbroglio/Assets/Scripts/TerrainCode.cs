using UnityEngine;

public class TerrainCode : MonoBehaviour
{
    private int width = 512;
    private int depth = 512;

    public float height;

    public float ranX;
    public float ranY;

    private float scale = 15.0f;

    private bool entranceMade;

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
        Terrain cave = GetComponent<Terrain>();
        cave.terrainData.heightmapResolution = width;
        cave.terrainData.size = new Vector3(width, height, depth);
        cave.terrainData.SetHeights(0, 0, HeightGeneration());

        //Pick a random area for a hole
        float[,] hole = new float[width, depth];
        Vector2 openingLeft = new Vector2(Random.Range(207, 244), 0);
        openingLeft.y = openingLeft.x + 12;

        Vector2 openingRight= new Vector2(Random.Range(256, 293), 0);
        openingRight.y = openingRight.x + 12;

        //Carve a hole into the wall for an entrance
        float avg = 0;
        for (int a = 0; a < width; a++) {
            for (int b = 0; b < depth; b++) {
                hole[a, b] = cave.terrainData.GetHeight(a, b);
                //Find average of area to determine if it is a good spot
                if ((a == openingLeft.x +1 || a == openingRight.x + 1) && b == 493) {
                    avg = 0;
                    for (int c = 0; c < 12; c++) {
                        for (int d = 0; d < 16; d++) {
                            avg += cave.terrainData.GetHeight(a + c, b + d);
                        }
                    }
                    Debug.Log(avg);
                }
                if (((a > openingLeft.x && a < openingLeft.y) || (a > openingRight.x && a < openingRight.y)) && (b > 492) && avg < 1000) {
                    hole[a, b] = 0.0f;
                    entranceMade = true;
                }
            }
        }

        //Clear out section at entrance
        cave.terrainData.SetHeights(0, 0, hole);
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
                deposit = formula(perlin, x, y);
            } else {
                deposit = formula(perlin, y);
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
}