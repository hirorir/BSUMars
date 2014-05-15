using UnityEngine;
using System.Collections;

public class FirstPersonCharacter : MonoBehaviour
{
	[SerializeField] private float runSpeed = 8f;                                       // The speed at which we want the character to move
	[SerializeField] private float strafeSpeed = 4f;                                    // The speed at which we want the character to be able to strafe
	[SerializeField] private float jumpPower = 5f;                                      // The power behind the characters jump. increase for higher jumps
	#if !MOBILE_INPUT
	[SerializeField] private bool walkByDefault = true;									// controls how the walk/run modifier key behaves.
	[SerializeField] private float walkSpeed = 3f;                                      // The speed at which we want the character to move
	#endif
	[SerializeField] private AdvancedSettings advanced = new AdvancedSettings();        // The container for the advanced settings ( done this way so that the advanced setting are exposed under a foldout
	[SerializeField] private bool lockCursor = true;
	[SerializeField]
	private bool movEnabled = true;
	[SerializeField] private float rigDist = 3f;

	[System.Serializable]
	public class AdvancedSettings                                                       // The advanced settings
	{
		public float gravityMultiplier = 1f;                                            // Changes the way gravity effect the player ( realistic gravity can look bad for jumping in game )
		public PhysicMaterial zeroFrictionMaterial;                                     // Material used for zero friction simulation
		public PhysicMaterial highFrictionMaterial;                                     // Material used for high friction ( can stop character sliding down slopes )
		public float groundStickyEffect = 5f;											// power of 'stick to ground' effect - prevents bumping down slopes.
	}
	
	private CapsuleCollider capsule;                                                    // The capsule collider for the first person character
	private const float jumpRayLength = 0.5f;                                           // The length of the ray used for testing against the ground when jumping
	public bool grounded { get; private set; }
	public bool conMode = false;
	private GameObject grid = null;
	private Vector2 input;
	private IComparer rayHitComparer;
	private GameObject hitObject = null;
	private GameObject cam; // The player's cam
	private GameObject reticule;
	private GameObject placingObject;
	private float originalMass;
	public Camera activeCam; // The active cam

	public bool holdingObject(){
		return(hitObject != null);
	}

	void Start(){
		cam = GameObject.FindGameObjectWithTag ("MainCamera");
		activeCam = cam.GetComponent<Camera>();
		//reticule = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		//Destroy (reticule.GetComponent<SphereCollider>());
		reticule = new GameObject ();
		reticule.name = "rig";
		reticule.transform.parent = cam.transform;
		Rigidbody dummyRig = reticule.AddComponent<Rigidbody> ();
		dummyRig.useGravity = false;
		dummyRig.isKinematic = true;
		dummyRig.constraints = RigidbodyConstraints.FreezeAll;
	}

	public void dropObject(){
		//Destroy(reticule.GetComponent<SpringJoint> ());
		//print ("dropped");
		hitObject.layer = 11;											//recognizes character collision
		Destroy(reticule.GetComponent<FixedJoint> ());		
		hitObject.transform.parent = null;
		hitObject.rigidbody.mass = originalMass;
		//hitObject.rigidbody.constraints = RigidbodyConstraints.None;
		hitObject.rigidbody.useGravity = true;
		/*ConstructionPiece conPiece = hitObject.GetComponent<ConstructionPiece>();
		if(conPiece != null){
			if (conPiece.curGrid != null)
				conPiece.StartPlacement(conPiece.curGrid);
		}*/
		hitObject = null;
	}

	public void pickupObject(GameObject hit) {
		//print("picked Up");
		hitObject = hit;
		hitObject.layer = 13;											//ignores character collision
		hitObject.transform.parent = cam.transform;						//if it's a construction piece, do this
		hitObject.rigidbody.useGravity = false;
		originalMass = hitObject.rigidbody.mass;
		hitObject.rigidbody.mass = 0.001f;
		//hitObject.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		hitObject.transform.position = reticule.transform.position;
		FixedJoint joint = reticule.AddComponent<FixedJoint>();			//have the cube gravitate towards the reticule
		//SpringJoint joint = reticule.AddComponent<SpringJoint>();

		joint.connectedBody = hitObject.rigidbody;

		/*joint.spring = 500f;
		joint.damper = 0.01f;
		joint.maxDistance = 0.01f;*/
	}

