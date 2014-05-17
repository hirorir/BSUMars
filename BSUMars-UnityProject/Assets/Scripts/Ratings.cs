using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Ratings : MonoBehaviour {

	public GameObject grid; // The construction grid containing the player's building.
	/* RATING REQUIREMENTS */
	// Set the value for a requirement to -1 for the ratings system to ignore it.
	// Set a max material requirement to 0 to not allow that material in the building.
	// Piece values are calculated according to the size classification of construction pieces.
	// Each mini-cube (size class 2) equates to one piece unit, so a standard block is 4, a small wall 16, and so on.
	[SerializeField] private int minPieces = 200; // The minimum number of cubes needed for completion. 
	[SerializeField] private int maxPieces = 1600; // The maximum number of pieces allowed for completion.
	[SerializeField] private Dictionary<string, int> minMaterials; // The minimum number of each material needed for completion.
	[SerializeField] private Dictionary<string, int> maxMaterials; // The maximum number of each each material allowed for completion.
	[SerializeField] private float minHeight = 5f;
	[SerializeField] private float maxHeight = 20f;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

	}

	// Check if the player's building meets the minimum requirements for completion.
	public void checkReqs() {
		//List<GameObject> conPieces = GameObject.FindGameObjectsWithTag("ConPiece").Where(obj => grid.collider.bounds.Contains(obj.transform.position)).ToList<GameObject>();
		List<GameObject> conPieces = grid.GetComponent<ConGrid>().getBuilding();
		Dictionary<string, int> materialCounts = new Dictionary<string,int>();
		int pieceCount = 0;
		float highHeight = 0f; // The highest piece height of the building.
		float height;
		int amt;

		// Calculate the building stats
		foreach(GameObject obj in conPieces) {
			ConstructionPiece piece = obj.GetComponent<ConstructionPiece>();
			switch (piece.blockSize) {
				case 2:
					amt = 1;
					break;
				case 3:
					amt = 4;
					break;
				case 4:
					amt = 16;
					break;
				case 5:
					amt = 32;
					break;
				case 6:
					amt = 64;
					break;
				case 7:
					amt = 128;
					break;
				default:
					amt = 0;
					break;
			}

			pieceCount += amt;
			countMaterial(materialCounts, piece.bldgMat, amt);

			height = obj.transform.position.y + obj.renderer.bounds.size.y / 2f;
			if (height > highHeight)
				highHeight = height;
		}

		Debug.Log("Piece Count: " + pieceCount);
		Debug.Log("Height: " + highHeight);
		Debug.Log("Material composition:");
		foreach (KeyValuePair<string, int> pair in materialCounts) {
			Debug.Log(pair.Key + ": " + pair.Value);
		}

		/*// Check the block counts
		if (minPieces > -1 && maxPieces > -1)
			if (conPieces.Count < minPieces || conPieces.Count > maxPieces)
				return;
		else if (minPieces > -1)
			if (conPieces.Count < minPieces)
				return;
		else if (maxPieces > -1)
			if (conPieces.Count > maxPieces)
				return;

		// Check the building height
		if (minHeight > -1 && maxHeight > -1)
			if (highHeight < minHeight || highHeight > maxHeight)
				return;
			else if (minHeight > -1)
				if (highHeight < minHeight)
					return;
			else if (maxHeight > -1)
				if (highHeight > maxHeight)
					return;

		// Check the material counts
		foreach (KeyValuePair<string, int> req in minMaterials) {
			if (materialCounts.ContainsKey(req.Key)) {
				if (minMaterials[req.Key] > -1 && maxMaterials[req.Key] > -1)
					if (materialCounts[req.Key] < minMaterials[req.Key] || materialCounts[req.Key] > maxMaterials[req.Key])
					return;
				else if (minMaterials[req.Key] > -1)
					if (materialCounts[req.Key] < maxMaterials[req.Key])
						return;
				else if (maxMaterials[req.Key] > -1)
					if (materialCounts[req.Key] > maxMaterials[req.Key])
						return;
			}
		}*/
	}

	private void countMaterial(Dictionary<string, int> materialCounts, string mat, int amt) {
		if (materialCounts.ContainsKey(mat))
			materialCounts[mat] += amt;
		else
			materialCounts.Add(mat, amt);
	}

	private void calcRatings() {

	}
}
