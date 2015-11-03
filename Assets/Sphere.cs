using UnityEngine;
using System.Collections;

public class Sphere : MonoBehaviour {
	public int y = 0;

	// Use this for initialization
	void Start () {
		// int x = (int)(transform.position.x * 100.0f);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (1)) {
			if(Random.Range (0,50) == 40)
				Debug.Log("clicked");
			Vector3 p = transform.position;
			p.y = y;
			transform.position = p;
		}
	}
//	void OnTriggerStay(Collider c) {
//		if(Random.Range (0,50) == 40)
//			Debug.Log("collide!");
//		y++;
//	}

//	void OnTriggerEnter(Collider collision) {
//		if(Random.Range (0,50) == 40)
//		Debug.Log("trigger!");
//		y++;
//	}
}
