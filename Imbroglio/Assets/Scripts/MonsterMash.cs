using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMash : MonoBehaviour
{
    private Rigidbody rigid;

    private GameObject tracking;
    private Movit player;
    public GameObject worm;
    public GameObject effects;

    public int health;
    public bool hunting;
    public bool itemHit;
    public bool searching;
    public float itemDistance;
    public float playerThreshold = 0.2f;
    public Vector3 goTowards;

    private bool nudge;
    private float spd;

    private Vector3 prevention;
    private Vector3 attackSpot;
    private bool attacking;

    private AudioSource sounds;
    public AudioClip[] growls;
    private bool doNoise;

    private float pan;
    private const float HALFPI = 1.57079632679489661923132169164f;
    private const float INVERSE = 57.2957795130823208767981548141f;
    private const float NEGATE = -0.69813170079773212f;
    private const float APPROX = 0.87266462599716477f;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        tracking = GameObject.Find("Player");
        player = tracking.GetComponent<Movit>();
        sounds = GetComponent<AudioSource>();
        nudge = true;
        attacking = false;
        health = 91;
        effects.gameObject.SetActive(true);
        StartCoroutine(noises());
    }

    void Update()
    {
        //Stop when killed
        if (health <= 0) {
            //Stop particles
            effects.gameObject.SetActive(false);
        }

        if (!player.menuUp && health > 0) {
            //Movement type speeds
            if (hunting) {
                spd = 8.0f;
                goTowards = tracking.transform.position;
            } else if (searching) {
                spd = 4.0f;
            } else {
                spd = 2.0f;
            }

            //Calc player sound based on distance
            if (player.playerSoundLevel > playerThreshold) {
                float noise = 8 / Vector3.Distance(transform.position, tracking.transform.position);
                noise *= player.playerSoundLevel;
                if (noise > playerThreshold) {
                    hunting = true;
                    searching = false;
                } else if (noise < playerThreshold / 2) {
                    hunting = false;
                }
            }

            //Calc thrown sound based on distance
            if (itemHit) {
                if (itemDistance > playerThreshold) {
                    hunting = false;
                    searching = true;
                } else {
                    itemHit = false;
                }
            }

            //When reaching the target position
            if (Vector3.Distance(transform.position, goTowards) < 5 && !attacking) {
                goTowards = new Vector3(500, 0, 500);
                rigid.velocity = Vector3.zero;
                attackSpot = transform.position;
                attacking = true;
                searching = false;
                hunting = false;
                itemHit = false;

                //Worm bite
                GameObject[] heads = GameObject.FindGameObjectsWithTag("Worm");
                foreach (GameObject head in heads) {
                    Destroy(head);
                }
                Instantiate(worm, new Vector3(attackSpot.x, -12, attackSpot.z), transform.rotation);
            }

            //Attack here
            if (attacking) {
                transform.position = attackSpot;
                effects.gameObject.SetActive(false);
                StartCoroutine(attackDuration());
            }

            //Play constant audio out of 3 growls
            if (doNoise) {
                //calc distance to player, and player can only hear the monster when within 80m of monster
                float toPlayer = Vector3.Distance(transform.position, tracking.transform.position);
                sounds.pitch = (Random.Range(0.875f, 1.125f));
                sounds.PlayOneShot(growls[Random.Range(0, growls.Length)], Mathf.Clamp01(12 / toPlayer - 0.15f));
                doNoise = false;
                StartCoroutine(noises());
            }

            //Nudge it 
            while (nudge && !attacking) {
                float veloX;
                float veloZ;
                float delay;

                if (hunting) {
                    veloX = goTowards.x - transform.position.x;
                    veloZ = goTowards.z - transform.position.z;
                    delay = 0.25f;
                } else if (searching) {
                    veloX = goTowards.x - transform.position.x;
                    veloZ = goTowards.z - transform.position.z;
                    delay = 0.5f;
                } else {
                    veloX = Random.Range(-6.0f, 6.0f);
                    veloZ = Random.Range(-6.0f, 6.0f);
                    delay = 0.75f;
                }

                rigid.AddForce(new Vector3(veloX, 0, veloZ).normalized * spd, ForceMode.Impulse);
                rigid.velocity = new Vector3(Mathf.Clamp(rigid.velocity.x, -10, 10), 0, Mathf.Clamp(rigid.velocity.z, -10, 10));

                nudge = false;
                StartCoroutine(waitForce(delay));
            }
        } else {
            //Lock velocity when paused
            rigid.velocity = Vector3.zero;
        }
    }

    //Do the math for pan stereo here to be less computational
    void FixedUpdate()
    {
        //Pan stereo constantly keeping track of player position relative to monster
        //Sin( (Player Rotation - Inverse Cos (X dist / Distance) * sign(Y dist) * 57.29578) / 90) * 1.5707963268)
        float Xdist = (tracking.transform.position.x - transform.position.x);
        float Zdist = (tracking.transform.position.z - transform.position.z);
        Xdist = Xdist / SquareRoot((Xdist * Xdist) + (Zdist * Zdist));
        Zdist = Mathf.Sign(Zdist) * INVERSE;
        pan = Mathf.Cos((tracking.transform.eulerAngles.y - ArcSub(Xdist) * Zdist) / 90 * HALFPI);
        sounds.panStereo = pan;
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
        a = (a + x / a) / 2;
        a = (a + x / a) / 2;
        a = (a + x / a) / 2;
        a = (a + x / a) / 2;
        a = (a + x / a) / 2;
        //a = (a + x / a) / 2;
        return a;
    }

    //Monster gives up after hitting the same area again
    IEnumerator giveUpTime(float time)
    {
        yield return new WaitForSeconds(time);
        searching = false;
        itemHit = false;
    }

    IEnumerator waitForce(float time)
    {
        yield return new WaitForSeconds(time);
        nudge = true;
    }

    IEnumerator attackDuration()
    {
        yield return new WaitForSeconds(1.5f);
        attacking = false;
        effects.gameObject.SetActive(true);
    }

    IEnumerator noises()
    {
        yield return new WaitForSeconds(2.75f);
        doNoise = true;
    }
}