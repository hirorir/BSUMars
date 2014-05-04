using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConstructionPiece : MonoBehaviour {
	[SerializeField] protected float conSpeed = 16f; // The controlled placement movement speed of the object.
	[SerializeField] protected float rotSpeed = 90f; // The controlled placement rotation speed of the object.
	private float adjX;
	private float adjZ;
	private bool placing = false; // Whether or not the piece is being placed by the player.
	private bool wasPlaced = false;
	private GameObject player;
	private Camera mainCam;
    private Camera activeCam;
	private List<Camera> conCams;

	// Use this for initialization
	void Start () {
		StartCoroutine(getobjs());
	}

	private IEnumerator getobjs() {
		yield return new WaitForSeconds(0.2f);
		player = GameObject.Find("First Person Character");
		mainCam = GameObject.Find("First Person Camera").GetComponent<Camera>();
        activeCam = mainCam;
        conCams = new List<Camera>();
        foreach(Transform child in transform) {
            if (child.name == "ConstructionCamera")
                conCams.Add(child.gameObject.GetComponent<Camera>());
        }
	}

	void Update() {

	}
	
	void FixedUpdate () {
		if (placing) {
			Vector3 movementVec = new Vector3(0f, 0f, 0f);
			if (Input.GetKey(KeyCode.W)) {
				movementVec += new Vector3(adjZ * Time.deltaTime, 0f, adjX * Time.deltaTime);
				//transform.Translate(adjZ * Time.deltaTime, 0f, adjX * Time.deltaTime);
			} 
			if (Input.GetKey(KeyCode.A)) {
				//transform.Translate(-adjX * Time.deltaTime, 0f, adjZ * Time.deltaTime, Space.World);
				movementVec += new Vector3(-adjX * Time.deltaTime, 0f, adjZ * Time.deltaTime);
			} 
			if (Input.GetKey(KeyCode.S)) {
				//transform.Translate(adjZ * Time.deltaTime, 0f, -adjX * Time.deltaTime, Space.World);
				movementVec += new Vector3(adjZ * Time.deltaTime, 0f, -adjX * Time.deltaTime);
			} 
			if (Input.GetKey(KeyCode.D)) {
				//transform.Translate(adjX * Time.deltaTime, 0f, adjZ * Time.deltaTime, Space.World);
				movementVec += new Vector3(adjX * Time.deltaTime, 0f, adjZ * Time.deltaTime);
			} 
			if (Input.GetKey(KeyCode.R)) {
                //transform.Translate(0f, Time.deltaTime * conSpeed, 0f, Space.World);
				movementVec += new Vector3(0f, Time.deltaTime * conSpeed, 0f);
			} 
			if (Input.GetKey(KeyCode.F) && transform.position.y > collider.bounds.size.y / 2) {
                //transform.Translate(0f, -Time.deltaTime * conSpeed, 0f, Space.World);
				movementVec += new Vector3(0f, -Time.deltaTime * conSpeed, 0f);
			} 
			if (Input.GetKey(KeyCode.Q)) {
				transform.Rotate(0f, -rotSpeed * Time.deltaTime, 0f);
			} else if (Input.GetKey(KeyCode.E)) {
				transform.Rotate(0f, rotSpeed * Time.deltaTime, 0f);
			}
			transform.Translate(movementVec, Space.World);
		}
	}

	protected void OnGUI() {
		if (placing) {
			GUI.Box(new Rect(Screen.width - 200, Screen.height - 150, 150, 100), "Press Enter to place");
			if (Input.GetKeyDown(KeyCode.Return)) {
				endPlacement();
				wasPlaced = true;
			}
		}
	}

	protected void OnTriggerStay(Collider target) {
		if (target.tag == "ConGrid" && !placing && !wasPlaced) {
			if (target.bounds.Contains(transform.position) && transform.parent == player.transform) {
				SnapTo(target.gameObject, target.bounds.size.z);
			}
		}
	}

	protected void OnTriggerExit(Collider target) {
		if (target.tag == "ConGrid" && !wasPlaced) {
			endPlacement();
			wasPlaced = false;
		}
	}

	// This function is jank and hacked together right now. Fixing it after shit works.
	public void SnapTo(GameObject grid, float offset) {
		if (transform.parent == player.transform)
			transform.parent = null;
		//player.transform.position = grid.transform.position - offset * grid.transform.forward;
		//player.transform.eulerAngles = new Vector3(0f, 0f, 0f);
		player.rigidbody.velocity = new Vector3(0f, 0f, 0f);
		//transform.position = grid.transform.position;
		transform.eulerAngles = new Vector3(0f, 0f, 0f);
		//rigidbody.velocity = new Vector3(0f, 0f, 0f);
		//rigidbody.freezeRotation = true;
        //rigidbody.useGravity = false;
		Destroy(rigidbody);
		placing = true;
		player.GetComponent<FirstPersonCharacter>().disableMovement();
		mainCam.enabled = false;
        activeCam = closestCam(grid);
        //activeCam = grid.transform.Find("ConstructionCameraZ").GetComponent<Camera>();
		activeCam.enabled = true;
		adjX = Mathf.Cos(activeCam.transform.eulerAngles.y * Mathf.PI / 180f) * conSpeed;
		adjZ = Mathf.Sin(activeCam.transform.eulerAngles.y * Mathf.PI / 180f) * conSpeed;
	}

	public void endPlacement() {
		mainCam.enabled = true;
		activeCam.enabled = false;
		placing = false;
		player.GetComponent<FirstPersonCharacter>().enableMovement();
        gameObject.AddComponent<Rigidbody>();
		//rigidbody.useGravity = true;
	}

    private Camera closestCam(GameObject obj) {
        float minDist = 10000;
        Camera nearCam = new Camera();
        foreach (Transform camera in obj.transform){
            float dist = Vector3.Distance(camera.position, player.transform.position);
            Camera cam = camera.GetComponent<Camera>();
            if (cam && dist < minDist) {
                Debug.Log("ew");
                nearCam = camera.GetComponent<Camera>();
                minDist = dist;
            }
        }
		Debug.Log(nearCam.name);
        return nearCam;
    }
}
