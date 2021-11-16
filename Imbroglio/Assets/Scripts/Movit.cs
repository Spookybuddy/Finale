using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movit : MonoBehaviour
{
    private float turn;
    private float strafe;
    private float advance;
    public float stepping;

    private Rigidbody rigid;
    private float veloCap;
    private float bobby;

    private bool kickIt;

    public List<GameObject> inventory;
    private bool yeetIt;
    private float pi = 3.14159f;
    private Mousing lookingAt;

    void Start()
    {
        lookingAt = GameObject.Find("Main Camera").GetComponent<Mousing>();
        rigid = GetComponent<Rigidbody>();
        kickIt = true;
        yeetIt = true;
    }

    void Update()
    {
        //Mouse Camera (Left/Right)
        turn = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up, turn);

        //Strafing
        strafe = Input.GetAxis("Horizontal");
        rigid.AddRelativeForce(Vector3.right * strafe * Time.deltaTime * (veloCap/2), ForceMode.Impulse);

        //Walk forward
        advance = Input.GetAxis("Vertical");
        rigid.AddRelativeForce(Vector3.forward * advance * Time.deltaTime * (veloCap/2), ForceMode.Impulse);

        //Velocity cap
        rigid.angularVelocity = Vector3.zero;
        rigid.velocity = new Vector3(Mathf.Max(Mathf.Min(rigid.velocity.x, veloCap), -veloCap), 0, Mathf.Max(Mathf.Min(rigid.velocity.z, veloCap), -veloCap));

        //Sneak -> Sprint -> Walk >>> CHANGE TO INPUT MANAGER
        if (Input.GetKey(KeyCode.LeftShift)) {
            veloCap = 1;
            bobby = 0.5f;
        } else if (Input.GetKey(KeyCode.Space)) {
            veloCap = 8;
            bobby = 1.5f;
        } else {
            veloCap = 4;
            bobby = 1;
        }

        //Camera bob wave based on speed
        if (advance != 0 || strafe != 0) {
            stepping = stepping + (1/(100/bobby));
        }

        //Kick attack >>> CHANGE TO INPUT MANAGER
        if (Input.GetKeyDown(KeyCode.R) && kickIt) {
            kickIt = false;
            StartCoroutine(kickReset());
        }

        //Throw item >>> CHANGE TO INPUT MANAGER
        if (Input.GetKeyDown(KeyCode.Q) && yeetIt && inventory.Count > 0) {
            yeetIt = false;
            StartCoroutine(yeetDelay());

            //Spawn item based on direction looking both horizontal & vertical
            float waves = (pi * transform.eulerAngles.y / 180);
            Instantiate(inventory[0], transform.position + new Vector3(1.3f * Mathf.Sin(waves), -Mathf.Sin(lookingAt.looks), 1.3f * Mathf.Cos(waves)), transform.rotation);

            //Remove thrown item from inventory
            //inventory.RemoveAt(0);
        }
    }

    //Kick reset delay
    IEnumerator kickReset()
    {
        yield return new WaitForSeconds(0.8f);
        kickIt = true;
    }

    //Throw reset delay
    IEnumerator yeetDelay()
    {
        yield return new WaitForSeconds(1.2f);
        yeetIt = true;
    }
}