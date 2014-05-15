using UnityEngine;
using System.Collections;

// A struct that holds the requirements for blocks in a recipe. Created because C# Tuple is not available.
public class ConTuple {
	public int size; // The required size of the blocks.
	public string material; // The required material of the blocks.
	public int numReq; // The number of blocks with the above requirements needed.

	public ConTuple(int size, string material, int numReq) {
		this.size = size;
		this.material = material;
		this.numReq = numReq;
	}
}