	public void ConCam(GameObject grid) {
		Screen.lockCursor = false;
		cam.GetComponent<Camera>().enabled = false;
		if (hitObject != null)
			activeCam = closestCam(hitObject, grid);
		else
			activeCam = closestCam(gameObject, grid);
		activeCam.enabled = true;
	}
	
	void Awake ()
	{
		// Set up a reference to the capsule collider.
		capsule = collider as CapsuleCollider;
		grounded = true;
		Screen.lockCursor = lockCursor;
		rayHitComparer = new RayHitComparer();
	}

	void OnDisable()
	{
		Screen.lockCursor = false;
	}

	void Update()
	{
		if (Input.GetMouseButtonDown (0) && hitObject != null) {
			GameObject projectile = hitObject;
			dropObject ();
			projectile.rigidbody.AddExplosionForce(500f, cam.transform.position, 10f);
		}

		RaycastHit hit;

		if(Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out hit, rigDist, ~((1 << 12) + (1 << 9) + (1 << 13)))){
			reticule.transform.position = hit.point;
			if(hitObject != null){
				// || (reticule.transform.position - hitObject.transform.position).magnitude < 0.5f
				if(hit.distance < 0.5f)		//if it's too close or too far from cursor, drop it
					dropObject ();
			}
		}
		else reticule.transform.position = cam.transform.TransformDirection (Vector3.forward) * rigDist + cam.transform.position;	//reticule is always rigDist in front of the camera

		if(hitObject != null)
			hitObject.rigidbody.angularVelocity = new Vector3 (0, 0, 0);

