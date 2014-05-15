using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Recipe {
	public List<ConTuple> reqObjs; // A list of the required blocks' materials and sizes.
	private List<GameObject> usedObjs; // Objects that will be used if the recipe is executed.
	public bool sizeStrict; // Whether or not objects whose size is larger than the minimum size are accepted.
	private GameObject result; // The new GameObject spawned by the recipe's execution.
	private string recName; // The name of the recipe

	public Recipe(List<ConTuple> reqObjs, bool sizeStrict, GameObject result, string recName) {
		this.reqObjs = reqObjs;
		this.sizeStrict = sizeStrict;
		this.result = result;
		this.usedObjs = new List<GameObject>();
		this.recName = recName;
	}

	// Add a construction piece that will be used for a recipe.
	public void addPiece(GameObject piece) {
		this.usedObjs.Add(piece);
	}

	// Clear the list of pieces that will be used.
	public void clearPieces() {
		this.usedObjs.Clear();
	}

	public List<GameObject> getPieces() {
		return this.usedObjs;
	}

	public GameObject outputResult() {
		return this.result;
	}

	public string getName() {
		return this.recName;
	}
}
