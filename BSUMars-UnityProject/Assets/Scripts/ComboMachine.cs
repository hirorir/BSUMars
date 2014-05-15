using UnityEngine;
using System.Collections;

public class ComboMachine : MonoBehaviour {
	[SerializeField] private GameObject grid;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

	}

	public void displayRecipes() {
		grid.GetComponent<ComboGrid>().checkAllRecipes();
	}
}
