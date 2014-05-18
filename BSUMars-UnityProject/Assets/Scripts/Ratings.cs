using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Ratings : MonoBehaviour {

	public GameObject grid; // The construction grid containing the player's building.
	private int pieceCount = 0;
	private float highHeight = 0f;
	private Dictionary<string, int> materialCounts;
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
	[SerializeField] private GameObject blockLabel;
	[SerializeField] private GameObject heightLabel;
	[SerializeField] private GameObject compLabel;
	[SerializeField] private GameObject reqPanel;
	[SerializeField] private GameObject ratingsPanel;
	[SerializeField] private GameObject menu;
	private GameObject statLabel;
	// Use this for initialization
	void Start () {
		if (KongregateAPI.instance == null)
			Debug.Log("This should never happen.");

		statLabel = Resources.Load("Prefabs/Material Comp") as GameObject;
		minMaterials = new Dictionary<string, int>();
		maxMaterials = new Dictionary<string, int>();
		minMaterials.Add("Concrete", 180);
		minMaterials.Add("Metal", 20);
		maxMaterials.Add("Concrete", 2840);
		maxMaterials.Add("Metal", 160);
		setReqPanel();
	}
	
	// Update is called once per frame
	void Update () {

	}

	// Check if the player's building meets the minimum requirements for completion.
	private bool checkReqs() {
		// Check the block counts
		if (minPieces > -1 && maxPieces > -1)
			if (pieceCount < minPieces || pieceCount > maxPieces)
				return false;
			else if (minPieces > -1)
				if (pieceCount < minPieces)
					return false;
				else if (maxPieces > -1)
					if (pieceCount > maxPieces)
						return false;

		// Check the building height
		if (minHeight > -1 && maxHeight > -1)
			if (highHeight < minHeight || highHeight > maxHeight)
				return false;
			else if (minHeight > -1)
				if (highHeight < minHeight)
					return false;
				else if (maxHeight > -1)
					if (highHeight > maxHeight)
						return false;


		// Check the material counts
		foreach (KeyValuePair<string, int> req in minMaterials) {
			if (minMaterials[req.Key] > -1)
				if (materialCounts[req.Key] < minMaterials[req.Key])
					return false;
		}

		foreach (KeyValuePair<string, int> req in minMaterials) {
			if (maxMaterials[req.Key] > -1)
				if (materialCounts[req.Key] > maxMaterials[req.Key])
					return false;
		}

		
		return true;
	}

	// Calculate the building's stats.
	public void calcStats() {
		List<GameObject> conPieces = grid.GetComponent<ConGrid>().getBuilding();
		materialCounts = new Dictionary<string,int>();
		pieceCount = 0; // The number of construction pieces on the grid.
		highHeight = 0f; // The highest piece height of the building.
		float height; // The height of an object.
		int amt; // The amount of blocks in an object (determined by size classification).

		foreach (Transform label in compLabel.transform) {
			Destroy(label.gameObject);
		}

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

		blockLabel.GetComponent<UILabel>().text = pieceCount + " blocks";
		heightLabel.GetComponent<UILabel>().text = highHeight + " units";
		int numLabels = 0;
		foreach (KeyValuePair<string, int> pair in materialCounts) {
			numLabels++;
			GameObject matLabel = (GameObject) Instantiate(statLabel, compLabel.transform.position, compLabel.transform.rotation);
			matLabel.transform.parent = compLabel.transform;
			matLabel.transform.localScale = compLabel.transform.localScale;
			matLabel.transform.localPosition = new Vector3(matLabel.transform.localPosition.x, matLabel.transform.localPosition.y - 100f * numLabels, matLabel.transform.localPosition.z); // The 100f is static to save time.
			matLabel.GetComponent<UILabel>().text = pair.Key + ": " + pair.Value;
		}

		NGUITools.SetActive(menu, true);
		Time.timeScale = 0;
		Screen.lockCursor = false;
		if (checkReqs()) {
			NGUITools.SetActive(menu.transform.Find("Panel - Stats").Find("End Construction").gameObject, true);
			calcRatings();
		} else
			NGUITools.SetActive(menu.transform.Find("Panel - Stats").Find("End Construction").gameObject, false);
	}

	private void countMaterial(Dictionary<string, int> materialCounts, string mat, int amt) {
		if (materialCounts.ContainsKey(mat))
			materialCounts[mat] += amt;
		else
			materialCounts.Add(mat, amt);
	}

	// Calculate the player's ratings and scores.
	private void calcRatings() {
		int points = 0;
		int total = 0;

		// Block count score
		if (minPieces > -1 && maxPieces > -1) {
			points = (int) (3001f * (1f - Mathf.Abs(((float) (minPieces + maxPieces) - (float) pieceCount * 2f) / ((float) (maxPieces - minPieces)))));
		} else if (minPieces > -1) {
			points = (int)(3001f * Mathf.Min(Mathf.Abs((2f * (float)(pieceCount - minPieces)) / ((float) minPieces)), 1f));
		} else if (maxPieces > -1) {
			points = (int)(3001f * Mathf.Min(Mathf.Abs((2f * (float)(maxPieces - pieceCount)) / ((float) maxPieces)), 1f));
		}

		// Handle menu and score
		if (points >= 1000) {
			NGUITools.SetActive(ratingsPanel.transform.GetChild(1).GetChild(1).gameObject, true);
			if (points >= 2000)
				NGUITools.SetActive(ratingsPanel.transform.GetChild(1).GetChild(2).gameObject, true);
		}
		total += points;
		ratingsPanel.transform.GetChild(1).GetChild(0).GetComponent<UILabel>().text = "Score: " + points.ToString();

		// Height score
		if (minHeight > -1 && maxHeight > -1) {
			points = (int)(3000f * (1f - Mathf.Abs(((minHeight + maxHeight) - highHeight * 2f) / (maxHeight - minHeight))));
		} else if (minHeight > -1) {
			points = (int)(3000f * Mathf.Min(Mathf.Abs((2f * (highHeight - minHeight))) / minHeight, 1f));
		} else if (maxHeight > -1) {
			points = (int)(3000f * Mathf.Min(Mathf.Abs((2f * (maxHeight - pieceCount)) / maxHeight), 1f));
		}

		// Handle menu and score
		if (points >= 1000) {
			NGUITools.SetActive(ratingsPanel.transform.GetChild(3).GetChild(1).gameObject, true);
			if (points >= 2000)
				NGUITools.SetActive(ratingsPanel.transform.GetChild(3).GetChild(2).gameObject, true);
		}
		total += points;
		ratingsPanel.transform.GetChild(3).GetChild(0).GetComponent<UILabel>().text = "Score: " + points.ToString();

		// Material composition score
		// Concrete
		if (minMaterials["Concrete"] > -1 && maxMaterials["Concrete"] > -1) {
			points = (int)(3000f * (1f - Mathf.Abs(((float)(minMaterials["Concrete"] + maxPieces) - (float)materialCounts["Concrete"] * 2f) / ((float)(maxPieces - minMaterials["Concrete"])))));
		} else if (minMaterials["Concrete"] > -1) {
			points = (int)(3000f * Mathf.Min(Mathf.Abs((2f * (float)(materialCounts["Concrete"] - minMaterials["Concrete"])) / ((float)minMaterials["Concrete"])), 1f));
		} else if (maxPieces > -1) {
			points = (int)(3000f * Mathf.Min(Mathf.Abs((2f * (float)(maxPieces - materialCounts["Concrete"])) / ((float)maxPieces)), 1f));
		}


		// Metal
		if (minPieces > -1 && maxPieces > -1) {
			points += (int)(3000f * (1f - Mathf.Abs(((float)(minPieces + maxPieces) - (float)pieceCount * 2f) / ((float)(maxPieces - minPieces)))));
		} else if (minPieces > -1) {
			points += (int)(3000f * Mathf.Min(Mathf.Abs((2f * (float)(pieceCount - minPieces)) / ((float)minPieces)), 1f));
		} else if (maxPieces > -1) {
			points += (int)(3000f * Mathf.Min(Mathf.Abs((2f * (float)(maxPieces - pieceCount)) / ((float)maxPieces)), 1f));
		}

		points = points / 2;

		// Handle menu and score
		if (points >= 1000) {
			NGUITools.SetActive(ratingsPanel.transform.GetChild(4).GetChild(1).gameObject, true);
			if (points >= 2000)
				NGUITools.SetActive(ratingsPanel.transform.GetChild(4).GetChild(2).gameObject, true);
		}
		ratingsPanel.transform.GetChild(4).GetChild(0).GetComponent<UILabel>().text = "Score: " + points.ToString();
		total += points;

		// Total rating and score
		if (total >= 3000) {
			NGUITools.SetActive(ratingsPanel.transform.GetChild(7).GetChild(1).gameObject, true);
			if (total >= 6000)
				NGUITools.SetActive(ratingsPanel.transform.GetChild(7).GetChild(2).gameObject, true);
		}
		
		ratingsPanel.transform.GetChild(7).GetChild(0).GetComponent<UILabel>().text = "Score: " + total.ToString();
	}

	// Set the values in the requirements panel. Mostly hardcoding this for now to save time.
	private void setReqPanel() {
		Transform workingReq = reqPanel.transform.GetChild(2);

		// Display the block count requirements
		if (minPieces > -1)
			workingReq.GetChild(1).GetComponent<UILabel>().text = "Min: " + minPieces + " blocks";
		else
			workingReq.GetChild(1).GetComponent<UILabel>().text = "Min: N/A";
		if (maxPieces > -1)
			workingReq.GetChild(0).GetComponent<UILabel>().text = "Max: " + maxPieces + " blocks";
		else
			workingReq.GetChild(0).GetComponent<UILabel>().text = "Max: N/A";

		workingReq = reqPanel.transform.GetChild(3);

		// Display the height requirements
		if (minHeight > -1)
			workingReq.GetChild(1).GetComponent<UILabel>().text = "Min: " + minHeight + " units";
		else
			workingReq.GetChild(1).GetComponent<UILabel>().text = "Min: N/A";
		if (maxHeight > -1)
			workingReq.GetChild(0).GetComponent<UILabel>().text = "Max: " + maxHeight + " units";
		else
			workingReq.GetChild(0).GetComponent<UILabel>().text = "Max: N/A";

		workingReq = reqPanel.transform.GetChild(4);

		// Display the material requirements
		// Concrete requirements
		if (minMaterials["Concrete"] > -1 && maxMaterials["Concrete"] > -1)
			workingReq.GetChild(0).GetComponent<UILabel>().text = "Concrete: min: " + minMaterials["Concrete"] + " blocks, max: " + maxMaterials["Concrete"] + " blocks";
		else if (minMaterials["Concrete"] > -1)
			workingReq.GetChild(0).GetComponent<UILabel>().text = "Concrete: min: " + minMaterials["Concrete"] + " blocks, max: N/A";
		else if (maxMaterials["Concrete"] > -1)
			workingReq.GetChild(0).GetComponent<UILabel>().text = "Concrete: min: N/A, max: " + maxMaterials["Concrete"] + " blocks";
		else
			workingReq.GetChild(0).GetComponent<UILabel>().text = "Concrete: N/A";
		// Metal requirements
		if (minMaterials["Metal"] > -1 && maxMaterials["Metal"] > -1)
			workingReq.GetChild(1).GetComponent<UILabel>().text = "Metal: min: " + minMaterials["Metal"] + " blocks, max: " + maxMaterials["Metal"] + " blocks";
		else if (minMaterials["Metal"] > -1)
			workingReq.GetChild(1).GetComponent<UILabel>().text = "Metal: min: " + minMaterials["Metal"] + " blocks, max: N/A";
		else if (maxMaterials["Metal"] > -1)
			workingReq.GetChild(1).GetComponent<UILabel>().text = "Metal: min: N/A, max: " + maxMaterials["Metal"] + " blocks";
		else
			workingReq.GetChild(1).GetComponent<UILabel>().text = "Metal: N/A";
	}
}
