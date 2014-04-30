using UnityEngine;
using System.Collections;

public class CharacterControl : MonoBehaviour {

	[SerializeField] private float speed = 20f;
	// Use this for initialization
	void Start () {
		Screen.lockCursor = true;
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.rigidbody.velocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis ("Vertical")) * speed;
	}
}
