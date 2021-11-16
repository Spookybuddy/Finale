using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mousing : MonoBehaviour
{
    private float tilt;
    private float pi = 3.14159f;
    private bool flipping;
    private Movit script;

    public GameObject player;
    public Light lamp;

    public float looks;

    void Start()
    {
        //Hide mouse (Unhide when paused/menu)
        Cursor.visible = false;
        script = GameObject.Find("Player").GetComponent<Movit>();
    }

    void Update()
    {
        //Use a sin wave to add camera bob
        float playY = 1.75f + (Mathf.Sin(script.stepping * pi) / 8);

        //Get player position
        float playX = transform.position.x;
        float playZ = transform.position.z;
        transform.position = new Vector3(playX, playY, playZ);

        //Tilt camera on X axis (Up/Down)
        tilt = Input.GetAxis("Mouse Y");
        looks = (pi * transform.localEulerAngles.x / 60);

        //Prevent looking too high/low
        if (transform.localEulerAngles.x - tilt > 30 && transform.localEulerAngles.x - tilt < 180) {
            transform.localEulerAngles = new Vector3(30, transform.localEulerAngles.y, 0);
        } else if (transform.localEulerAngles.x - tilt > 180 && transform.localEulerAngles.x - tilt < 330) {
            transform.localEulerAngles = new Vector3(330, transform.localEulerAngles.y, 0);
        } else {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x - tilt, transform.localEulerAngles.y, 0);
        }

        //Flashlight
        if (Input.GetKeyDown(KeyCode.E) && !flipping) {
            flipping = true;
            lamp.gameObject.SetActive(!lamp.gameObject.activeSelf);
            StartCoroutine(flash());
        }

        //Flashlight intensity decreases when looking down to reduce over saturation
        lamp.intensity = 2 - Mathf.Sin(looks);
    }

    //Flashlight on/off delay
    IEnumerator flash()
    {
        yield return new WaitForSeconds(0.7f);
        flipping = false;
    }
}