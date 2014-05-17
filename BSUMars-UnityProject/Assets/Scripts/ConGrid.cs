using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConGrid : MonoBehaviour {
	private List<GameObject> building;

	// Use this for initialization
	void Start () {
		building = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void addPiece(GameObject piece) {
		building.Add(piece);
	}

	public void removePiece(GameObject piece) {
		building.Remove(piece);
	}

	public List<GameObject> getBuilding() {
		return building;
	}
}
