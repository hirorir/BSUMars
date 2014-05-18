using UnityEngine;
using System.Collections;

public class StatsButton : MonoBehaviour {
	[SerializeField] private GameObject target;
	[SerializeField] bool resume = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnClick() {
		if (resume) {
			Time.timeScale = 1;
			Screen.lockCursor = true;
			Transform menu = transform.parent.parent;
			if (transform.parent.name == "Pause")
				NGUITools.SetActive(transform.parent.gameObject, false);
			NGUITools.SetActive(menu.Find("Panel - Stats").gameObject, true);
			NGUITools.SetActive(menu.gameObject, false);
		} else {
			NGUITools.SetActive(transform.parent.gameObject, false);
			NGUITools.SetActive(target, true);
			Screen.lockCursor = false;
		}
	}
}
