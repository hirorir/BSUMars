using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ConstructionPiece : MonoBehaviour {
	[SerializeField] protected float conSpeed = 16f; // The controlled placement movement speed of the object.
	[SerializeField] protected float rotSpeed = 90f; // The controlled placement rotation speed of the object.
	public string bldgMat = "Concrete"; // The material of the object.
	public int blockSize = 3; // The size classificarion of the object.
	public int explosionCube = 3; // Controls how many pieces the object breaks into from explosions. For cubes, explodes into n^2 pieces.
	//public float origMass;
	private float adjX;
	private float adjZ;
	public GameObject curGrid;
	private bool placing = false; // Whether or not the piece is being placed by the player.
	private GameObject player;
	private FirstPersonCharacter playerChar;
	private Vector2 springMidpointXZ;
	//private Camera mainCam;
    //private Camera activeCam;
	//private List<Camera> conCams;

	[SerializeField] private float radius = 10.0F;
	[SerializeField] private float power = 500.0F;

	void OnParticleCollision(GameObject other){
		Vector3 direction = other.transform.position - transform.position;
		if(direction.magnitude < radius)
			explosion(power, other.transform.position, radius, 3.0f);
	}

	// Use this for initialization
	void Start () {
		StartCoroutine(getobjs());
		//origMass = rigidbody.mass;
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
		/*if (player != null) {
			if (transform.parent == player.transform && Input.GetKeyDown(KeyCode.Return)) {
				transform.parent = null;
			}
		}*/
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
			if (Input.GetKey(KeyCode.Space)) {
				movementVec += new Vector3(0f, Time.deltaTime * conSpeed, 0f);
			} 
			if (Input.GetKey(KeyCode.Z) && transform.position.y > collider.bounds.size.y / 2) {
				movementVec += new Vector3(0f, -Time.deltaTime * conSpeed, 0f);
			} 
			if (Input.GetKey(KeyCode.Q)) {
				/*if (GetComponent<FixedJoint>()) {
					Vector3 rotPoint = new Vector3(springMidpointXZ.x, transform.position.y, springMidpointXZ.y);
					transform.RotateAround(rotPoint, Vector3.up, -rotSpeed * Time.deltaTime);
					foreach (FixedJoint connection in GetComponents<FixedJoint>())
						connection.transform.RotateAround(rotPoint, Vector3.up, -rotSpeed * Time.deltaTime);
				} else*/
				transform.Rotate(0f, -rotSpeed * Time.deltaTime, 0f);
			}
			if (Input.GetKey(KeyCode.E)) {
				/*if (GetComponent<FixedJoint>()) {
					Vector3 rotPoint = new Vector3(springMidpointXZ.x, transform.position.y, springMidpointXZ.y);
					transform.RotateAround(rotPoint, Vector3.up, rotSpeed * Time.deltaTime);
					foreach (FixedJoint connection in GetComponents<FixedJoint>())
						connection.transform.RotateAround(rotPoint, Vector3.up, rotSpeed * Time.deltaTime);
				} else*/
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
		} else if (target.tag == "Combinations") {
			target.GetComponent<ComboGrid>().addItem(gameObject);
		}
	}

	protected void OnTriggerExit(Collider target) {
		if (target.tag == "ConGrid") {
			curGrid = null;
		} else if (target.tag == "Combinations") {
			target.GetComponent<ComboGrid>().removeItem(gameObject);
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
		/*foreach (FixedJoint joint in GetComponents<FixedJoint>()) {
			joint.connectedBody.rigidbody.isKinematic = true;
		}*/
	}

	protected void OnCollisionEnter(Collision target) {
		if (placing && target.gameObject.tag == "ConPiece") {
			rigidbody.velocity = Vector3.zero;
			/*if (playerChar.conMode) {
				//FixedJoint joint = gameObject.AddComponent<FixedJoint>();
				//joint.connectedBody = target.rigidbody;
				//joint.connectedBody.rigidbody.useGravity = false;
				//joint.connectedBody.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
				//joint.connectedBody.rigidbody.isKinematic = false;
				//RecalculateMidPoint();
				endPlacement();
			}*/
		}
	}

	public void setAdjs(float angle) {
		adjX = Mathf.Cos(angle * Mathf.PI / 180f) * conSpeed;
		adjZ = Mathf.Sin(angle * Mathf.PI / 180f) * conSpeed;
	}

	// Adds an explosion force to the piece, and breaks it apart if it is breakable.
	// If the ExplosionCube for any object is 1, it does not break apart when exploded.
	// If it is 0, then it is destroyed in an explosion.
	// Non-cube items maintain their ExplosionCube values until they are broken into cubes.
	public void explosion(float power, Vector3 explosionPos, float radius, float upwardsModifier) {
		switch (blockSize) {
			case 4:
				if (explosionCube != 1) {
					if (explosionCube > 0) {
						GameObject cubeType;
						if (bldgMat == "Concrete")
							cubeType = Resources.Load("Prefabs/ConcreteBlock") as GameObject;
						else if (bldgMat == "Metal")
							cubeType = Resources.Load("Prefabs/MetalBlock") as GameObject;
						else
							cubeType = Resources.Load("Prefabs/MetalBlock") as GameObject;

						Vector3 wallCorner = new Vector3(transform.position.x - collider.bounds.size.x * 0.375f, transform.position.y, transform.position.z);
						// Break the small wall into cubes
						for (int i = 0; i < 4; i++) {
							GameObject newPiece = GameObject.Instantiate(cubeType) as GameObject;
							newPiece.GetComponent<ConstructionPiece>().explosionCube--;
							newPiece.GetComponent<ConstructionPiece>().blockSize = 3;
							if (transform.localScale.x == 4)
								newPiece.transform.position = wallCorner + new Vector3(i * collider.bounds.size.x / 4f, 0f, 0f);
							else
								newPiece.transform.position = wallCorner + new Vector3(0f, 0f, i * collider.bounds.size.z / 4f);
							//newPiece.transform.localScale = transform.localScale / (float)explosionCube;
							newPiece.rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0f);
							//newPiece.GetComponent<ConstructionPiece>().explosionCube--;
						}
					}
				} else
					rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0f);
				Destroy(gameObject);
				break;
			case 5:
				if (explosionCube != 1) {
					if (explosionCube > 0) {
						GameObject cubeType;
						if (bldgMat == "Concrete")
							cubeType = Resources.Load("Prefabs/ConcreteBlock") as GameObject;
						else if (bldgMat == "Metal")
							cubeType = Resources.Load("Prefabs/MetalBlock") as GameObject;
						else
							cubeType = Resources.Load("Prefabs/MetalBlock") as GameObject;

						Vector3 wallCorner = new Vector3(transform.position.x - collider.bounds.size.x * 0.4375f, transform.position.y, transform.position.z);
						// Break the small wall into cubes
						for (int i = 0; i < 8; i++) {
							GameObject newPiece = GameObject.Instantiate(cubeType) as GameObject;
							newPiece.GetComponent<ConstructionPiece>().explosionCube--;
							newPiece.GetComponent<ConstructionPiece>().blockSize = 3;
							newPiece.transform.position = wallCorner + new Vector3(i * collider.bounds.size.x / 8f, 0f, 0f);
							//newPiece.transform.localScale = transform.localScale / (float)explosionCube;
							newPiece.rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0f);
							//newPiece.GetComponent<ConstructionPiece>().explosionCube--;
						}
					}
				} else
					rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0f);
				Destroy(gameObject);
				break;
			case 6:
				if (explosionCube != 1) {
					if (explosionCube > 0) {
						GameObject wallType;
						if (bldgMat == "Concrete")
							wallType = Resources.Load("Prefabs/ConcSmallWall") as GameObject;
						else if (bldgMat == "Metal")
							wallType = Resources.Load("Prefabs/MetalSmallWall") as GameObject;
						else
							wallType = Resources.Load("Prefabs/MetalSmallWall") as GameObject;

						Vector3 wallCorner = new Vector3(transform.position.x, transform.position.y - collider.bounds.size.y * 0.375f, transform.position.z);
						// Break the small wall into cubes
						for (int i = 0; i < 4; i++) {
							GameObject newPiece = GameObject.Instantiate(wallType) as GameObject;
							//newPiece.GetComponent<ConstructionPiece>().explosionCube = explosionCube - 1;
							newPiece.GetComponent<ConstructionPiece>().blockSize = 4;
							newPiece.transform.position = wallCorner + new Vector3(0f, i * collider.bounds.size.y / 4f, 0f);
							//newPiece.transform.localScale = transform.localScale / (float)explosionCube;
							newPiece.rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0f);
							//newPiece.GetComponent<ConstructionPiece>().explosionCube--;
						}
					}
				} else
					rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0f);
				Destroy(gameObject);
				break;
			case 7:
				if (explosionCube != 1) {
					if (explosionCube > 0) {
						GameObject wallType;
						if (bldgMat == "Concrete")
							wallType = Resources.Load("Prefabs/ConcSmallWall") as GameObject;
						else if (bldgMat == "Metal")
							wallType = Resources.Load("Prefabs/MetalSmallWall") as GameObject;
						else
							wallType = Resources.Load("Prefabs/MetalSmallWall") as GameObject;

						Vector3 wallCorner = new Vector3(transform.position.x - collider.bounds.size.x * 0.25f, transform.position.y - collider.bounds.size.y * 0.375f, transform.position.z);
						// Break the small wall into cubes
						for (int i = 0; i < 2; i++) {
							for (int j = 0; j < 4; j++) {
								GameObject newPiece = GameObject.Instantiate(wallType) as GameObject;
								//newPiece.GetComponent<ConstructionPiece>().explosionCube = explosionCube - 1;
								newPiece.GetComponent<ConstructionPiece>().blockSize = 4;
								newPiece.transform.position = wallCorner + new Vector3(i * collider.bounds.size.x / 2f, j * collider.bounds.size.y / 4f, 0f);
								//newPiece.transform.localScale = transform.localScale / (float)explosionCube;
								newPiece.rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0f);
								//newPiece.GetComponent<ConstructionPiece>().explosionCube--;
							}
						}
					}
				} else
					rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0f);
				Destroy(gameObject);
				break;
			default:
				if (explosionCube != 1) {
					if (explosionCube > 0) {
						/*if(transform.localScale.x <= 0.25f){
							Destroy (gameObject);
							return;
						}*/
						//List<GameObject> newPieces = new List<GameObject>();
						int expCubeTrue = explosionCube * explosionCube * explosionCube;
						Vector3 cubeCorner = new Vector3(transform.position.x + collider.bounds.size.x * (1 - expCubeTrue) / (2f * expCubeTrue), transform.position.y + collider.bounds.size.y * (1 - expCubeTrue) / (2f * expCubeTrue),
														   transform.position.z + collider.bounds.size.z * (1 - expCubeTrue) / (2f * expCubeTrue));
						for (int i = 0; i < explosionCube; i++) {
							for (int j = 0; j < explosionCube; j++) {
								for (int k = 0; k < explosionCube; k++) {
									GameObject newPiece = GameObject.Instantiate(gameObject) as GameObject;
									newPiece.GetComponent<ConstructionPiece>().explosionCube = explosionCube - 1;
									newPiece.GetComponent<ConstructionPiece>().blockSize = 2;
									newPiece.transform.position = new Vector3(cubeCorner.x + i % explosionCube, cubeCorner.y + j % explosionCube, cubeCorner.z + k % explosionCube);
									newPiece.transform.localScale = transform.localScale / ((float)explosionCube + Random.value);
									newPiece.rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0f);
									newPiece.GetComponent<ConstructionPiece>().explosionCube--;
								}
							}
						}
					}
					Destroy(gameObject);
				} else {
					rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0f);
					explosionCube--;
				}
				break;
	};
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
