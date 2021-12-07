using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormBite : MonoBehaviour
{
    private Rigidbody rigid;

    //Pop out of ground
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);
    }

    //Delete when back underground
    void Update()
    {
        if (transform.position.y < -15) {
            Destroy(gameObject);
        }
    }
}