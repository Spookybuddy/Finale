using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class MonsterMash : MonoBehaviour
{
    public Rigidbody rigid;

    public GameObject tracking;
    private Movit player;
    public GameObject worm;

    public int health;
    public bool hunting;
    public bool itemHit;
    public bool searching;
    public float itemDistance;
    public float playerThreshold = 0.2f;
    public Vector3 goTowards;

    private bool move;
    private float spd;

    private bool attacking;

    private AudioSource sounds;
    public AudioClip[] growls;
    private bool doNoise;

    private Node[,] mazeData;
    private Vector2 scale;
    private int size;
    public List<Vector3> path = new List<Vector3>();
    public LineRenderer line;

    private float pan;
    private const float HALFPI = 1.57079632679489662f;
    private const float INVERSE = 57.29577951308232087f;
    private const float NEGATE = -0.69813170079773212f;
    private const float APPROX = 0.87266462599716477f;

    void Start()
    {
        player = tracking.GetComponent<Movit>();
        sounds = GetComponent<AudioSource>();
        attacking = false;
        health = 91;
        StartCoroutine(noises());
    }

    void Update()
    {
        //Path finding test - Seems to work
        //Set trigger box at entry to begin wandering
        if (Input.GetKeyDown(KeyCode.G)) {
            FindPath(mazePoint(transform.position), mazePoint(tracking.transform.position));
            line.positionCount = path.Count;
            line.SetPositions(path.ToArray());
        }

        //Stop when killed
        if (health <= 0) move = false;

        if (!player.menuUp && health > 0) {
            //Movement type speeds
            if (hunting) {
                spd = 9;
                goTowards = tracking.transform.position;
            } else if (searching) {
                spd = 5;
            } else {
                spd = 3;
            }

            //Calc player sound based on distance
            if (player.playerSoundLevel > playerThreshold) {
                float noise = 8 / Vector3.Distance(transform.position, tracking.transform.position);
                noise *= player.playerSoundLevel;
                if (noise > playerThreshold) {
                    hunting = true;
                    searching = false;
                } else if (noise < playerThreshold / 2) {
                    hunting = false;
                }
            }

            //Calc thrown sound based on distance
            if (itemHit) {
                if (itemDistance > playerThreshold) {
                    hunting = false;
                    searching = true;
                } else {
                    itemHit = false;
                }
            }

            //When reaching the target position
            if (Vector3.Distance(transform.position, goTowards) < 1.5 && !attacking) {
                float range = (scale.x * (size - 1));
                goTowards = new Vector3(Random.Range(-range - 144, range - 144), 0, Random.Range(-range, range));
                move = false;
                attacking = true;
                searching = false;
                hunting = false;
                itemHit = false;

                //Worm bite
                GameObject[] heads = GameObject.FindGameObjectsWithTag("Worm");
                foreach (GameObject head in heads) {
                    Destroy(head);
                }
                Instantiate(worm, new Vector3(transform.position.x, -12, transform.position.z), transform.rotation);
            }

            //Attack here
            if (attacking) {
                move = false;
                StartCoroutine(attackDuration());
            } else { move = true; }

            //Play constant audio out of 3 growls
            if (doNoise) {
                //calc distance to player, and player can only hear the monster when within 80m of monster
                float toPlayer = Vector3.Distance(transform.position, tracking.transform.position);
                sounds.pitch = (Random.Range(0.875f, 1.125f));
                sounds.PlayOneShot(growls[Random.Range(0, growls.Length)], Mathf.Clamp01(12 / toPlayer - 0.15f));
                doNoise = false;
                StartCoroutine(noises());
            }

            //Move towards path[0] until distance < 0.5, then remove point and move towards next
            if (path.Count > 0 && move) {
                transform.position = Vector3.MoveTowards(transform.position, path[0], Time.deltaTime * spd);
                if (Vector3.Distance(transform.position, path[0]) < 0.5f) {
                    path.RemoveAt(0);
                    //If list is now empty, recalculate pathing
                    if (path.Count == 0) FindPath(mazePoint(transform.position), mazePoint(goTowards));
                }
            }
        } else {
            move = false;
        }
    }

    //Do the math for pan stereo here to be less computational
    //Do the path finding calls here too for that same reason
    void FixedUpdate()
    {
        //Pan stereo constantly keeping track of player position relative to monster
        //Sin( (Player Rotation - Inverse Cos (X dist / Distance) * sign(Y dist) * 57.29578) / 90) * 1.5707963268)
        float Xdist = (tracking.transform.position.x - transform.position.x);
        float Zdist = (tracking.transform.position.z - transform.position.z);
        Xdist = Xdist / SquareRoot((Xdist * Xdist) + (Zdist * Zdist));
        Zdist = Mathf.Sign(Zdist) * INVERSE;
        pan = Mathf.Cos((tracking.transform.eulerAngles.y - ArcSub(Xdist) * Zdist) / 90 * HALFPI);
        sounds.panStereo = pan;

        //Update the player position when hunting
        if (move && hunting) {
            FindPath(mazePoint(transform.position), mazePoint(goTowards));
        }
    }

    //Faster approximation of Arc cosine
    private float ArcSub(float x)
    {
        return (NEGATE * x * x - APPROX) * x + HALFPI;
    }

    //Faster approximation of Square Root
    private float SquareRoot(float x)
    {
        float a = 125;
        for (int b = 0; b < 5; b++) { a = (a + x / a) / 2; }
        return a;
    }

    //Calculate the path to target
    private void FindPath(Node start, Node end)
    {
        //Remove watch
        Stopwatch watch = new Stopwatch();
        watch.Start();

        Node begin = start;
        Node target = end;
        int maxSize = (int)(scale.x * (size - 1) * scale.y * (size - 1));
        Heap<Node> open = new Heap<Node>(maxSize);
        HashSet<Node> closed = new HashSet<Node>();
        open.Add(begin);

        while(open.Count > 0) {
            Node cur = open.RemoveFirst();
            closed.Add(cur);

            if (cur == target) {
                RetracePath(begin, end);
                watch.Stop();
                print (watch.ElapsedMilliseconds + "ms");
                return;
            }

            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    Node N = cur.Neighbor(x, y);
                    if (N == null || !N.walk || closed.Contains(N)) continue;

                    int mov = cur.gcost + PathDistance(cur, N) + N.penalty;
                    if (mov < N.gcost || !open.Contains(N)) {
                        N.gcost = mov;
                        N.hcost = PathDistance(N, target);
                        N.prev = cur;

                        if (!open.Contains(N)) open.Add(N);
                        else open.UpdateItem(N);
                    }
                }
            }
        }
    }

    //Convert given positions into maze point when within bounds
    private Node mazePoint(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / 2 + (scale.x * (size - 1)) / 2 + 72);
        int y = Mathf.RoundToInt(position.z / 2 + (scale.y * (size - 1)) / 2);
        if ((x > 0 && x < scale.x * (size - 1)) && (y > 0 && y < scale.y * (size - 1))) return mazeData[x, y];
        else return mazeData[(int)Random.Range(0, scale.x * (size - 1)), (int)Random.Range(0, scale.y * (size - 1))];
    }

    //Save the maze data upon generation for pathfinding
    public void PathfindingData(Node[,] maze, int space, Vector2 XZ)
    {
        mazeData = maze;
        size = space;
        scale = XZ;
    }

    //Returns the distance between given nodes
    private int PathDistance(Node A, Node B)
    {
        int X = Mathf.Abs(A.mazeX - B.mazeX);
        int Y = Mathf.Abs(A.mazeY - B.mazeY);
        return (14 * Mathf.Min(X, Y) + 10 * Mathf.Abs(X - Y));
    }

    //Get the path points
    private void RetracePath(Node start, Node end)
    {
        path = new List<Vector3>();
        Node cur = end;

        //Convert the maze points into world coords, moving backward through the established list
        while (cur != start) {
            //Add noise to the points lol
            float x = 2 * cur.mazeX - (scale.x * (size - 1)) - 144 + Random.Range(-0.25f, 0.25f);
            float y = 2 * cur.mazeY - (scale.y * (size - 1)) + Random.Range(-0.25f, 0.25f);
            path.Add(new Vector3(x, 1, y));
            cur = cur.prev;
        }
        path.Reverse();

        line.positionCount = path.Count;
        line.SetPositions(path.ToArray());
    }

    //Tells monster where and how loud a thrown object is
    public void UpdateSound(float vol, Vector3 pos)
    {
        if (vol > playerThreshold) {
            hunting = false;
            searching = true;
            goTowards = pos;
            FindPath(mazePoint(transform.position), mazePoint(goTowards));
        }
    }

    //Attack animation
    IEnumerator attackDuration()
    {
        yield return new WaitForSeconds(1.5f);
        attacking = false;
        move = true;
    }

    //Growling spacing
    IEnumerator noises()
    {
        yield return new WaitForSeconds(2.75f);
        doNoise = true;
    }
}