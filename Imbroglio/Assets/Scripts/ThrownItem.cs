using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownItem : MonoBehaviour
{
    private Rigidbody physics;
    private Mousing looking;
    private GameObject playerPos;
    //private GameObject tremorPos;
    //private GameObject luminePos;

    private AudioSource sound;
    public AudioClip hitSound;

    void Start()
    {
        //Establish basics
        physics = GetComponent<Rigidbody>();
        sound = GetComponent<AudioSource>();
        looking = GameObject.Find("Main Camera").GetComponent<Mousing>();
        playerPos = GameObject.Find("Player");
        //tremorPos = GameObject.Find("Tremor");
        //luminePos = GameObject.Find("Lumine");

        //Add force in direction camera is looking & forward force
        physics.AddRelativeForce(Vector3.up * -5 * Mathf.Sin(looking.looks), ForceMode.Impulse);
        physics.AddRelativeForce(Vector3.forward * 15, ForceMode.Impulse);

        //Add some spin
        physics.AddRelativeTorque(Vector3.right * Random.Range(0.25f, 0.75f), ForceMode.Impulse);

        //Destroy if object hasn't been destroyed after 3sec (Covers clipping out)
        StartCoroutine(deathFailsafe());
    }

    private void noise()
    {
        //TEMP: Playsound volume based on distance to player and monster
        //distanceCalc(tremorPos);
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

    //Destroy after hitting something
    void OnCollisionExit()
    {
        //OnCollisionExit because it looks better (Bounces off, instead of vanishing)
        Debug.Log("Distance: " + distanceCalc(playerPos));
        Debug.Log("Volume: " + Mathf.Min(1, 2 / distanceCalc(playerPos)));
        //Particle here
        //noise();
        Destroy(gameObject);
    }

    IEnumerator deathFailsafe()
    {
        yield return new WaitForSeconds(3.0f);
        Destroy(gameObject);
    }
}
