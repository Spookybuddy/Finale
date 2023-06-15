using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mousing : MonoBehaviour
{
    private Movit script;
    private float tilt;
    private bool flipping;
    public float looks;

    void Start()
    {
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
        }
    }
}