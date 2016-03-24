using UnityEngine;
using System.Collections;

public class OperationsControler : MonoBehaviour {

    Vector3 velocity;
    Transform targetMob;
    Rigidbody myRigidbody;

    //initilization
    void Start () {
        myRigidbody = GetComponent<Rigidbody>();
    }

    //Ridged body movment
    void FixedUpdate()
    {
        myRigidbody.MovePosition(myRigidbody.position + transform.TransformDirection(velocity) * Time.fixedDeltaTime);
    }

    //Movment input
    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }
}
