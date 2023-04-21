using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    public int mazeX;
    public int mazeY;
    public bool walk;
    public int gcost;
    public int hcost;
    public int penalty;
    Node[,] neighbors = new Node[3, 3];
    public Node prev;
    int heapIndex;

    //Node construction
    public Node(int x, int y, bool valid, int weight) {
        mazeX = x;
        mazeY = y;
        walk = valid;
        penalty = weight;
    }

    //Record adjacent nodes
    public void Adjacent(int x, int y, Node n) {
        neighbors[x + 1, y + 1] = n;
    }

    //Return neighbor at x, y
    public Node Neighbor(int x, int y) {
        return neighbors[x + 1, y + 1];
    }

    //F cost
    public int F {
        get { return gcost + hcost; }
    }

    //Heap index
    public int HeapIndex {
        get { return heapIndex; }
        set { heapIndex = value; }
    }

    //Compare costs
    public int CompareTo(Node comparison) {
        int compare = F.CompareTo(comparison.F);
        if (compare == 0) compare = hcost.CompareTo(comparison.hcost);
        return -compare;
    }
}

public class Generation : MonoBehaviour
{
    public int scale;
    private int scale_1;
    public Vector2 XZ;
    private Vector2 XZ_1;
    public int entranceSize;
    public Transform StartingArea;
    public GameObject ChunkPrefab;
    public MonsterMash AI;
    private Node[,] pathData;
    private int[,] heights;
    private int[,] weights;
    private GameObject[,] chunks;
    private Vector2 size;
    private List<Vector2> points;

    public GameObject decorations;
    public GameObject archway;
    public GameObject railroad;
    public GameObject minecart;

    void Start()
    {
        //Instantiate all chunks at start, then randomize them when needed
        //Example: Generate all chunks when game is booted up, then randomize everytime they start a new run
        scale_1 = scale - 1;
        XZ_1 = XZ / 2;
        size = XZ * scale_1;
        chunks = new GameObject[(int)XZ.x, (int)XZ.y];
        for (int i = 0; i < XZ.x; i++) {
            for (int j = 0; j < XZ.y; j++) {
                Vector3 pos = new Vector3((i * scale_1) - (XZ_1.x * scale_1) + transform.position.x, 0, (j * scale_1) - (XZ_1.y * scale_1) + transform.position.z);
                GameObject segment = Instantiate(ChunkPrefab, pos, Quaternion.identity, transform) as GameObject;
                chunks[i, j] = segment;
            }
        }

        Generate();
    }

    //Create a height map for a maze, and then call each chunk to create a mesh based off of that map
    public void Generate()
    {
        //Shrink to generate in scale, size up later
        transform.localScale = new Vector3(1, 1, 1);

        //Assign the height values for a maze
        Mazercise();

        //Locate valid spawns for decorations
        Decorate();

        //Entrance hole
        for (int h = -Mathf.FloorToInt(entranceSize / 2); h <= Mathf.CeilToInt(entranceSize / 2); h++) {
            for (int o = 0; o < 3; o++) {
                heights[(int)size.x - o, (int)(size.y / 2) + h] = -1;
            }
        }

        //Generate the meshes
        for (int i = 0; i < XZ.x; i++) {
            for (int j = 0; j < XZ.y; j++) {
                chunks[i, j].GetComponent<Caving>().NewMesh(scale, size, heights);
            }
        }

        //AI
        Pathing();

        //Scale up because the scaling was a bit off
        transform.localScale = new Vector3(2, 2, 2);
    }

