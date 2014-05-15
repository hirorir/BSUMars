﻿using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
	public GameObject main;
	public GameObject credits;
	public GameObject back;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnClick() {
		if (name == "Credit") {
			NGUITools.SetActive(main, false);
			NGUITools.SetActive(credits, true);
		} else if (name == "Play") {
			Application.LoadLevel("mars_concrete_house");
		} else if (name == "Back") {
			NGUITools.SetActive(credits, false);
			NGUITools.SetActive(main, true);
		}
	}
}
