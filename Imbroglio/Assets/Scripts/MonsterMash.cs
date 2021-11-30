using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMash : MonoBehaviour
{
    private Rigidbody rigid;

    private GameObject tracking;
    private Movit player;

    public bool hunting;
    public bool itemHit;
    public bool searching;
    public float itemDistance;
    public float playerThreshold = 0.15f;
    public Vector3 goTowards;

    private bool nudge;
    private float spd;

    private Vector3 prevention;
    private Vector3 attackSpot;
    private bool attacking;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        tracking = GameObject.Find("Player");
        player = tracking.GetComponent<Movit>();
        transform.position = new Vector3(Random.Range(-127, -384), 25, Random.Range(-127, 128));
        nudge = true;
        attacking = false;
    }

    void Update()
    {
        //Movement type speeds
        if (hunting) {
            spd = 6.0f;
            goTowards = tracking.transform.position;
        } else if (searching) {
            spd = 4.0f;
        } else {
            spd = 2.0f;
        }

        //Calc player sound based on distance
        if (player.playerSoundLevel > playerThreshold) {
            float noise = 8/Mathf.Sqrt(Mathf.Pow(transform.position.x - tracking.transform.position.x, 2) + Mathf.Pow(transform.position.z - tracking.transform.position.z, 2));
            noise = noise * player.playerSoundLevel;
            if (noise > playerThreshold) {
                hunting = true;
                searching = false;
            } else if (noise < playerThreshold/2) {
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
        if (Mathf.Abs(goTowards.x - transform.position.x) < 2.5f && Mathf.Abs(goTowards.z - transform.position.z) < 2.5f && !attacking) {
            goTowards = new Vector3(500, 0, 500);
            rigid.velocity = Vector3.zero;
            attackSpot = transform.position;
            attacking = true;
            searching = false;
            hunting = false;
            itemHit = false;
        }

        //Attack here
        if (attacking) {
            transform.position = attackSpot;
            StartCoroutine(attackDuration());
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
    }

    //Monster gives up after hitting the same area again
    void OnCollisionExit()
    {
        if (Mathf.Abs(prevention.x - transform.position.x) < 3.5f && Mathf.Abs(prevention.z - transform.position.z) < 3.5f) {
            hunting = false;
            searching = false;
            itemHit = false;
        }
        prevention = transform.position;
    }

    IEnumerator waitForce(float time)
    {
        yield return new WaitForSeconds(time);
        nudge = true;
    }

    IEnumerator attackDuration()
    {
        yield return new WaitForSeconds(1);
        attacking = false;
    }
}
