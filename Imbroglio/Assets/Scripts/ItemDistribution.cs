using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDistribution : MonoBehaviour
{
    public GameObject[] items;

    private Vector3 location;
    private Quaternion rotation;
    private GameObject storage;
    private bool delay;

    void Start()
    {
        storage = GameObject.Find("Item Depository");
        delay = true;
    }

    void Update()
    {
        //Do in update to let everything else load
        if (delay) {
            //Spawn 25 pickupables in the caves, with a preference towards rocks
            for (int i = 0; i < 25; i++) {
                location = new Vector3(Random.Range(-10.0f, -500.0f), 0.5f, Random.Range(-240.0f, 240.0f));
                rotation = Quaternion.identity;
                int weighted = Random.Range(1, 4) % 2;
                ChildSpawn(weighted);
            }

            //Spawn 10 pickupables outside, with a preference towards bottles
            for (int i = 0; i < 10; i++) {
                location = new Vector3(100 + Random.Range(-32.0f, 32.0f), 0.5f, Random.Range(-80.0f, 80.0f));
                rotation = Quaternion.identity;
                int weighted = Random.Range(0, 3) % 2;
                ChildSpawn(weighted);
            }
            delay = false;
        }
    }

    private void ChildSpawn(int index)
    {
        GameObject pickup = Instantiate(items[index], location, rotation) as GameObject;
        ThrownItem script = pickup.GetComponent<ThrownItem>();
        script.spawnedIn = true;
        pickup.transform.parent = storage.transform;
    }
}