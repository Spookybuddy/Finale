using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownItem : MonoBehaviour
{
    private Rigidbody physics;
    private Mousing looking;
    private MonsterMash tremor;
    private Rigidbody player;
    private Transform playerPos;
    private Transform tremorPos;

    private AudioSource sound;
    public AudioClip hitSound;
    public GameObject music;

    public bool breakable;
    public bool spawnedIn;
    public List<GameObject> shatterPieces;

    void Start()
    {
        //Establish basics
        physics = GetComponent<Rigidbody>();
        sound = GetComponent<AudioSource>();
        looking = GameObject.FindWithTag("MainCamera").GetComponent<Mousing>();
        playerPos = GameObject.FindWithTag("Player").transform;
        tremorPos = GameObject.FindWithTag("Monster").transform;
        player = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();
        tremor = GameObject.FindWithTag("Monster").GetComponent<MonsterMash>();

        //If the player position is within range at start, that means the item was thrown
        if (Vector3.Distance(playerPos.position, transform.position) < 3) {
            spawnedIn = false;
            Thrown();

            //Set tag for monster to identify
            gameObject.tag = "Thrown";
        }
    }

    //Check if out of bounds
    void FixedUpdate()
    {
        if (transform.position.y < -2) Destroy(gameObject);
    }

    //Thrown code
    private void Thrown()
    {
        //Find the fastest velocity of the player to affect throw speed
        float boost = Mathf.Max(Mathf.Abs(player.velocity.x)/8 + 1, Mathf.Abs(player.velocity.z)/8 + 1, 1);

        //Add force in direction camera is looking & forward force
        physics.AddRelativeForce(Vector3.up * -5 * Mathf.Sin(looking.looks), ForceMode.Impulse);
        physics.AddRelativeForce(Vector3.forward * 15 * boost, ForceMode.Impulse);

        //Add some spin
        physics.AddRelativeTorque(Vector3.right * Random.Range(0.25f, 0.75f), ForceMode.Impulse);
    }

    //Playsound volume based on distance to player
    private void noise(AudioClip clip)
    {
        sound.PlayOneShot(clip, Mathf.Min(1, 2/distanceCalc(playerPos.position)));
    }

    //Get distance for different objects
    private float distanceCalc(Vector3 target)
    {
        float x = (Mathf.Pow(target.x - transform.position.x, 2) + Mathf.Pow(target.z - transform.position.z, 2));
        float a = 125;
        for (int b = 0; b < 5; b++) { a = (a + x / a) / 2; }
        return a;
    }

    //Broadcast location on collision
    void OnCollisionEnter()
    {
        if (!spawnedIn) {
            //Change tag so player can pickup again
            gameObject.tag = "Throwable";

            //Tell monster to search for sound at location
            tremor.UpdateSound(Mathf.Min(2, 16 / distanceCalc(tremorPos.position)), transform.position);

            //Playsound on impact
            if (!breakable && !spawnedIn) noise(hitSound);

            //Spawn some fragments because I hate the particle system in Unity
            if (breakable) {
                for (int a = 0; a < 7; a++) {
                    Instantiate(shatterPieces[Random.Range(0, shatterPieces.Count)], transform.position, transform.rotation);
                }
                StartCoroutine(failsafe());
            }
        }
    }

    //OnCollisionExit because it looks better (Bounces off, instead of vanishing)
    void OnCollisionExit()
    {
        //If breakable object, destroy after impact
        if (breakable && !spawnedIn) {
            //Spawn in a music box to play the full sound. Would get cut off due to destroying object
            Instantiate(music, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    //Check to see if hit worm after being thrown
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Worm") && gameObject.CompareTag("Thrown")) {
            tremor.health = tremor.health - (breakable ? 13 : 7);
        }
    }

    //Destroy breakables failsafe
    IEnumerator failsafe()
    {
        yield return new WaitForSeconds(0.8f);
        Destroy(gameObject);
    }
}