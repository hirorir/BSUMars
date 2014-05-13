using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ConstructionPiece : MonoBehaviour {
	[SerializeField] protected float conSpeed = 16f; // The controlled placement movement speed of the object.
	[SerializeField] protected float rotSpeed = 90f; // The controlled placement rotation speed of the object.
	[SerializeField] protected int explosionCube = 1; // Controls how many pieces the object breaks into from explosions. For cubes, explodes into n^2 pieces.
	public float origMass;
	private float adjX;
	private float adjZ;
	public GameObject curGrid;
	private bool placing = false; // Whether or not the piece is being placed by the player.
	private bool wasPlaced = false;
	private GameObject player;
	private FirstPersonCharacter playerChar;
	private Vector2 springMidpointXZ;
	//private Camera mainCam;
    //private Camera activeCam;
	//private List<Camera> conCams;

	// Use this for initialization
	void Start () {
		StartCoroutine(getobjs());
		origMass = rigidbody.mass;
		springMidpointXZ = new Vector2(transform.position.x, transform.position.z);
	}

	private IEnumerator getobjs() {
		yield return new WaitForEndOfFrame();
		player = GameObject.Find("First Person Camera");
		playerChar = player.transform.parent.parent.GetComponent<FirstPersonCharacter>();
		//mainCam = player.GetComponent<Camera>();
        //activeCam = mainCam;
	}

	void Update() {
		if (placing) {
			rigidbody.velocity = Vector3.zero;
		}
		/*if (player != null) {
			if (playerChar != null) {
				if (Input.GetMouseButtonDown(0)) {
					//Debug.Log(":D");
				}
			}
		}*/
	}
	
	void FixedUpdate () {
		if (player != null) {
			if (transform.parent == player.transform && Input.GetKeyDown(KeyCode.Return)) {
				transform.parent = null;
			}
		}
		if (placing) {
			Vector3 movementVec = new Vector3(0f, 0f, 0f);
			if (Input.GetKey(KeyCode.W)) {
				movementVec += new Vector3(adjZ * Time.deltaTime, 0f, adjX * Time.deltaTime);
			} 
			if (Input.GetKey(KeyCode.A)) {
				movementVec -= new Vector3(adjX * Time.deltaTime, 0f, -adjZ * Time.deltaTime);
			} 
			if (Input.GetKey(KeyCode.S)) {
				movementVec -= new Vector3(adjZ * Time.deltaTime, 0f, adjX * Time.deltaTime);
			} 
			if (Input.GetKey(KeyCode.D)) {
				movementVec += new Vector3(adjX * Time.deltaTime, 0f, -adjZ * Time.deltaTime);
			} 
			if (Input.GetKey(KeyCode.R)) {
				movementVec += new Vector3(0f, Time.deltaTime * conSpeed, 0f);
			} 
			if (Input.GetKey(KeyCode.F) && transform.position.y > collider.bounds.size.y / 2) {
				movementVec += new Vector3(0f, -Time.deltaTime * conSpeed, 0f);
			} 
			if (Input.GetKey(KeyCode.Q)) {
				if (GetComponent<FixedJoint>()) {
					Vector3 rotPoint = new Vector3(springMidpointXZ.x, transform.position.y, springMidpointXZ.y);
					transform.RotateAround(rotPoint, Vector3.up, -rotSpeed * Time.deltaTime);
					foreach (FixedJoint connection in GetComponents<FixedJoint>())
						connection.transform.RotateAround(rotPoint, Vector3.up, -rotSpeed * Time.deltaTime);
				} else
					transform.Rotate(0f, -rotSpeed * Time.deltaTime, 0f);
			} else if (Input.GetKey(KeyCode.E)) {
				if (GetComponent<FixedJoint>()) {
					Vector3 rotPoint = new Vector3(springMidpointXZ.x, transform.position.y, springMidpointXZ.y);
					transform.RotateAround(rotPoint, Vector3.up, rotSpeed * Time.deltaTime);
					foreach (FixedJoint connection in GetComponents<FixedJoint>())
						connection.transform.RotateAround(rotPoint, Vector3.up, rotSpeed * Time.deltaTime);
				} else
					transform.Rotate(0f, rotSpeed * Time.deltaTime, 0f);
			}
			transform.Translate(movementVec, Space.World);

			
		}
	}

	/*protected void OnGUI() {
		if (placing) {
			GUI.Box(new Rect(Screen.width - 200, Screen.height - 150, 150, 100), "Press Enter to place");
			if (Input.GetKeyDown(KeyCode.Return)) {
				endPlacement();
				wasPlaced = true;
			}
		}
	}*/

	protected void OnTriggerEnter(Collider target) {
		if (target.tag == "ConGrid" && !placing) {
			curGrid = target.gameObject;
		}
	}

	protected void OnTriggerExit(Collider target) {
		if (target.tag == "ConGrid") {
			curGrid = null;
			if (!wasPlaced) {
				endPlacement();
				wasPlaced = false;
			}
		}
	}

	private void RecalculateMidPoint() {
		FixedJoint[] connections = GetComponents<FixedJoint>();
		Vector2 average = new Vector2(transform.position.x, transform.position.z);
		foreach (FixedJoint connection in connections) {
			average.x += connection.transform.position.x;
			average.y += connection.transform.position.z;
		}
		springMidpointXZ = average / (connections.Length + 1);
	}

	// This function is jank and hacked together right now. Fixing it after shit works.
	public void StartPlacement(GameObject grid) {
		if (transform.parent == player.transform)
			transform.parent = null;
		transform.eulerAngles = Vector3.zero;
		playerChar.rigidbody.velocity = Vector3.zero;
		//rigidbody.velocity = Vector3.zero;
		foreach (GameObject piece in GameObject.FindGameObjectsWithTag("ConPiece")) {
			piece.rigidbody.isKinematic = true;
		}
		rigidbody.useGravity = false;
		rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		rigidbody.isKinematic = false;
		placing = true;
		playerChar.pickup(gameObject);
		playerChar.conMode = true;
		playerChar.disableMovement();
		playerChar.ConCam(grid);
		setAdjs(playerChar.activeCam.transform.eulerAngles.y);
	}

	public void endPlacement() {
		playerChar.reactivateCam();
		placing = false;
		playerChar.enableMovement();
		playerChar.conMode = false;
		playerChar.nullifyPickup();
		Screen.lockCursor = true;
		foreach (GameObject piece in GameObject.FindGameObjectsWithTag("ConPiece")) {
			piece.rigidbody.isKinematic = false;
            piece.rigidbody.AddForce(Vector3.zero);
		}
		rigidbody.useGravity = true;
		rigidbody.constraints = RigidbodyConstraints.None;
	}

	protected void OnCollisionEnter(Collision target) {
		if (placing && target.gameObject.tag == "ConPiece") {
			rigidbody.velocity = Vector3.zero;
			if (Input.GetKey(KeyCode.Space)) {
				FixedJoint joint = gameObject.AddComponent<FixedJoint>();
				joint.connectedBody = target.rigidbody;
				joint.connectedBody.rigidbody.useGravity = false;
				joint.connectedBody.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
				joint.connectedBody.rigidbody.isKinematic = false;
				RecalculateMidPoint();
			}
		}
	}

	public void setAdjs(float angle) {
		adjX = Mathf.Cos(angle * Mathf.PI / 180f) * conSpeed;
		adjZ = Mathf.Sin(angle * Mathf.PI / 180f) * conSpeed;
	}

	// Adds an explosion force to the piece, and breaks it apart if it is breakable.
	// Only works for cubes for now.
	public void explosion(float power, Vector3 explosionPos, float radius, float upwardsModifier) {
		if (explosionCube > 1) {
			//List<GameObject> newPieces = new List<GameObject>();
			int expCubeTrue = explosionCube * explosionCube * explosionCube;
			Vector3 cubeCorner = new Vector3(transform.position.x + collider.bounds.size.x * (1 - expCubeTrue) / (2f * expCubeTrue), transform.position.y + collider.bounds.size.y * (1 - expCubeTrue) / (2f * expCubeTrue),
											   transform.position.z + collider.bounds.size.z * (1 - expCubeTrue) / (2f * expCubeTrue));
			for (int i = 0; i < explosionCube; i++) {
				for (int j = 0; j < explosionCube; j++) {
					for (int k = 0; k < explosionCube; k++) {
						GameObject newPiece = GameObject.Instantiate(gameObject) as GameObject;
						newPiece.transform.position = new Vector3(cubeCorner.x + i % explosionCube, cubeCorner.y + j % explosionCube, cubeCorner.z + k % explosionCube);
						newPiece.transform.localScale = transform.localScale / (float)explosionCube;
						newPiece.rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0f);
					}
				}
			}
			Destroy(gameObject);
		} else {
			rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0F);
		}
	}

    /*private Camera closestCam(GameObject obj) {
        float minDist = 10000;
        Camera nearCam = new Camera();
        foreach (Transform camera in obj.transform){
            float dist = Vector3.Distance(camera.position, player.transform.position);
            Camera cam = camera.GetComponent<Camera>();
            if (cam && dist < minDist) {
                nearCam = camera.GetComponent<Camera>();
                minDist = dist;
            }
        }
        return nearCam;
    }

	private Camera getRotCam(Camera orig, string dir) {
		if (dir == "left") {
			switch (orig.name) {
				case "ConstructionCameraFront":
					return orig.transform.parent.Find("ConstructionCameraLeft").GetComponent<Camera>();
				case "ConstructionCameraLeft":
					return orig.transform.parent.Find("ConstructionCameraBack").GetComponent<Camera>();
				case "ConstructionCameraBack":
					return orig.transform.parent.Find("ConstructionCameraRight").GetComponent<Camera>();
				case "ConstructionCameraRight":
					return orig.transform.parent.Find("ConstructionCameraFront").GetComponent<Camera>();
				default:
					return orig;
			}
		} else if (dir == "right") {
			switch (orig.name) {
				case "ConstructionCameraFront":
					return orig.transform.parent.Find("ConstructionCameraRight").GetComponent<Camera>();
				case "ConstructionCameraLeft":
					return orig.transform.parent.Find("ConstructionCameraFront").GetComponent<Camera>();
				case "ConstructionCameraBack":
					return orig.transform.parent.Find("ConstructionCameraLeft").GetComponent<Camera>();
				case "ConstructionCameraRight":
					return orig.transform.parent.Find("ConstructionCameraBack").GetComponent<Camera>();
				default:
					return orig;
			}
		} else {
			return orig;
		}
	}*/
}
