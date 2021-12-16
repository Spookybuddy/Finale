using UnityEngine;

public class TerrainCode : MonoBehaviour
{
    private int width = 512;
    private int depth = 512;

    public float height;

    private float ranX;
    private float ranY;
    private float perlin;

    private float scale = 15.0f;

    private bool leftMade;
    private bool rightMade;

    public Vector2 leftOpening;
    public Vector2 rightOpening;
    public GameObject lantern;
    public GameObject markers;
    public GameObject person;

    private GameObject monster;

    void Start()
    {
        monster = GameObject.Find("Periscope");
        leftMade = false;
        rightMade = false;
        CaveGen();
    }

    void Update()
    {
        //If no entrance has been made, try again, destroying any lanterns and npcs spawned
        if (!leftMade || !rightMade) {
            GameObject[] limitLamps = GameObject.FindGameObjectsWithTag("Lamp");
            foreach (GameObject light in limitLamps) {
                Destroy(light);
            }
            GameObject[] limitPeople = GameObject.FindGameObjectsWithTag("NPC");
            foreach (GameObject person in limitPeople) {
                Destroy(person);
            }
            CaveGen();
        }
    }

    private void CaveGen()
    {
        ranX = Random.Range(0, 100);
        ranY = Random.Range(0, 100);

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
                if ((a > leftOpening.x && a < leftOpening.y) && (b > 492) && avg < 1000) {
                    hole[a, b] = 0.0f;
                    leftMade = true;
                }
                if ((a > rightOpening.x && a < rightOpening.y) && (b > 492) && avg < 1000) {
                    hole[a, b] = 0.0f;
                    rightMade = true;
                }
            }
        }

        //Clear out section at entrance and spawn lanterns at entrances made
        cave.SetHeights(0, 0, hole);
        if (leftMade) {
            spawnLamps((leftOpening.x - 257));
            spawnLamps((leftOpening.x - 243));
        }
        if (rightMade) {
            spawnLamps((rightOpening.x - 257));
            spawnLamps((rightOpening.x - 243));
        }
        if  (leftMade || rightMade) {
            leftMade = true;
            rightMade = true;
        }

        //Paint the cave
        cave.SetAlphamaps(0, 0, PaintGen(cave));

        //Put NPCs into random spots
        for (int n = 0; n < 3; n++) {
            bool spawned = false;
            while (!spawned) {
                int x = Random.Range(128*n + 64, 128*n + 192);
                int y = Random.Range(128*n + 64, 128*n + 192);
                if(cave.GetHeight(x, y) == 0) {
                    Instantiate(person, new Vector3(x-512, 1, y-256), transform.rotation);
                    spawned = true;
                }
            }
        }

        //Put monster in an open area in middle
        moveMonster(cave);
    }

    //Fill a double array with the heights
    private float[,] HeightGeneration()
    {
        float[,] heights = new float[width, depth];
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < depth; j++) {
                heights[i, j] = Mathf.Clamp01(Mathf.Round(Maths(i, j)));
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
                    splatter[p, q, r] = Paints(q, p, r);
                }
            }
        }
        return splatter;
    }

    //Wall formula
    private float Maths(int x, int y)
    {
        perlin = Mathf.PerlinNoise((float)x/width * scale + ranX, (float)y/depth * scale + ranY);
        float deliverable = perlin - 0.05f;

        //Left & right walls
        if (x < 6 || x > width - 7) {
            deliverable = formula(x);
        }

        //Back & front walls
        if (y < 6 || y > depth - 7) {
            //Overlaping walls take larger values
            if (x < 6 || x > width - 7) {
                deliverable = Mathf.Max(formula(x), formula(y));
            } else {
                deliverable = formula(y);
            }
        }
        return deliverable;
    }

    //Maths to make the painting
    private float Paints(int x, int y, int z)
    {
        float alpha = 1.0f;

        if (z == 0) {
            alpha = 1.0f - Maths(x, y);
        } else {
            alpha = Maths(x, y);
        }
        return alpha;
    }

    //Formula for cave height gen
    private float formula(int coord)
    {
        return (perlin + Mathf.Abs((float)coord - 255.5f)/10.0f - 25.0f);
    }

    //Spawn lanterns as children
    private void spawnLamps(float z)
    {
        GameObject light = Instantiate(lantern, new Vector3(0, 5, z), transform.rotation) as GameObject;
        light.transform.parent = GameObject.Find("Cave Ground").transform;
    }

    private void moveMonster(TerrainData cave)
    {
        bool placed = false;
        while (!placed) {
            int x = Random.Range(192, 320);
            int y = Random.Range(192, 320);
            if (cave.GetHeight(x, y) == 0) {
                monster.transform.position = new Vector3(x-512, 10, y-256);
                placed = true;
            }
        }
    }
}