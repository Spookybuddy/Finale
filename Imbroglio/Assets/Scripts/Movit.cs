using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Movit : MonoBehaviour
{
    private float turn;
    private float strafe;
    private float advance;
    public float stepping;

    private Rigidbody rigid;
    private float veloCap;
    private float bobby;

    public List<GameObject> inventory;
    public GameObject bottlePrefab;
    public GameObject rockPrefab;

    public GameObject markerPrefab;
    public int markers;

    private bool yeetIt;
    private bool placed;
    public bool byCar;
    public int numSaved;
    private bool menuLag;

    public bool menuUp;
    public bool paused;
    public bool confirm;
    public bool victory;
    public bool failure;
    public bool mainMenu;
    public bool storyMenu;

    public bool interactRange;
    public bool talkingRange;

    private Mousing lookingAt;
    private MonsterMash monster;

    public float playerSoundLevel;

    void Start()
    {
        lookingAt = GameObject.Find("Main Camera").GetComponent<Mousing>();
        monster = GameObject.Find("Periscope").GetComponent<MonsterMash>();
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
        numSaved = 0;
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
            turn = Input.GetAxis("Mouse X")/2;
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
                bobby = 0.5f;
            } else if (Input.GetKey(KeyCode.Space)) {
                veloCap = 8.0f;
                bobby = 1.5f;
            } else {
                veloCap = 4.0f;
                bobby = 1.0f;
            }

            //Camera bob wave based on speed
            //Remove or nerf
            /*
            if (advance != 0 || strafe != 0) {
                stepping = stepping + bobby/50.0f;
                playerSoundLevel = Mathf.Max(playerSoundLevel, 2 * bobby / 3.0f);
            } else {
                playerSoundLevel = Mathf.Max(playerSoundLevel - 3 * Time.deltaTime, 0);
            }
            */

            //Throw item
            if ((Input.GetKeyDown(KeyCode.Q) || Input.GetAxis("Throw") > 0) && yeetIt && inventory.Count > 0) {
                yeetIt = false;
                StartCoroutine(yeetDelay());

                //Spawn item based on direction looking both horizontal & vertical
                //Change to just relative transform lol
                float waves = (3.1415926f * transform.eulerAngles.y / 180);
                Vector3 spawnAt = new Vector3(1.3f * Mathf.Sin(waves), -Mathf.Sin(lookingAt.looks), 1.3f * Mathf.Cos(waves));
                Instantiate(inventory[0], transform.position + spawnAt, transform.rotation);

                //Remove thrown item from inventory
                inventory.RemoveAt(0);
            }

            //Place marker
            if ((Input.GetKeyDown(KeyCode.R)) && !placed && markers < 10) {
                placed = true;
                StartCoroutine(markerDelay());

                //Spawn a lantern on the floor in front of player
                float waves = (3.14159f * transform.eulerAngles.y / 180);
                Vector3 spawnAt = new Vector3(1.7f * Mathf.Sin(waves), -1.25f, 1.7f * Mathf.Cos(waves));
                Instantiate(markerPrefab, transform.position + spawnAt, transform.rotation);
                markers += 1;
            }

            //Interact with environment (Pick up item / Talk to NPC)
            if (Input.GetKeyDown(KeyCode.E) || Input.GetAxis("Interact") > 0) {
                if (byCar) {
                    //Victory if saved 2 people
                    if (numSaved == 2) {
                        victory = true;
                        menuUp = true;
                    }

                    //Remove all npcs
                    GameObject[] npcs = GameObject.FindGameObjectsWithTag("Saved");
                    if (npcs.Length > 0) {
                        Destroy(npcs[0]);
                        numSaved += 1;
                    }
                    byCar = false;
                    StartCoroutine(npcDelay());

                    //Victory if worm is dead
                    if (monster.health <= 0) {
                        victory = true;
                        menuUp = true;
                    }
                }
                GameObject[] throwables = GameObject.FindGameObjectsWithTag("Throwable");
                foreach (GameObject item in throwables) {
                    //Find distance to player and pickup if within reach
                    float dist = Mathf.Sqrt(Mathf.Pow(transform.position.x - item.transform.position.x, 2) + Mathf.Pow(transform.position.z - item.transform.position.z, 2));
                    if (dist < 2) {
                        //Add the prefab for the respective item (Because you can't just add the item and then destroy it, so this is my workaround)
                        if (item.name == "Rock(Clone)") {
                            inventory.Add(rockPrefab);
                        } else if (item.name == "Bottle(Clone)") {
                            inventory.Add(bottlePrefab);
                        }
                        Destroy(item);
                        interactRange = false;
                    }
                }
            }
        }
    }

    //While within car radius player can leave
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Finish")) {
            byCar = true;
        }
        if (collider.gameObject.CompareTag("Worm")) {
            failure = true;
            menuUp = true;
        }
        if (collider.gameObject.CompareTag("Throwable")) {
            interactRange = true;
        }
        if (collider.gameObject.CompareTag("NPC"))
        {
            talkingRange = true;
        }
    }

    void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.CompareTag("Worm")) {
            failure = true;
            menuUp = true;
        }
    }

    //When leave car radius
    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.CompareTag("Finish")) {
            byCar = false;
        }
        interactRange = false;
        talkingRange = false;
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

    //Deposit NPC delay
    IEnumerator npcDelay()
    {
        yield return new WaitForSeconds(0.6f);
        byCar = true;
    }

    //Lag menu exit ability
    IEnumerator menuDelay()
    {
        yield return new WaitForSeconds(0.25f);
        menuLag = false;
    }
}