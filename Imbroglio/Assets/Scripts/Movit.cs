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

    private bool yeetIt;
    private bool byCar;

    public bool menuUp;
    public bool paused;
    public bool victory;
    public bool failure;
    public bool mainMenu;

    public bool interactRange;

    private Mousing lookingAt;

    public float playerSoundLevel;

    void Start()
    {
        lookingAt = GameObject.Find("Main Camera").GetComponent<Mousing>();
        rigid = GetComponent<Rigidbody>();
        yeetIt = true;
        menuUp = true;
        mainMenu = true;
        paused = false;
        victory = false;
        failure = false;
        playerSoundLevel = 0;
    }

    void Update()
    {
        //Main menu
        if (mainMenu) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                mainMenu = false;
                menuUp = false;
            } else if (Input.GetKeyDown(KeyCode.Escape)) {
                Debug.Log("Quit");
                Application.Quit();
            }
        } else if (failure || victory) {
            //Escape on failure/victory reloads scene
            if (Input.GetKeyDown(KeyCode.Escape)) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        } else if (Input.GetKeyDown(KeyCode.Escape)) {
            //Escape when everything else is false is pause/unpause
            menuUp = !menuUp;
            paused = !paused;
        }

        if (!menuUp) {
            //Mouse Camera (Left/Right)
            turn = Input.GetAxis("Mouse X");
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
                veloCap = 1;
                bobby = 0.5f;
            } else if (Input.GetKey(KeyCode.Space)) {
                veloCap = 8;
                bobby = 1.5f;
            } else {
                veloCap = 4;
                bobby = 1;
            }

            //Camera bob wave based on speed
            if (advance != 0 || strafe != 0) {
                stepping = stepping + (1 / (100 / bobby));
                playerSoundLevel = Mathf.Max(playerSoundLevel, 2 * bobby / 3);
            } else {
                playerSoundLevel = Mathf.Max(playerSoundLevel - 3 * Time.deltaTime, 0);
            }

            //Throw item
            if ((Input.GetKeyDown(KeyCode.Q) || Input.GetAxis("Throw") > 0) && yeetIt && inventory.Count > 0) {
                yeetIt = false;
                StartCoroutine(yeetDelay());

                //Spawn item based on direction looking both horizontal & vertical
                float waves = (Mathf.PI * transform.eulerAngles.y / 180);
                Vector3 spawnAt = new Vector3(1.3f * Mathf.Sin(waves), -Mathf.Sin(lookingAt.looks), 1.3f * Mathf.Cos(waves));
                Instantiate(inventory[0], transform.position + spawnAt, transform.rotation);

                //Remove thrown item from inventory
                inventory.RemoveAt(0);
            }

            //Interact with environment (Pick up item / Talk to NPC)
            if (Input.GetKeyDown(KeyCode.E) || Input.GetAxis("Interact") > 0) {
                if (byCar) {
                    Debug.Log("Deposit NPC");
                    byCar = false;
                    StartCoroutine(npcDelay());

                    //Testing victory
                    victory = true;
                    menuUp = true;
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
    }

    //Throw reset delay
    IEnumerator yeetDelay()
    {
        yield return new WaitForSeconds(1.2f);
        yeetIt = true;
    }

    //Deposit NPC delay
    IEnumerator npcDelay()
    {
        yield return new WaitForSeconds(0.6f);
        byCar = true;
    }
}