using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMash : MonoBehaviour
{
    private Rigidbody rigid;

    private GameObject tracking;
    private Movit playerNoise;

    public bool hunting;

    public bool searching;
    public Vector3 goTowards;

    private bool nudge;
    private float spd;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        tracking = GameObject.Find("Player");
        playerNoise = tracking.GetComponent<Movit>();
        nudge = true;
    }

    void Update()
    {
        if (hunting) {
            spd = 7.5f;
        } else if (searching) {
            spd = 5.0f;
        } else {
            spd = 2.5f;
        }

        while (nudge) {
            float veloX;
            float veloZ;
            float delay;

            if (hunting) {
                veloX = tracking.transform.position.x - transform.position.x;
                veloZ = tracking.transform.position.z - transform.position.z;
                delay = 0.4f;
            } else if (searching) {
                veloX = goTowards.x - transform.position.x;
                veloZ = goTowards.z - transform.position.z;
                delay = 0.6f;
            } else {
                veloX = Random.Range(-6.0f, 6.0f);
                veloZ = Random.Range(-6.0f, 6.0f);
                delay = 0.8f;
            }

            rigid.AddForce(new Vector3(veloX, 0, veloZ).normalized * spd, ForceMode.Impulse);
            rigid.velocity = new Vector3(Mathf.Clamp(rigid.velocity.x, -10, 10), 0, Mathf.Clamp(rigid.velocity.z, -10, 10));

            nudge = false;
            StartCoroutine(waitForce(delay));
        }

    }

    IEnumerator waitForce(float time)
    {
        yield return new WaitForSeconds(time);
        nudge = true;
    }
}
