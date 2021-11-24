using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shardicles : MonoBehaviour
{
    private Rigidbody rigid;

    //Spawn shards in with random force and torque
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.AddForce(new Vector3(Random.Range(-2.0f, 2.0f), Random.Range(-0.5f, 1.5f), Random.Range(-2.0f, 2.0f)), ForceMode.Impulse);
        rigid.AddTorque(new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-0.5f, 0.5f), Random.Range(-1.0f, 1.0f)), ForceMode.Impulse);
        StartCoroutine(delete());
    }

    //Delete after 1.5 sec
    IEnumerator delete()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}