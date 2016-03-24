using UnityEngine;
using System.Collections;

[RequireComponent(typeof(OperationsControler))]
public class SimOperator : MonoBehaviour {

    public event System.Action TogglePause;
    //Interface config variables
    public float mouseSensitivityX = 20f;
    public float mouseSensitivityY = 20f;
    public float movementSpeed = 5f;

    //Inspection variables
    string selectionData;
    IInspectable currentlySelected;
    //Object inspector objects
    public GameObject[] Displayobjects = new GameObject[9];

    //Movment variables
    Vector3 moveAmount;
    Vector3 smoothMoveVelocity;
    bool locked = false;
    Transform targetMob;

    //Camara variables
    Transform viewCamera;
    float veiwheight = 10;
    float verticalLookRotation;
    float horizontalLookRotation;

    OperationsControler controller;

    //Camara and Controls Setup
    void Start () {
        controller = GetComponent<OperationsControler>();
        viewCamera = Camera.main.transform;
        Camera.main.orthographicSize = veiwheight;
    }

    //Removes dead animats from inspection gui
    void onfllowingDeath() {
        Displayobjects[0].SetActive(true);
        locked = false;

    }

	// Determins the Users movment and actions based on frame by frame input
	void Update () {

        Vector3 moveDir;
        Vector3 targetMoveAmount;
        //Camara Movment controls
        if (locked == true && targetMob != null)
        {
            transform.rotation = Quaternion.Euler(0,0,0);
            moveDir = (targetMob.position - transform.position).normalized;
            
            targetMoveAmount = moveDir * movementSpeed;
            moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .2f);
            controller.Move(moveAmount);
        }
        else {
            moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
            targetMoveAmount = moveDir * movementSpeed;
            moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .2f);
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

        if (Input.GetMouseButton(1) && locked != true)
        {
                transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivityX);
        }

        if (Input.GetMouseButton(2))
        {
            verticalLookRotation += Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivityY;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, 30, 90);
            viewCamera.localEulerAngles = new Vector3(verticalLookRotation, viewCamera.localEulerAngles.y, viewCamera.localEulerAngles.z);
        }

        if (Input.GetMouseButtonUp(0))
        {
            foreach(GameObject DisplayItem in Displayobjects) { DisplayItem.SetActive(false); }
            RaycastHit lookHit = new RaycastHit();
            Ray lookRay = viewCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(lookRay.origin, lookRay.direction, Color.green, 200f);
            if (Physics.Raycast(lookRay, out lookHit, 200f))
            {
                
                Debug.Log(lookHit.collider.gameObject.ToString() + "Locking on to this");
                currentlySelected = lookHit.collider.gameObject.GetComponent<IInspectable>();

                if (currentlySelected != null)
                {
                    selectionData = currentlySelected.BeInspected();
                    SimControls.UpdateSelection(selectionData);
                    StartCoroutine(UpdataSelectionData());
                }
                else {
                    currentlySelected = null;
                    selectionData = null;
                    SimControls.UpdateSelection(selectionData);
                }

                if (lookHit.collider.gameObject.GetComponent<LivingEntity>() == true)
                {
                    locked = true;

                    targetMob = lookHit.collider.gameObject.transform;

                    Displayobjects[0].SetActive(true);
                    Displayobjects[0].transform.localScale = targetMob.transform.localScale;
                    Displayobjects[0].GetComponent<Renderer>().material = targetMob.gameObject.GetComponent<Renderer>().material;
                    targetMob.GetComponent<LivingEntity>().OnDeath += onfllowingDeath;

                    transform.position = new Vector3(targetMob.position.x, transform.position.y, targetMob.position.z);
                    viewCamera.localEulerAngles = new Vector3(90, viewCamera.localEulerAngles.y, viewCamera.localEulerAngles.z);
                    transform.Rotate(Vector3.up);
                }
                else if (lookHit.collider.gameObject.GetComponent<AnimatEssence>() == true)
                {
                    Displayobjects[1].SetActive(true);
                }
                else if (lookHit.collider.gameObject.GetComponent<Terrain>() == true)
                {
                    Terrain currentTerrain = lookHit.collider.gameObject.GetComponent<Terrain>();
                    if (currentTerrain.isWater == true)
                    {
                        Displayobjects[2].SetActive(true);
                    }
                    else
                    {
                        Displayobjects[3].SetActive(true);
                    }
                }
                else if (lookHit.collider.gameObject.GetComponentInParent<PrimaryProducer>() == true)
                {
                    /*switch (lookHit.collider.gameObject.name) {
                        
                    }*/
                }
                else { locked = false; selectionData = null; }
            }
        }


        if (Input.GetKeyDown("p"))
        {
            TogglePause();
        }

        if (Input.GetKeyDown("-") || Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            veiwheight -= 5;
            veiwheight = Mathf.Clamp(veiwheight, 10, 50);
            transform.position = new Vector3(transform.position.x, veiwheight, transform.position.z);
            //Camera.main.orthographicSize = veiwheight;
        }

        if (Input.GetKeyDown("+") || Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            veiwheight += 5; ;
            veiwheight = Mathf.Clamp(veiwheight, 10, 50);
            transform.position = new Vector3(transform.position.x, veiwheight, transform.position.z);
            //Camera.main.orthographicSize = veiwheight;
        }

        if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
        {
            locked = false;
        }

    }

    //Updates object inspection gui
    IEnumerator UpdataSelectionData()
    {
        while (currentlySelected !=  null)
        {
            if (currentlySelected != null) { 
                selectionData = currentlySelected.BeInspected();
            }
            else { selectionData = null; }
            SimControls.UpdateSelection(selectionData);

            yield return new WaitForSeconds(0.5f);
        }
    }

}
