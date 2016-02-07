using UnityEngine;
using System.Collections;

[RequireComponent(typeof(OperationsControler))]
public class SimOperator : MonoBehaviour {

    public event System.Action TogglePause;

    public float mouseSensitivityX = 50f;
    public float mouseSensitivityY = 10f;
    public float movementSpeed = 5f;
    //public Transform crossHairs;

    Vector3 moveAmount;
    Vector3 smoothMoveVelocity;
    bool locked = false;
    Transform targetMob;

    Transform viewCamera;
    float veiwheight = 10;
    float verticalLookRotation;
    float horizontalLookRotation;

    OperationsControler controller;

    void Start () {
        controller = GetComponent<OperationsControler>();
        viewCamera = Camera.main.transform;
        Camera.main.orthographicSize = veiwheight;
    }
	
	// Update is called once per frame
	void Update () {

        Vector3 moveDir;
        Vector3 targetMoveAmount;
        //Camara Movment controls
        if (locked == true && targetMob != null)
        {
            moveDir = (targetMob.position - transform.position).normalized;
            targetMoveAmount = moveDir * movementSpeed/4;
            moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);
            controller.Move(moveAmount);
        }
        else {
            moveDir = new Vector3(Input.GetAxisRaw("Vertical"), 0, -Input.GetAxisRaw("Horizontal")).normalized;
            targetMoveAmount = moveDir * movementSpeed;
            moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);
            controller.Move(moveAmount);
        }

       //Cross hair implementation
       /* Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane uiPlane = new Plane(Vector3.forward, ((Vector3.forward * 5)));
        float rayDistance;

        if (uiPlane.Raycast(ray, out rayDistance)) {
            Vector3 point = ray.GetPoint(rayDistance);
            crossHairs.position = point;
        }*/

        if (Input.GetMouseButton(1))
        {
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivityX);
        }

        if (Input.GetMouseButton(2))
        {
            verticalLookRotation += Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivityY;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, 30, 90);
            viewCamera.localEulerAngles = new Vector3(verticalLookRotation, viewCamera.localEulerAngles.y, viewCamera.localEulerAngles.z);
        }

        if (Input.GetMouseButton(0))
        {
            RaycastHit lookHit = new RaycastHit();
            Ray lookRay = viewCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(lookRay.origin, lookRay.direction, Color.green, 200f);
            if (Physics.Raycast(lookRay, out lookHit, 200f))
            {
                Debug.Log(lookHit.collider.gameObject.ToString() + "Locking on to this");
                if (lookHit.collider.gameObject.GetComponent<LivingEntity>() == true)
                {
                    locked = true;
                    targetMob = lookHit.collider.gameObject.transform;
                    transform.position = new Vector3(targetMob.position.x, transform.position.y, targetMob.position.z);
                    viewCamera.localEulerAngles = new Vector3(90, viewCamera.localEulerAngles.y, viewCamera.localEulerAngles.z);
                    transform.Rotate(Vector3.up);
                }
        }
        }


        if (Input.GetKeyDown("p"))
        {
            TogglePause();
        }

        if (Input.GetKeyDown("-") || Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            veiwheight -= 4;
            veiwheight = Mathf.Clamp(veiwheight, 10, 50);
            transform.position = new Vector3(transform.position.x, veiwheight, transform.position.z);
            //Camera.main.orthographicSize = veiwheight;
        }

        if (Input.GetKeyDown("+") || Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            veiwheight += 4; ;
            veiwheight = Mathf.Clamp(veiwheight, 10, 50);
            transform.position = new Vector3(transform.position.x, veiwheight, transform.position.z);
            //Camera.main.orthographicSize = veiwheight;
        }

        if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
        {
            locked = false;
        }

    }
}
