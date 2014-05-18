using UnityEngine;
using System.Collections;

public class LoadButton : MonoBehaviour {
	[SerializeField] private string level = "restart";
	private ComboGrid grid;
	private Recipe recipe; // Used only if the button is for a recipe
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public delegate void clickFunc();
	public static clickFunc onClick = () => {
	};

	public void createOnClick(Recipe rec, clickFunc func) {
		
		onClick = func;
	}

	public void setCFunc(ComboGrid cGrid, Recipe rec, clickFunc func) {
		grid = cGrid;
		recipe = rec;
		onClick = func;
	}

	void OnClick() {
		if (recipe != null) {
			grid.combine(recipe);
			grid.clearRecs();
			onClick();
		} else {
			Time.timeScale = 1;
			switch (level) {
				case "restart":
					Application.LoadLevel(Application.loadedLevel);
					break;
				case "menu":
					Screen.lockCursor = false;
					Application.LoadLevel("menu");
					break;
				default:
					Application.LoadLevel(level);
					break;
			}
		}
	}
}