    //Record pathfinding data and send to monster----------------------------------------------------------------------------------------------------
    private void Pathing()
    {
        //Create a weight map for points based on distance from walls
        //Include obstacles in weights
        weights = new int[(int)size.x + 1, (int)size.y + 1];
        for (int i = 0; i <= size.x; i++) {
            for (int j = 0; j <= size.y; j++) {
                int adjacentWalls = 0;
                for (int x = -1; x < 2; x++) {
                    int a = (int)Mathf.Clamp(i + x, 0, size.x);
                    for (int y = -1; y < 2; y++) {
                        int b = (int)Mathf.Clamp(j + y, 0, size.y);
                        if (heights[a, b] > 0) adjacentWalls++;
                    }
                }
                weights[i, j] = 10 * adjacentWalls;
            }
        }

        //Create nodes that line up with the height map
        pathData = new Node[(int)size.x + 1, (int)size.y + 1];
        for (int x = 0; x <= size.x; x++) {
            for (int y = 0; y <= size.y; y++) {
                pathData[x, y] = new Node(x, y, heights[x, y] <= 0, weights[x, y]);
            }
        }

        //Record each node's neighbors
        for (int x = 0; x <= size.x; x++) {
            for (int y = 0; y <= size.y; y++) {
                for (int p = -1; p < 2; p++) {
                    for (int q = -1; q < 2; q++) {
                        int a = x + p;
                        int b = y + q;
                        if ((p == 0 && q == 0) || a < 0 || a > size.x || b < 0 || b > size.y) continue;
                        pathData[x, y].Adjacent(p, q, pathData[a, b]);
                    }
                }
            }
        }

        AI.PathfindingData(pathData, scale, XZ);
    }

    //All decorations have a radius of 2-------------------------------------------------------------------------------------------------------------
    //Rails will spawn in lines 7 or more
    //Minecart has a chance to spawn on rails or knocked over next to walls
    private void Decorate()
    {
        for (int x = 0; x <= size.x; x++) {
            for (int y = 0; y <= size.y; y++) {
                //Check archways Y
                if (heights[x, y] > 0 && y + 6 <= size.y) {
                    if (heights[x, y + 6] > 0) ArchCheckY(x, y);
                }
                //Check archways X
                if (heights[x, y] > 0 && x + 6 <= size.x) {
                    if (heights[x + 6, y] > 0) ArchCheckX(x, y);
                }
            }
        }
    }

    //Arch checks for cardinal gaps of entrance size
    private void ArchCheckY(int x, int y)
    {
        for (int a = 1; a < 6; a++) if (heights[x, y + a] > 0) return;
        float p = 2 * x - size.x - 144;
        float q = 2 * (y + 3) - size.y;
        Instantiate(archway, new Vector3(p, 0, q), Quaternion.identity, decorations.transform);
    }

    private void ArchCheckX(int x, int y)
    {
        for (int a = 1; a < 6; a++) if (heights[x + a, y] > 0) return;
        float p = 2 * (x + 3) - size.x - 144;
        float q = 2 * y - size.y;
        Instantiate(archway, new Vector3(p, 0, q), new Quaternion(0, 0.5f, 0, 0.5f), decorations.transform);
    }

