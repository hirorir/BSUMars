using UnityEngine;
using System.Collections;

public class LoadButton : MonoBehaviour {
	[SerializeField] private static string level = "restart";

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public delegate void clickFunc();
	public static clickFunc onClick = () => {
		switch (level) {
			case "restart":
				Application.LoadLevel(Application.loadedLevel);
				break;
			case "menu":
				Application.LoadLevel("menu");
				break;
			default:
				break;
		}
	};

	public void setCFunc(clickFunc func) {
		onClick = func;
	}

	void OnClick() {
		onClick();
	}
}
