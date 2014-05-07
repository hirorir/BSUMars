using UnityEngine;
using System.Collections;

public class explosion : MonoBehaviour {

	// Use this for initialization
	[SerializeField] private float radius = 20.0F;
	[SerializeField] private float power = 500.0F;

	void Start() {
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown (KeyCode.P)){
			Vector3 explosionPos = transform.position;
			Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
			ConstructionPiece conPiece;
			foreach (Collider hit in colliders) {
				if (hit.rigidbody){
					conPiece = hit.GetComponent<ConstructionPiece>();
					if (conPiece != null)
						conPiece.explosion(power, explosionPos, radius, 3.0f);
					else
						hit.rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0F);
				}
			}
			Destroy (gameObject);
		}
	}
}
