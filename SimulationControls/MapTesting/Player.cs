using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity {

    public event System.Action TogglePause;

    public float mouseSensitivityX = 50f;
    public float mouseSensitivityY = 50f;
    public float movementSpeed = 5f;
    public float jumpForce = 150f;
    Rigidbody rb;

    Vector3 moveAmount;
    Vector3 smoothMoveVelocity;

    Transform viewCamera;
    float verticalLookRotation;

    bool locked;
    PlayerController controller;
    GunController gunController;

    protected override void Start(){
        base.Start();
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        rb = GetComponent<Rigidbody>();
        viewCamera = Camera.main.transform;
    }

    void DidLockCursor()
    {
        Debug.Log("Locking cursor");
        Cursor.lockState = CursorLockMode.Locked;
        locked = true;
    }
    void DidUnlockCursor()
    {
        Debug.Log("Unlocking cursor");
        Cursor.lockState = CursorLockMode.None;
        locked = false;
    }

    void Update()
    {
        //Top Down
        /*
        // Movment
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * movementSpeed;
        controller.Move(moveVelocity);

        // Look
        Ray ray = viewCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        Plane Ground = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;
        */
        // Movment
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 targetMoveAmount = moveDir * movementSpeed;
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);
        controller.Move(moveAmount);

        // Look
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivityX);
        verticalLookRotation += Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivityY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -45, 45);
        viewCamera.localEulerAngles = Vector3.left * verticalLookRotation;

        if (Input.GetKeyDown("p"))
        {
            TogglePause();
        }

        if (Input.GetMouseButton(1))
        {
            gunController.Shoot();
        }

        if (Input.GetKeyDown("1"))
        {
            RaycastHit lookHit = new RaycastHit();
            Ray lookRay = viewCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(lookRay.origin, lookRay.direction, Color.green, 30f);
            if (Physics.Raycast(lookRay, out lookHit, 30f))
            {
                Debug.Log(lookHit.collider.gameObject.ToString());
                if (lookHit.collider.gameObject.name == "surface")
                {
                    controller.ToggleHeight(lookHit.collider.gameObject.GetComponentInParent<Terrain>());

                }
            }
        }

        if (Input.GetKeyDown("e"))
        {
            RaycastHit lookHit = new RaycastHit();
            Ray lookRay = viewCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(lookRay.origin, lookRay.direction, Color.green, 1000f);
            if (Physics.Raycast(lookRay, out lookHit, 1000f))
            {
                Debug.Log(lookHit.collider.gameObject.ToString());
                if (lookHit.collider.gameObject.name == "surface") {
                    Debug.Log(lookHit.collider.gameObject.tag);
                    controller.Gather(lookHit.collider.gameObject);

                }
            }
        }

        if (Input.GetButtonDown("Jump"))
        {
            rb.AddForce(transform.up * jumpForce);
        }

        /*
        if (Ground.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            controller.LookAt(point);
        }
        */
        //Wepon input
        if (Input.GetKeyDown("f"))
        {
            controller.FertilizeBlock();
        }

        if (Input.GetKeyDown("r"))
        {
            controller.PickFruit();
        }

        if (Input.GetKeyDown("l"))
        {
            if (locked == true)
            {
                DidUnlockCursor();
            }
            else {
                DidLockCursor();
            }
        }

    }


}
