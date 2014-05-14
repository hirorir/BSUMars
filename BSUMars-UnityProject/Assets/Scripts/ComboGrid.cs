using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ComboGrid : MonoBehaviour {
	[SerializeField] protected GameObject player;
	private List<GameObject> comboItems;
	private int blockCount;

	// Use this for initialization
	void Start () {
		comboItems = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.R)) {
			if (!player.GetComponent<FirstPersonCharacter>().isPlacing())
				checkRecipe();
		}
	}

	public void addItem(GameObject item) {
		comboItems.Add(item);
		blockCount++;
	}

	public void removeItem(GameObject item) {
		comboItems.Remove(item);
		blockCount--;
	}

	private void combine() {
		foreach (GameObject item in comboItems) {
			Destroy(item);
		}

		comboItems.Clear();
		blockCount = 0;
		GameObject piece = Resources.Load("Prefabs/BldgBase") as GameObject;
		GameObject.Instantiate(piece, new Vector3(transform.position.x, transform.position.y + piece.collider.bounds.size.y / 2, transform.position.z), Quaternion.identity);
		Debug.Log(comboItems.Count);
	}

	private void checkRecipe() {
		// temporary
		if (blockCount > 4)
			combine();
	}

	// RECIPES

}
