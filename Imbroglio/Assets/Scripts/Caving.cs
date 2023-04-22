using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Meshes
{
    public bool wall;
    public MeshCollider hitbox;
    public MeshFilter filter;
}

public class Caving : MonoBehaviour
{
    private int scale;
    private Vector3 center;
    private Vector2 edges;

    public List<Meshes> terrain = new List<Meshes>();
    public GameObject tile;

    private Mesh cave;
    private Mesh overlay;
    private Mesh ceiling;
    private Vector3[] VERTs;
    private Vector3[] UERTs;
    private int[] TRIs;
    private int[] TWOs;
    private Vector2[] UVs;
    private Vector2[] VVs;
    private int[,] mazeData;

    void Start()
    {
        cave = new Mesh();
        overlay = new Mesh();
    }

    public void NewMesh(int size, Vector2 lengths, int[,] maze)
    {
        //Check if the mesh exists
        if (cave == null) cave = new Mesh();
        if (overlay == null) overlay = new Mesh();

        scale = size;
        mazeData = maze;

        //Adjust ground tile to center and scale
        tile.transform.localPosition = new Vector3((scale - 1) / 2f, 0, (scale - 1) / 2f);
        tile.transform.localScale = new Vector3((scale - 1), (scale - 1), 1);

        //Verts & UVs because they are the same size
        VERTs = new Vector3[scale * scale + (scale - 1) * (scale - 1)];
        UERTs = new Vector3[VERTs.Length];
        UVs = new Vector2[VERTs.Length];
        VVs = new Vector2[VERTs.Length];
        for (int i = 0; i < scale; i++) {
            for (int j = 0; j < scale; j++) {
                int index = Indices(i, j);
                VERTs[index] = new Vector3(i, AverageDepth(i, j, lengths), j);
                UERTs[index] = new Vector3(i, 0, j);
                UVs[index] = new Vector2((float)i / scale, (float)j / scale);
                VVs[index] = new Vector2(i % 2, 0);
            }
        }

        //Loop through secondary points to average point heights with surroundings
        for (int i = 0; i < (scale - 1); i++) {
            for (int j = 0; j < (scale - 1); j++) {
                int index = scale * (i + 1) + (scale - 1) * i + j;
                VERTs[index] = new Vector3(i + 0.5f, AverageDepth(index) + Random.Range(-0.2f, 0.2f), j + 0.5f);
                UERTs[index] = new Vector3(i + 0.5f, 0, j + 0.5f);
                UVs[index] = new Vector2((i + 0.5f) / scale, (j + 0.5f) / scale);
                VVs[index] = new Vector2(0.5f, 0);
            }
        }

        //Tris
        TRIs = new int[(scale - 1) * (scale - 1) * 12];
        TWOs = new int[TRIs.Length];
        for (int i = 0; i < scale - 1; i++) {
            for (int j = 0; j < scale - 1; j++) {
                //Coords are stored W N E S C for ease of triangle creation, with N being top left
                int[] coords = new int[5];
                int index = scale * (i + 1) + (scale - 1) * i + j;
                coords[0] = Indices(i, j);
                coords[1] = coords[0] + 1;
                coords[2] = coords[0] + (2 * scale);
                coords[3] = coords[0] + (2 * scale - 1);
                coords[4] = coords[0] + scale;
                Triangles(12 * (i * (scale - 1) + j), 0, coords, index);
            }
        }

        //Reset & Update
        AssignMesh(cave, VERTs, TRIs, UVs);
        AssignMesh(overlay, UERTs, TWOs, VVs);

        //Assign required mesh and collider if needed
        foreach (Meshes mesh in terrain) {
            if (mesh.wall) {
                mesh.filter.mesh = cave;
                mesh.hitbox.sharedMesh = cave;
            } else {
                mesh.filter.mesh = overlay;
            }
        }
    }

    //Assign variables
    private void AssignMesh(Mesh Mesh, Vector3[] V, int[] T, Vector2[] U)
    {
        Mesh.Clear();
        Mesh.vertices = V;
        Mesh.triangles = T;
        Mesh.SetUVs(0, U);
        Mesh.RecalculateNormals();
    }

    //Center point height based on surrounding point heights
    private float AverageHeight(int index)
    {
        float value = 0;
        if (VERTs[index - scale].y >= 1) value++;
        if (VERTs[index - scale + 1].y >= 1) value++;
        if (VERTs[index + scale - 1].y >= 1) value++;
        if (VERTs[index + scale].y >= 1) value++;
        return Mathf.Clamp01((value - 1) / 2);
    }

    //Center vertex averages quadrent height
    private float AverageDepth(int index)
    {
        float value = VERTs[index - scale].y + VERTs[index - scale + 1].y + VERTs[index + scale - 1].y + VERTs[index + scale].y;
        if (value > 0) return Mathf.Clamp01((value - 1) / 2);
        else return Mathf.Ceil(value) / 4;
    }

    //Get adjacent maze vertex heights to see if point should be lowered
    private float AverageDepth(int x, int y, Vector2 lengths)
    {
        int offX = (int)(transform.localPosition.x + lengths.x / 2);
        int offY = (int)(transform.localPosition.z + lengths.y / 2);
        if (mazeData[offX + x, offY + y] > 0) return 1;

        //If all adjacent are 0, lower
        if (mazeData[Mathf.Max(offX + x - 1, 0), offY + y] > 0) return 0;
        if (mazeData[Mathf.Min(offX + x + 1, (int)lengths.x), offY + y] > 0) return 0;
        if (mazeData[offX + x, Mathf.Max(offY + y - 1, 0)] > 0) return 0;
        if (mazeData[offX + x, Mathf.Min(offY + y + 1, (int)lengths.y)] > 0) return 0;
        return -0.5f;
    }

    //Index factoring in the midpoints
    private int Indices(int i, int j)
    {
        return (i * (scale - 1)) + (i * scale) + j;
    }

    //Assign Triangles for only wall sections
    private void Triangles(int index, int iteration, int[] cardinals, int vertex)
    {
        //Render tris only if the y > 0
        int raised = -1;
        if (AverageHeight(vertex) == 0) {
            for (int i = 0; i < 4; i++) {
                if (VERTs[cardinals[i]].y > 0) raised = i;
            }
        }

        //All verts recorded to give cave roofing
        TRIs[index] = cardinals[iteration];
        TRIs[index + 1] = cardinals[(iteration + 1) % 4];
        TRIs[index + 2] = cardinals[4];

        //Assign overlay UVs along raised edges. Change to check map data rather than verts
        if (iteration == raised || (iteration + 1) % 4 == raised || AverageHeight(vertex) != 0) {
            VVs[cardinals[iteration]] = new Vector2(VVs[cardinals[iteration]].x, 1);
            VVs[cardinals[(iteration + 1) % 4]] = new Vector2(VVs[cardinals[(iteration + 1) % 4]].x, 1);
            VVs[cardinals[4]] = new Vector2(0.5f, 1);
        } else {
            //If the quadrent doesn't contain any raised points, record the Tris in the overlay mesh
            TWOs[index] = cardinals[iteration];
            TWOs[index + 1] = cardinals[(iteration + 1) % 4];
            TWOs[index + 2] = cardinals[4];
        }

        //Recursion because I'm fancy like that
        if (iteration + 1 < 4) Triangles(index + 3, iteration + 1, cardinals, vertex);
    }
}