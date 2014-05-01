using UnityEngine;
using System.Collections;

public class ConstructionPiece : MonoBehaviour {
	private bool placing = false; // Whether or not the piece is being placed by the player.

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (placing) {
			if (Input.GetKeyDown(KeyCode.W)) {
				transform.Translate(Time.deltaTime * 5f, 0f, 0f);
			} else if (Input.GetKeyDown(KeyCode.A)) {
				transform.Translate(0f, 0f, - Time.deltaTime * 5f);
			} else if (Input.GetKeyDown(KeyCode.S)) {
				transform.Translate(- Time.deltaTime * 5f, 0f, 0f);
			} else if (Input.GetKeyDown(KeyCode.D)) {
				transform.Translate(0f, 0f, Time.deltaTime * 5f);
			} else if (Input.GetKeyDown(KeyCode.R)) {
				transform.Translate(0f, Time.deltaTime * 5f, 0f);
			} else if (Input.GetKeyDown(KeyCode.F)) {
				transform.Translate(0f, -Time.deltaTime * 5f, 0f);
			}
		}
	}

	protected void OnTriggerEnter(Collider target) {
		if (target.tag == "ConGrid" && !placing) {
			if (target.bounds.Contains(transform.position)) {
				SnapTo(target.gameObject);
			}
		}
	}

	public void SnapTo(GameObject grid) {
		transform.position = grid.transform.position;
		placing = true;
	}
}
