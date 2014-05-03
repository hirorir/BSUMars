using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConstructionPiece : MonoBehaviour {
	[SerializeField] protected float conSpeed = 8f; // The controlled movement speed of the object.
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
	
	// Update is called once per frame
	void Update () {
		if (placing) {
			if (Input.GetKey(KeyCode.W)) {
				transform.Translate(0f, 0f, Time.deltaTime * conSpeed, Space.World);
			} else if (Input.GetKey(KeyCode.A)) {
				transform.Translate(-Time.deltaTime * conSpeed, 0f, 0f, Space.World);
			} else if (Input.GetKey(KeyCode.S)) {
                transform.Translate(0f, 0f, -Time.deltaTime * conSpeed, Space.World);
			} else if (Input.GetKey(KeyCode.D)) {
                transform.Translate(Time.deltaTime * conSpeed, 0f, 0f, Space.World);
			} else if (Input.GetKey(KeyCode.R)) {
                transform.Translate(0f, Time.deltaTime * conSpeed, 0f, Space.World);
			} else if (Input.GetKey(KeyCode.F) && transform.position.y - collider.bounds.size.y > 0) {
                transform.Translate(0f, -Time.deltaTime * conSpeed, 0f, Space.World);
			} else if (Input.GetKey(KeyCode.Q)) {
				transform.Rotate(0f, -1f, 0f);
			} else if (Input.GetKey(KeyCode.E)) {
				transform.Rotate(0f, 1f, 0f);
			}
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
		if (target.tag == "ConGrid") {
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
		transform.position = grid.transform.position;
		transform.eulerAngles = new Vector3(0f, 0f, 0f);
		//rigidbody.velocity = new Vector3(0f, 0f, 0f);
		//rigidbody.freezeRotation = true;
        //rigidbody.useGravity = false;
		Destroy(rigidbody);
		placing = true;
		player.GetComponent<FirstPersonCharacter>().disableMovement();
		mainCam.enabled = false;
        //activeCam = closestCam();
        Debug.Log(grid.transform.Find("ConstructionCameraZ").name);
        activeCam = grid.transform.Find("ConstructionCameraZ").GetComponent<Camera>();
		activeCam.enabled = true;
		
	}

	public void endPlacement() {
		mainCam.enabled = true;
		activeCam.enabled = false;
		placing = false;
		player.GetComponent<FirstPersonCharacter>().enableMovement();
        gameObject.AddComponent<Rigidbody>();
		//rigidbody.useGravity = true;
	}

    private Camera closestCam() {
        float minDist = 10000;
        Camera nearCam = new Camera();
        foreach (GameObject camera in transform){
            float dist = Vector3.Distance(camera.transform.position, player.transform.position);
            Camera cam = camera.GetComponent<Camera>();
            if (cam &&  dist < minDist) {
                Debug.Log("ew");
                nearCam = camera.GetComponent<Camera>();
                minDist = dist;
            }
        }
        return nearCam;
    }
}
