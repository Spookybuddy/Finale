using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicBox : MonoBehaviour
{
    private AudioSource sound;
    public AudioClip hit;

    private GameObject player;

    void Start()
    {
        sound = GetComponent<AudioSource>();
        player = GameObject.Find("Player");
        noise(hit);

        //Live long enough to play sound
        StartCoroutine(decay());
    }

    private void noise(AudioClip clip)
    {
        //Playsound volume based on distance to player
        sound.PlayOneShot(clip, Mathf.Min(1, 2 / distanceCalc(player)));
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

    IEnumerator decay()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}