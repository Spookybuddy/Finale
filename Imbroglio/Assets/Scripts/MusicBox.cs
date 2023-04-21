using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicBox : MonoBehaviour
{
    public AudioSource sound;
    public AudioClip hit;
    private GameObject player;
    private const float HALFPI = 1.57079632679489662f;
    private const float INVERSE = 57.29577951308232087f;
    private const float NEGATE = -0.69813170079773212f;
    private const float APPROX = 0.87266462599716477f;

    void Start()
    {
        player = GameObject.FindWithTag("Player");

        //Sin( (Player Rotation - Inverse Cos (X dist / Distance) * sign(Y dist) * 57.29578) / 90) * 1.5707963268)
        float Xdist = (player.transform.position.x - transform.position.x);
        float Zdist = (player.transform.position.z - transform.position.z);
        float volume = SquareRoot((Xdist * Xdist) + (Zdist * Zdist));
        Xdist = Xdist / volume;
        Zdist = Mathf.Sign(Zdist) * INVERSE;
        sound.panStereo = Mathf.Cos((player.transform.eulerAngles.y - ArcSub(Xdist) * Zdist) / 90 * HALFPI);
        sound.PlayOneShot(hit, Mathf.Min(1, 4 / volume));

        //Live long enough to play sound
        StartCoroutine(decay());
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

    //Destroy after 1.5sec
    IEnumerator decay()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}