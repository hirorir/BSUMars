using UnityEngine;
using System.Collections;

public class explosion_particles_destroy : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
	
	}


	// Update is called once per frame
	void FixedUpdate () {
		if (!audio.isPlaying) {
			Destroy (gameObject);
		}
	}
}
