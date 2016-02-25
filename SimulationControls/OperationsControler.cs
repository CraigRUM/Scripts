using UnityEngine;
using System.Collections;

public class OperationsControler : MonoBehaviour {

    Vector3 velocity;
    Transform targetMob;
    Rigidbody myRigidbody;

    void Start () {
        myRigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        myRigidbody.MovePosition(myRigidbody.position + transform.TransformDirection(velocity) * Time.fixedDeltaTime);
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }
}