    //Prim Maze algorithm----------------------------------------------------------------------------------------------------------------------------
    //Code from https://kairumagames.com/blog/cavetutorial
    private void Mazercise()
    {
        //Start with all points as walls
        heights = new int[(int)size.x + 1, (int)size.y + 1];
        for (int i = 0; i <= size.x; i++) {
            for (int j = 0; j <= size.y; j++) {
                heights[i, j] = 1;
            }
        }

        //Starting point at entrance
        int x = (int)size.x - 1;
        int y = (int)(size.y / 2) + 1;
        heights[x, y] = 0;

        //Record valid points in cardinal directions from start
        points = new List<Vector2>();
        Check(x, y);

        //Until all valid points have been used, randomly pick, check, and expand it, then remove from list
        while (points.Count > 0) {
            int index = Random.Range(0, points.Count);
            x = (int)points[index].x;
            y = (int)points[index].y;
            heights[x, y] = 0;
            points.RemoveAt(index);

            //Randomly check cardinal points until one that isnt a wall is found
            List<int> cardinals = new List<int>() { 0, 1, 2, 3 };
            while (cardinals.Count > 0) {
                int i = Random.Range(0, cardinals.Count);
                switch (cardinals[i]) {
                    case 0:
                        if (y - 2 >= 0 && heights[x, y - 2] <= 0 && heights[x, y - 1] == 1) {
                            heights[x, y - 1] = 0;
                            cardinals.Clear();
                        } else cardinals.RemoveAt(i);
                        break;
                    case 1:
                        if (y + 2 < size.y && heights[x, y + 2] <= 0 && heights[x, y + 1] == 1) {
                            heights[x, y + 1] = 0;
                            cardinals.Clear();
                        } else cardinals.RemoveAt(i);
                        break;
                    case 2:
                        if (x - 2 >= 0 && heights[x - 2, y] <= 0 && heights[x - 1, y] == 1) {
                            heights[x - 1, y] = 0;
                            cardinals.Clear();
                        } else cardinals.RemoveAt(i);
                        break;
                    case 3:
                        if (x + 2 < size.x && heights[x + 2, y] <= 0 && heights[x + 1, y] == 1) {
                            heights[x + 1, y] = 0;
                            cardinals.Clear();
                        } else cardinals.RemoveAt(i);
                        break;
                }
            }

            //Add valid cells and repeat
            Check(x, y);
        }

        //Prune dead ends, expand into a cavern like environment, then prune some more dead ends again
        Prune(3);
        Grow(4);
        Prune(3);
    }

    //Prune dead ends by given amount
    void Prune(int amount)
    {
        //Check if more than 1 neighbor, otherwise delete
        bool[,] remove = new bool[(int)size.x, (int)size.y];
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                if (heights[x, y] == 0) {
                    int neighbors = 0;
                    if (y - 1 >= 0 && heights[x, y - 1] == 0) neighbors++;
                    if (y + 1 < size.y && heights[x, y + 1] == 0) neighbors++;
                    if (x - 1 >= 0 && heights[x - 1, y] == 0) neighbors++;
                    if (x + 1 < size.x && heights[x + 1, y] == 0) neighbors++;
                    remove[x, y] = (neighbors <= 1);
                }
            }
        }

        //Loop through listed dead ends and remove them to prevent cascading
        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.y; j++) {
                if (remove[i, j] == true) heights[i, j] = 1;
            }
        }

        //Recursion
        if (amount > 0) Prune(amount - 1);
    }

    //Grow the maze into a cave
    void Grow(int iterations)
    {
        //Expand cells if 4/8 adjacent cells are paths
        bool[,] grown = new bool[(int)size.x, (int)size.y];
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                if (heights[x, y] == 1) {
                    int neighbors = 0;
                    for (int a = -1; a < 2; a++) {
                        for (int b = -1; b < 2; b++) {
                            int neighbor_x = x - a;
                            int neighbor_y = y - b;
                            if (neighbor_x >= 0 && neighbor_x < size.x && neighbor_y >= 0 && neighbor_y < size.y) {
                                if (heights[neighbor_x, neighbor_y] == 0) neighbors++;
                            }
                        }
                    }
                    grown[x, y] = (neighbors > 3);
                }
            }
        }

        //Loop through list and assign based on values. Prevents cells frombeing validated when they shouldn't be
        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.y; j++) {
                if (grown[i, j] == true) heights[i, j] = 0;
            }
        }

        //Recursion
        if (iterations > 0) Grow(iterations - 1);
    }

    //Check if 2 spaces away is viable
    void Check(int x, int y)
    {
        if (y - 2 >= 0 && heights[x, y - 2] > 0) CreatePoint(new Vector2(x, y - 2));
        if (y + 2 < size.y && heights[x, y + 2] > 0) CreatePoint(new Vector2(x, y + 2));
        if (x - 2 >= 0 && heights[x - 2, y] > 0) CreatePoint(new Vector2(x - 2, y));
        if (x + 2 < size.x && heights[x + 2, y] > 0) CreatePoint(new Vector2(x + 2, y));
    }

    //Add point to list if not already on there
    void CreatePoint(Vector2 point)
    {
        if (!points.Contains(point)) points.Add(point);
    }
}