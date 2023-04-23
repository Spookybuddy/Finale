using UnityEngine;

public class WormBite : MonoBehaviour
{
    public float force;
    public Rigidbody rigid;

    void Start()
    {
        rigid.AddForce(Vector3.up * force, ForceMode.Impulse);
    }

    //Delete when back underground
    void Update()
    {
        if (transform.position.y < -6) Destroy(gameObject);
    }
}