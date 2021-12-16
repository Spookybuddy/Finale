using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mousing : MonoBehaviour
{
    private Movit script;
    private float tilt;

    public Light lamp;
    private bool flipping;

    public float looks;

    public float flashBat;
    public float flashMax;

    void Start()
    {
        //Hide mouse (Unhide when paused/menu)
        Cursor.visible = false;
        flashMax = 45.0f;
        flashBat = flashMax;
        script = GameObject.Find("Player").GetComponent<Movit>();
    }

    void Update()
    {
        if (!script.menuUp) {
            //Use a sin wave to add camera bob
            float playY = 1.8f + (Mathf.Sin(script.stepping * 3.14159f) / 10);

            //Get player position
            float playX = transform.position.x;
            float playZ = transform.position.z;
            transform.position = new Vector3(playX, playY, playZ);

            //Tilt camera on X axis (Up/Down)
            tilt = Input.GetAxis("Mouse Y")/2;
            looks = (3.14159f * transform.localEulerAngles.x / 60);

            //Prevent looking too high/low
            if (transform.localEulerAngles.x - tilt > 30 && transform.localEulerAngles.x - tilt < 180) {
                transform.localEulerAngles = new Vector3(30, transform.localEulerAngles.y, 0);
            } else if (transform.localEulerAngles.x - tilt > 180 && transform.localEulerAngles.x - tilt < 330) {
                transform.localEulerAngles = new Vector3(330, transform.localEulerAngles.y, 0);
            } else {
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x - tilt, transform.localEulerAngles.y, 0);
            }

            //Prevent looking too high/low

            if (transform.localEulerAngles.x - tilt > 30 && transform.localEulerAngles.x - tilt < 180) {
                transform.localEulerAngles = new Vector3(30, transform.localEulerAngles.y, 0);
            } else if (transform.localEulerAngles.x - tilt > 180 && transform.localEulerAngles.x - tilt < 330) {
                transform.localEulerAngles = new Vector3(330, transform.localEulerAngles.y, 0);
            } else {
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x - tilt, transform.localEulerAngles.y, 0);
            }

            //Flashlight
            if (Input.GetKeyDown(KeyCode.F) && !flipping) {
                //play flashlight click noise
                script.playerSoundLevel = 0.8f;
                flipping = true;
                lamp.gameObject.SetActive(!lamp.gameObject.activeSelf);
                StartCoroutine(flash());
            }

            //Drain flashlight battery
            if (lamp.gameObject.activeSelf && flashBat > 0) {
                flashBat = flashBat - Time.deltaTime / flashMax * 2.5f;
            }

            //Brightness decreases with battery
            lamp.intensity = 2.5f * flashBat / flashMax;
        }
    }

    //Flashlight on/off delay
    IEnumerator flash()
    {
        yield return new WaitForSeconds(0.7f);
        flipping = false;
    }
}