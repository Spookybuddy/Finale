using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownItem : MonoBehaviour
{
    private Rigidbody physics;
    private Mousing looking;

    void Start()
    {
        physics = GetComponent<Rigidbody>();
        looking = GameObject.Find("Main Camera").GetComponent<Mousing>();

        //Angle in direction of camera
        physics.AddRelativeForce(Vector3.up * -5 * Mathf.Sin(looking.looks), ForceMode.Impulse);
        physics.AddRelativeForce(Vector3.forward * 15, ForceMode.Impulse);

        //TEMP >>> Destroy after set time >>> Destroy when hit object & make sound
        StartCoroutine(die());
    }

    IEnumerator die()
    {
        yield return new WaitForSeconds(1.8f);
        Destroy(gameObject);
    }
}
