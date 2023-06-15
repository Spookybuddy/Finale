using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movit : MonoBehaviour
{
    //Holy shit please reorganize these variables
    //Screenshake should only affect camera
    //Menuing should be moved to gamemanger & UI buttons - That will free up a lot of space
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
    public Light lamp;
    private bool flipping;
    public float flashBat;
    public float flashMax;

    public bool menuUp;
    public bool paused;
    public bool victory;
    public bool failure;

    public bool interactRange;
    public Vector3 entrance;
    public AnimationCurve curve;
    private bool inCave;
    private bool caveIn;
    private bool collapseTriggered;
    private bool disabled;
    private float shakeMonster;
    private float monsterDistance;
    private Vector3 markPoint;

    private Mousing lookingAt;
    private MonsterMash monster;
    private GameManager manager;
    private Transform monsterPos;

    public float playerSoundLevel;

    void Start()
    {
        lookingAt = GameObject.FindWithTag("MainCamera").GetComponent<Mousing>();
        monsterPos = GameObject.FindWithTag("Monster").transform;
        monster = monsterPos.gameObject.GetComponent<MonsterMash>();
        manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        rigid = GetComponent<Rigidbody>();
        yeetIt = true;
        placed = false;
        markers = 0;
        menuUp = true;
        caveIn = false;
        collapseTriggered = false;
        disabled = false;
        playerSoundLevel = 0.0f;
        flashBat = flashMax;
    }

    void Update()
    {
        //Cave in cutscene: Forced look at entrance
        if (disabled && !caveIn) {
            Vector3 target = (entrance - transform.position).normalized;
            Quaternion rotato = Quaternion.LookRotation(target);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotato, Time.deltaTime * 4);
        }

        //Controllable
        if (!menuUp && !disabled) {
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

            //Pause
            if (Input.GetKeyDown(KeyCode.Escape)) {
                menuUp = true;
                manager.MenuState(2);
            }

            //Flashlight
            if (Input.GetKeyDown(KeyCode.F) && !flipping) {
                flipping = true;
                lamp.gameObject.SetActive(!lamp.gameObject.activeSelf);
                StartCoroutine(flash());
            }

            //Drain flashlight battery, and the intensity does as well
            if (lamp.gameObject.activeSelf && flashBat > 0) {
                flashBat = flashBat - Time.deltaTime / flashMax * 2.5f;
                lamp.intensity = 2.5f * flashBat / flashMax;
            }

            //Screenshake when close to worm - FIX
            monsterDistance = (monsterPos.position - transform.position).magnitude;
            if (monsterDistance < 64) {
                shakeMonster = (80 - monsterDistance) * 0.003125f;
            } else {
                shakeMonster = 0;
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

    //Raycast above player to detect when in cave
    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, Vector3.up, out RaycastHit hit, 4)) {
            inCave = hit.collider.CompareTag("Cave");
            markPoint = hit.point;
        } else { inCave = false; }
    }

    //Event triggers & Worm
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Worm")) {
            menuUp = true;
            manager.MenuState(2);
        }
        if (collider.gameObject.CompareTag("Event") && !collapseTriggered) {
            collapseTriggered = true;
            StartCoroutine(caveRumble(1));
            StartCoroutine(caveCollapse());
        }
    }

    //Backup for worm bite
    void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.CompareTag("Worm")) {
            menuUp = true;
            manager.MenuState(2);
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

    //Flashlight on/off delay
    IEnumerator flash()
    {
        yield return new WaitForSeconds(0.7f);
        flipping = false;
    }

    IEnumerator caveRumble(float duration)
    {
        //Screenshake over X sec
        Vector3 start = transform.position;
        float elapsed = 0;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            transform.position = start + Random.insideUnitSphere * curve.Evaluate(elapsed / duration);
            yield return null;
        }
        monster.BeginHunt();
        transform.position = start;
        disabled = true;
    }

    IEnumerator caveCollapse()
    {
        yield return new WaitForSeconds(6);
        disabled = false;
        caveIn = true;
    }
}