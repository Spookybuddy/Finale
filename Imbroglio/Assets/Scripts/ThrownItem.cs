using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownItem : MonoBehaviour
{
    private Rigidbody physics;
    private Mousing looking;
    private MonsterMash tremor;
    private GameObject playerPos;
    private GameObject tremorPos;

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
        looking = GameObject.Find("Main Camera").GetComponent<Mousing>();
        playerPos = GameObject.Find("Player");
        tremorPos = GameObject.Find("Periscope");
        tremor = tremorPos.GetComponent<MonsterMash>();

        //If the player position is within range at start, that means the item was thrown
        if (Mathf.Abs(playerPos.transform.position.x - transform.position.x) < 3 && Mathf.Abs(playerPos.transform.position.z - transform.position.z) < 3) {
            spawnedIn = false;
            thrown();

            //Set tag for monster to identify
            gameObject.tag = "Thrown";
        }
    }

    void Update()
    {
        if (transform.position.y < -2) {
            Destroy(gameObject);
        }
    }

    //Thrown code
    private void thrown()
    {
        //Find the fastest velocity of the player to affect throw speed
        float boost = Mathf.Max(Mathf.Abs(playerPos.GetComponent<Rigidbody>().velocity.x)/8 + 1, Mathf.Abs(playerPos.GetComponent<Rigidbody>().velocity.z)/8 + 1, 1);

        //Add force in direction camera is looking & forward force
        physics.AddRelativeForce(Vector3.up * -5 * Mathf.Sin(looking.looks), ForceMode.Impulse);
        physics.AddRelativeForce(Vector3.forward * 15 * boost, ForceMode.Impulse);

        //Add some spin
        physics.AddRelativeTorque(Vector3.right * Random.Range(0.25f, 0.75f), ForceMode.Impulse);
    }

    private void noise(AudioClip clip)
    {
        //Playsound volume based on distance to player
        sound.PlayOneShot(clip, Mathf.Min(1, 2/distanceCalc(playerPos)));
    }

    //Get distance for different objects
    private float distanceCalc(GameObject target)
    {
        float posX = Mathf.Pow(transform.position.x - target.transform.position.x, 2);
        float posY = Mathf.Pow(transform.position.y - target.transform.position.y, 2);
        float posZ = Mathf.Pow(transform.position.z - target.transform.position.z, 2);
        float distance = Mathf.Sqrt(posX + posY + posZ);
        return distance;
    }

    //Broadcast location on collision
    void OnCollisionEnter()
    {
        if (!spawnedIn) {
            //Change tag so player can pickup again
            gameObject.tag = "Throwable";

            //Tell monster to search for sound at location
            tremor.itemHit = true;
            tremor.itemDistance = Mathf.Min(2, 16.0f/distanceCalc(tremorPos));
            tremor.goTowards = transform.position;

            //Playsound on impact
            if (!breakable && !spawnedIn) {
                noise(hitSound);
            }

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
            int dmg = 0;
            if (breakable) {
                dmg = 13;
            } else {
                dmg = 7;
            }
            tremor.health = tremor.health - dmg;
        }
    }

    //Destroy breakables failsafe
    IEnumerator failsafe()
    {
        yield return new WaitForSeconds(0.8f);
        Destroy(gameObject);
    }
}