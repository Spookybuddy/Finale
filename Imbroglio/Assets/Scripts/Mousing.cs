using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mousing : MonoBehaviour
{
    private Movit script;
    private float tilt;
    private bool flipping;

    public Light lamp;
    public float looks;
    public float flashBat;
    public float flashMax;

    void Start()
    {
        //Hide mouse (Unhide when paused/menu)
        Cursor.lockState = CursorLockMode.Locked;
        flashMax = 45.0f;
        flashBat = flashMax;
        script = GameObject.Find("Player").GetComponent<Movit>();
    }

    void Update()
    {
        if (!script.menuUp) {
            //Get player position
            float playX = transform.position.x;
            float playZ = transform.position.z;
            transform.position = new Vector3(playX, 2.333f, playZ);

            //Tilt camera on X axis (Up/Down)
            tilt = Input.GetAxis("Mouse Y") / 2;
            looks = (3.14159265f * transform.localEulerAngles.x / 60);

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