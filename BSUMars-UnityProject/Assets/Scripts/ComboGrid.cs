using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class ComboGrid : MonoBehaviour {
	[SerializeField] protected GameObject player;
	private List<GameObject> comboItems; // Construction pieces in the grid.
	private List<Recipe> recipes; // All possible recipes.
	private List<Recipe> activeRecipes; // Recipes that have their requirements met.
	[SerializeField] GameObject comboPanel; // The combo panel in the UI
	[SerializeField] GameObject menu;
	private GameObject recipeButton;
	//private int blockCount;

	// Use this for initialization
	void Start () {
		recipeButton = Resources.Load("Prefabs/Recipe") as GameObject;
		comboItems = new List<GameObject>();
		addRecipes();
	}
	
	// Update is called once per frame
	void Update () {
		/*if (Input.GetKeyDown(KeyCode.R)) {
			if (!player.GetComponent<FirstPersonCharacter>().isPlacing())
				combine();
		}*/
	}

	// Using OnGUI until I have time to create something nicer using NGUI
	/*void OnGUI() {
		if (activeRecipes.Count > 0) {
			Time.timeScale = 0;
			GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
			for (int i = 0; i < activeRecipes.Count; i++) {
				if (GUI.Button(new Rect(Screen.width * 0.2f, Screen.height * 0.1f * (i + 1), 200f, 50f), activeRecipes[i].getName())) {
					combine(activeRecipes[i]);
					activeRecipes.Clear();
					Time.timeScale = 1;
					Screen.lockCursor = true;
				}
			}

			if (GUI.Button(new Rect(Screen.width * 0.7f, Screen.height * 0.7f, 100f, 50f), "Cancel")) {
				activeRecipes.Clear();
				Time.timeScale = 1;
				Screen.lockCursor = true;
			}
		}
	}*/

	public void addItem(GameObject item) {
		comboItems.Add(item);
		//blockCount++;
	}

	public void removeItem(GameObject item) {
		comboItems.Remove(item);
		//blockCount--;
	}

	public void combine(Recipe recipe) {
		List<GameObject> checklist = recipe.getPieces();
		foreach (GameObject item in checklist) {
			Destroy(item);
			comboItems.Remove(item);
			//blockCount--;
		}

		recipe.clearPieces();
		GameObject piece = recipe.outputResult();
		piece = (GameObject) GameObject.Instantiate(piece, new Vector3(transform.position.x, transform.position.y + piece.renderer.bounds.size.y / 2, transform.position.z), Quaternion.identity);
		piece.rigidbody.AddForce(Vector3.zero);
		GameObject.FindGameObjectWithTag("Player").rigidbody.AddForce(Vector3.zero);
	}

	public void clearRecs() {
		activeRecipes.Clear();
	}

	public void checkAllRecipes() {
		activeRecipes.Clear();
		GameObject currentRecButton;

		foreach (Transform child in comboPanel.transform)
			if (child.name != "Cancel")
				Destroy(child.gameObject);
		foreach (Recipe recipe in recipes) {
			if (checkRecipe(recipe)) {
				activeRecipes.Add(recipe);
				currentRecButton = GameObject.Instantiate(recipeButton, recipeButton.transform.position, recipeButton.transform.rotation) as GameObject;
				currentRecButton.transform.parent = comboPanel.transform;
				currentRecButton.transform.localScale = recipeButton.transform.localScale;
				currentRecButton.transform.localPosition = new Vector3(-400, 400 - activeRecipes.Count * 100f, 0f);
				currentRecButton.transform.localEulerAngles = Vector3.zero;
				currentRecButton.transform.GetChild(0).GetComponent<UILabel>().text = recipe.getName();
				currentRecButton.GetComponent<LoadButton>().setCFunc(GetComponent<ComboGrid>(), recipe, () => {
					Time.timeScale = 1;
					Screen.lockCursor = true;
					NGUITools.SetActive(comboPanel, false);
					NGUITools.SetActive(menu, false);
					NGUITools.SetActive(menu.transform.GetChild(3).gameObject, true);
				});
			}
		}
		if (activeRecipes.Count > 0) {
			Time.timeScale = 0;
			Screen.lockCursor = false;
			NGUITools.SetActive(comboPanel, true);
			NGUITools.SetActive(menu, true);
			NGUITools.SetActive(menu.transform.GetChild(3).gameObject, false);
		}
	}

	private bool checkRecipe(Recipe recipe) {
		List<GameObject> blocks; // Blocks that satisfy a requirement.
		recipe.clearPieces();
		foreach (ConTuple reqs in recipe.reqObjs) {
			if (recipe.sizeStrict)
				blocks = comboItems.Where(obj => obj.GetComponent<ConstructionPiece>().bldgMat == reqs.material && obj.GetComponent<ConstructionPiece>().blockSize == reqs.size).ToList<GameObject>();
			else
				blocks = comboItems.Where(obj => obj.GetComponent<ConstructionPiece>().bldgMat == reqs.material && obj.GetComponent<ConstructionPiece>().blockSize >= reqs.size).ToList<GameObject>();

			if (blocks.Count >= reqs.numReq) {
				for (int i = 0; i < reqs.numReq; i++)
					recipe.addPiece(blocks[i]);
				blocks.Clear();
			} else {
				return false;
			}
		}
		return true;
	}

	// Hard-coded recipes go here
	private void addRecipes() {
		// Build a normal metal block from smaller blocks
		recipes = new List<Recipe>();
		activeRecipes = new List<Recipe>();
		List<ConTuple> matList = new List<ConTuple>();
		matList.Add(new ConTuple(2, "Metal", 4));
		recipes.Add(new Recipe(new List<ConTuple>(matList), true, Resources.Load("Prefabs/MetalBlock") as GameObject, "Metal Block"));

		// Build a normal concrete block from smaller blocks
		matList.Clear();
		matList.Add(new ConTuple(2, "Concrete", 4));
		recipes.Add(new Recipe(new List<ConTuple>(matList), true, Resources.Load("Prefabs/ConcreteBlock") as GameObject, "Concrete Block"));

		// Build a small concrete wall from blocks
		matList.Clear();
		matList.Add(new ConTuple(3, "Concrete", 4));
		recipes.Add(new Recipe(new List<ConTuple>(matList), true, Resources.Load("Prefabs/ConcSmallWall") as GameObject, "Small Concrete Wall"));

		// Build a small metal wall from blocks
		matList.Clear();
		matList.Add(new ConTuple(3, "Metal", 4));
		recipes.Add(new Recipe(new List<ConTuple>(matList), true, Resources.Load("Prefabs/MetalSmallWall") as GameObject, "Small Metal Wall"));

		// Build a concrete column from blocks
		matList.Clear();
		matList.Add(new ConTuple(3, "Concrete", 4));
		recipes.Add(new Recipe(new List<ConTuple>(matList), true, Resources.Load("Prefabs/ConcColumn") as GameObject, "Concrete Column"));

		// Build a metal column from blocks
		matList.Clear();
		matList.Add(new ConTuple(3, "Metal", 4));
		recipes.Add(new Recipe(new List<ConTuple>(matList), true, Resources.Load("Prefabs/MetalColumn") as GameObject, "Metal Column"));

		// Convert between a concrete column and small wall
		matList.Clear();
		matList.Add(new ConTuple(4, "Concrete", 1));
		recipes.Add(new Recipe(new List<ConTuple>(matList), true, Resources.Load("Prefabs/ConcSmallWall") as GameObject, "Small Concrete Wall"));

		// Convert between a metal column and small wall
		matList.Clear();
		matList.Add(new ConTuple(4, "Metal", 1));
		recipes.Add(new Recipe(new List<ConTuple>(matList), true, Resources.Load("Prefabs/MetalSmallWall") as GameObject, "Small Metal Wall"));

		// Convert between a concrete column and small wall
		matList.Clear();
		matList.Add(new ConTuple(4, "Concrete", 1));
		recipes.Add(new Recipe(new List<ConTuple>(matList), true, Resources.Load("Prefabs/ConcColumn") as GameObject, "Concrete Column"));

		// Convert between a metal column and small wall
		matList.Clear();
		matList.Add(new ConTuple(4, "Metal", 1));
		recipes.Add(new Recipe(new List<ConTuple>(matList), true, Resources.Load("Prefabs/MetalColumn") as GameObject, "Metal Column"));

		// Build a long concrete wall from small walls
		matList.Clear();
		matList.Add(new ConTuple(4, "Concrete", 2));
		recipes.Add(new Recipe(new List<ConTuple>(matList), true, Resources.Load("Prefabs/ConcLongWall") as GameObject, "Long Concrete Wall"));

		// Build a long metal wall from small walls
		matList.Clear();
		matList.Add(new ConTuple(4, "Metal", 2));
		recipes.Add(new Recipe(new List<ConTuple>(matList), true, Resources.Load("Prefabs/MetalLongWall") as GameObject, "Long Metal Wall"));

		// Build a long concrete wall from blocks
		matList.Clear();
		matList.Add(new ConTuple(3, "Concrete", 8));
		recipes.Add(new Recipe(new List<ConTuple>(matList), true, Resources.Load("Prefabs/ConcLongWall") as GameObject, "Long Concrete Wall"));

		// Build a long metal wall from blocks
		matList.Clear();
		matList.Add(new ConTuple(3, "Metal", 8));
		recipes.Add(new Recipe(new List<ConTuple>(matList), true, Resources.Load("Prefabs/MetalLongWall") as GameObject, "Long Metal Wall"));

		// Build a long concrete wall from small walls
		matList.Clear();
		matList.Add(new ConTuple(4, "Concrete", 4));
		recipes.Add(new Recipe(new List<ConTuple>(matList), true, Resources.Load("Prefabs/ConcLargeWall") as GameObject, "Large Concrete Wall"));

		// Build a long metal wall from small walls
		matList.Clear();
		matList.Add(new ConTuple(4, "Metal", 4));
		recipes.Add(new Recipe(new List<ConTuple>(matList), true, Resources.Load("Prefabs/MetalLargeWall") as GameObject, "Large Metal Wall"));
	}
}