		if (Input.GetKeyDown(KeyCode.E)) {
			if(hitObject != null){		//if it isn't null, it must be what the player is carrying. toggle off
				dropObject ();
			}
			else if (Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out hit, rigDist, (1 << 14) + (1 << 11))) {
				//Debug.Log("yus");
				if (hit.collider.tag == "ComboMachine")
					hit.collider.GetComponent<ComboMachine>().displayRecipes();
				else
					pickupObject(hit.collider.gameObject);			//otherwise get what the character is trying to get
			}
		}

		if (grid != null || conMode) {
			if (Input.GetKeyDown(KeyCode.Tab)) {
				if (conMode) {
					conMode = false;
					if (placingObject != null) {
						placingObject.GetComponent<ConstructionPiece>().endPlacement();
						placingObject = null;
					} else {
						reactivateCam();
					}
				} else {
					conMode = true;
					if (hitObject != null) {
                        placingObject = hitObject;
						dropObject();
                        ConstructionPiece conPiece = placingObject.GetComponent<ConstructionPiece>();
                        conPiece.StartPlacement(conPiece.curGrid);
					} else {
						rigidbody.velocity = Vector3.zero;
						disableMovement();
						ConCam(grid);
					}
				}
			}
		}

		if (conMode) {
			if (Input.GetKeyDown(KeyCode.LeftArrow)) {
				activeCam.enabled = false;
				activeCam = getRotCam(activeCam, "left");
				activeCam.enabled = true;
				placingObject.GetComponent<ConstructionPiece>().setAdjs(activeCam.transform.eulerAngles.y);
			} else if (Input.GetKeyDown(KeyCode.RightArrow)) {
				activeCam.enabled = false;
				activeCam = getRotCam(activeCam, "right");
				activeCam.enabled = true;
				placingObject.GetComponent<ConstructionPiece>().setAdjs(activeCam.transform.eulerAngles.y);
			}
		} else {
			if (Input.GetMouseButtonUp(0)) {
				Screen.lockCursor = lockCursor;
			}
		}
	}


	public void FixedUpdate() {
		if (movEnabled) {
			float speed = runSpeed;

			// Read input
#if CROSS_PLATFORM_INPUT
			float h = CrossPlatformInput.GetAxis("Horizontal");
			float v = CrossPlatformInput.GetAxis("Vertical");
			bool jump = CrossPlatformInput.GetButton("Jump");
#else
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		bool jump = Input.GetButton("Jump");
#endif

#if !MOBILE_INPUT

			// On standalone builds, walk/run speed is modified by a key press.
			// We select appropriate speed based on whether we're walking by default, and whether the walk/run toggle button is pressed:
			bool walkOrRun = Input.GetKey(KeyCode.LeftShift);
			speed = walkByDefault ? (walkOrRun ? runSpeed : walkSpeed) : (walkOrRun ? walkSpeed : runSpeed);

			// On mobile, it's controlled in analogue fashion by the v input value, and therefore needs no special handling.


#endif

			input = new Vector2(h, v);

			// normalize input if it exceeds 1 in combined length:
			if (input.sqrMagnitude > 1) input.Normalize();

			// Get a vector which is desired move as a world-relative direction, including speeds
			Vector3 desiredMove = transform.forward * input.y * speed + transform.right * input.x * strafeSpeed;

			// preserving current y velocity (for falling, gravity)
			float yv = rigidbody.velocity.y;

			// add jump power
			if (grounded && jump) {
				yv += jumpPower;
				grounded = false;
			}

			// Set the rigidbody's velocity according to the ground angle and desired move
			rigidbody.velocity = desiredMove + Vector3.up * yv;

			// Use low/high friction depending on whether we're moving or not
			if (desiredMove.magnitude > 0 || !grounded) {
				collider.material = advanced.zeroFrictionMaterial;
			} else {
				collider.material = advanced.highFrictionMaterial;
			}


			// Ground Check:

			// Create a ray that points down from the centre of the character.
			Ray ray = new Ray(transform.position, -transform.up);

			// Raycast slightly further than the capsule (as determined by jumpRayLength)
			RaycastHit[] hits = Physics.RaycastAll(ray, capsule.height * jumpRayLength);
			System.Array.Sort(hits, rayHitComparer);


			if (grounded || rigidbody.velocity.y < jumpPower * .5f) {
				// Default value if nothing is detected:
				grounded = false;
				// Check every collider hit by the ray
				for (int i = 0; i < hits.Length; i++) {
					// Check it's not a trigger
					if (!hits[i].collider.isTrigger) {
						// The character is grounded, and we store the ground angle (calculated from the normal)
						grounded = true;

						// stick to surface - helps character stick to ground - specially when running down slopes
						//if (rigidbody.velocity.y <= 0) {
						rigidbody.position = Vector3.MoveTowards(rigidbody.position, hits[i].point + Vector3.up * capsule.height * .5f, Time.deltaTime * advanced.groundStickyEffect);
						//}
						rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
						break;
					}
				}
			}

			Debug.DrawRay(ray.origin, ray.direction * capsule.height * jumpRayLength, grounded ? Color.green : Color.red);

			// add extra gravity
			rigidbody.AddForce(Physics.gravity * (advanced.gravityMultiplier - 1));
		}
	}

	
	//used for comparing distances
	class RayHitComparer: IComparer
	{
		public int Compare(object x, object y)
		{
			return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
		}	
	}

	protected void OnTriggerEnter(Collider target) {
		if (target.tag == "ConGrid") {
			grid = target.gameObject;
		}
	}

	protected void OnTriggerExit(Collider target) {
		if (target.tag == "ConGrid") {
			grid = null;
		}
	}

	public bool isPlacing() {
		return hitObject != null;
	}

	/*protected void OnTriggerStay(Collider target) {
		if (target.tag == "ConGrid" && !conMode) {
			conMode = true;
			if (hitObject != null) {
				dropObject();
			} else {
				rigidbody.velocity = Vector3.zero;
				disableMovement();
				ConCam(target.gameObject);
			}
		}
	}*/

	public void enableMovement() {
		movEnabled = true;
	}
	public void disableMovement() {
		movEnabled = false;
	}

	private Camera closestCam(GameObject activeObj, GameObject grid) {
		float minDist = 10000;
		Camera nearCam = new Camera();
		foreach (Transform camera in grid.transform) {
			float dist = Vector3.Distance(camera.position, activeObj.transform.position);
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
	}

	public void reactivateCam() {
		activeCam.enabled = false;
		activeCam = cam.GetComponent<Camera>();
		activeCam.enabled = true;
		Screen.lockCursor = true;
		enableMovement();
	}

	public void pickup(GameObject obj) {
		placingObject = obj;
	}

	public void nullifyPickup() {
		placingObject = null;
	}
}
