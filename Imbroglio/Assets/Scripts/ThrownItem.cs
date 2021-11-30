using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownItem : MonoBehaviour
{
    private Rigidbody physics;
    private Mousing looking;
    private GameObject playerPos;
    private GameObject tremorPos;

    private AudioSource sound;
    public AudioClip hitSound;

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

        //If the player position is within range at start, that means the item was thrown
        if (Mathf.Abs(playerPos.transform.position.x - transform.position.x) < 3 && Mathf.Abs(playerPos.transform.position.z - transform.position.z) < 3) {
            spawnedIn = false;
            thrown();
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

    private void noise()
    {
        //TEMP: Playsound volume based on distance to player
        //sound.PlayOneShot(hitSound, Mathf.Min(1, 2/distanceCalc(playerPos)));
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
            //Breakables do damage if monster ate it
            //Monster takes more damage when hit outside by non-breakables

            //Check if it hit monster

            //Destroy if monster ate it

            //Tell monster to search for sound at location
            MonsterMash tremor = tremorPos.GetComponent<MonsterMash>();
            tremor.itemHit = true;
            tremor.itemDistance = Mathf.Min(2, 16 / distanceCalc(tremorPos));
            tremor.goTowards = transform.position;

            //noise();

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
        if (breakable && !spawnedIn) {
            Destroy(gameObject);
        }
    }

    //Destroy breakables failsafe
    IEnumerator failsafe()
    {
        yield return new WaitForSeconds(0.8f);
        Destroy(gameObject);
    }
}