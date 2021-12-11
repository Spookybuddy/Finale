using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCsCode : MonoBehaviour
{
    private Rigidbody rigid;
    private bool following;

    public List<Vector3> positions;

    private GameObject player;

    void Start()
    {
        following = false;
        positions = new List<Vector3>();
        player = GameObject.Find("Player");
        positions.Add(transform.position);
        rigid = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        //Fill the list with the player positions
        if (following) {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) {
                positions.Insert(0, player.transform.position);
            }
        }

        //Keep list length below 80
        if (positions.Count > 80) {
            positions.RemoveAt(positions.Count-1);
        }

        //Move to the last position in the list
        transform.position = positions[positions.Count-1];
    }

    void OnTriggerStay(Collider collider)
    {
        //Follow player
        if (collider.gameObject.CompareTag("Player")) {
            if ((Input.GetKeyDown(KeyCode.E) || Input.GetAxis("Interact") > 0) && !following) {
                gameObject.tag = "Saved";
                following = true;
            }
        }

        //Worm eat
        if (collider.gameObject.CompareTag("Worm")) {
            Destroy(gameObject);
        }
    }
}