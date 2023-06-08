using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Movit : MonoBehaviour
{
    private float turn;
    private float strafe;
    private float advance;

    private Rigidbody rigid;
    private float veloCap;

    public List<GameObject> inventory;
    public GameObject bottlePrefab;
    public GameObject rockPrefab;
    public GameObject markerPrefab;
    public Transform spawnThrow;
    public int markers;

    private bool yeetIt;
    private bool placed;
    private bool menuLag;

    public bool menuUp;
    public bool paused;
    public bool confirm;
    public bool victory;
    public bool failure;
    public bool mainMenu;
    public bool storyMenu;

    public bool interactRange;
    private bool inCave;
    private Vector3 markPoint;

    private Mousing lookingAt;
    private MonsterMash monster;

    public float playerSoundLevel;

    void Start()
    {
        lookingAt = GameObject.FindWithTag("MainCamera").GetComponent<Mousing>();
        monster = GameObject.FindWithTag("Monster").GetComponent<MonsterMash>();
        rigid = GetComponent<Rigidbody>();
        yeetIt = true;
        placed = false;
        markers = 0;
        menuUp = true;
        mainMenu = true;
        storyMenu = false;
        paused = false;
        confirm = false;
        menuLag = false;
        victory = false;
        failure = false;
        playerSoundLevel = 0.0f;
    }

    void Update()
    {
        //Main menu
        if (mainMenu) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                mainMenu = false;
                storyMenu = true;
            } else if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.E)) {
                Debug.Log("Quit");
                Application.Quit();
            }
        } else if (storyMenu) {
            //Display objective text after beginning
            if (Input.GetKeyDown(KeyCode.Escape)) {
                storyMenu = false;
                menuUp = false;
            }
        } else if (failure || victory) {
            //Escape on failure/victory reloads scene
            if (Input.GetKeyDown(KeyCode.Escape)) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        } else if (confirm) {
            if (Input.GetKeyDown(KeyCode.Return) && !menuLag) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            if (Input.GetKeyDown(KeyCode.Escape)) {
                confirm = false;
                paused = true;
                menuUp = true;
            }
        } else if (paused) {
            //Quit to main via Enter with confirmation message
            if (Input.GetKeyDown(KeyCode.Return)) {
                menuLag = true;
                StartCoroutine(menuDelay());
                paused = false;
                confirm = true;
            }
            //Unpause via Escape
            if (Input.GetKeyDown(KeyCode.Escape)) {
                paused = false;
                menuUp = false;
            }
        } else if (Input.GetKeyDown(KeyCode.Escape)) {
            paused = true;
            menuUp = true;
        }

        if (!menuUp) {
            //Mouse Camera (Left/Right)
            turn = Input.GetAxis("Mouse X") / 2;
            transform.Rotate(Vector3.up, turn);

            //Strafing
            strafe = Input.GetAxis("Horizontal");
            rigid.AddRelativeForce(Vector3.right * strafe * Time.deltaTime * (veloCap / 2), ForceMode.Impulse);

            //Walk forward
            advance = Input.GetAxis("Vertical");
            rigid.AddRelativeForce(Vector3.forward * advance * Time.deltaTime * (veloCap / 2), ForceMode.Impulse);

            //Velocity cap
            rigid.angularVelocity = Vector3.zero;
            rigid.velocity = new Vector3(Mathf.Clamp(rigid.velocity.x, -veloCap, veloCap), 0, Mathf.Clamp(rigid.velocity.z, -veloCap, veloCap));

            //Sneak -> Sprint -> Walk >>> CHANGE TO INPUT MANAGER
            if (Input.GetKey(KeyCode.LeftShift)) {
                veloCap = 1.0f;
            } else if (Input.GetKey(KeyCode.Space)) {
                veloCap = 8.0f;
            } else {
                veloCap = 4.0f;
            }

            //Throw item
            if ((Input.GetKeyDown(KeyCode.Q) || Input.GetAxis("Throw") > 0) && yeetIt && inventory.Count > 0) {
                yeetIt = false;
                StartCoroutine(yeetDelay());

                //Spawn item based on direction looking both horizontal & vertical
                Instantiate(inventory[0], spawnThrow.position, transform.rotation);
                inventory.RemoveAt(0);
            }

            //Place marker
            if ((Input.GetKeyDown(KeyCode.R)) && !placed && markers < 10) {
                placed = true;
                StartCoroutine(markerDelay());

                //Spawn a lantern from ceiling if in cave
                if (inCave) {
                    Instantiate(markerPrefab, markPoint, transform.rotation);
                    markers += 1;
                }
            }

            //Victory if worm is dead
            if (monster.health <= 0) {
                victory = true;
                menuUp = true;
            }

            //Interact with environment
            if (Input.GetKeyDown(KeyCode.E) || Input.GetAxis("Interact") > 0) {
                GameObject[] throwables = GameObject.FindGameObjectsWithTag("Throwable");
                foreach (GameObject item in throwables) {
                    //Find distance to player and pickup if within reach
                    if (Vector3.Distance(item.transform.position, transform.position) < 2.5f) {
                        inventory.Add(item.name == "Rock(Clone)" ? rockPrefab : bottlePrefab);
                        Destroy(item);
                    }
                }
            }
        }
    }

    //Check if in cave
    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, Vector3.up, out RaycastHit hit, 4)) {
            inCave = hit.collider.CompareTag("Cave");
            markPoint = hit.point;
        } else { inCave = false; }
    }

    //Eaten by worm
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Worm")) {
            failure = true;
            menuUp = true;
        }
        if (collider.gameObject.CompareTag("Event")) {
            Debug.Log("CaveInCutscene");
        }
    }

    void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.CompareTag("Worm")) {
            failure = true;
            menuUp = true;
        }
    }

    //Throw reset delay
    IEnumerator yeetDelay()
    {
        yield return new WaitForSeconds(1.2f);
        yeetIt = true;
    }

    //Place marker delay
    IEnumerator markerDelay()
    {
        yield return new WaitForSeconds(1.8f);
        placed = false;
    }

    //Lag menu exit ability
    IEnumerator menuDelay()
    {
        yield return new WaitForSeconds(0.25f);
        menuLag = false;
    }
}