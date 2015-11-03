using UnityEngine;
using System.Collections;

public class CamTest : MonoBehaviour {
	public int width = 320;
	public int height = 240;
	public int FPS = 30;
	
	public WebCamTexture wct;
	public Color32[] c;
	
	// Use this for initialization
	void Start()
	{
		var devices = WebCamTexture.devices;
		if ( devices.Length == 0 ) {
			Debug.LogError( "Webカメラが検出できませんでした。" );
			return;
		}
		// WebCamテクスチャを作成する.
		wct = new WebCamTexture( width, height, FPS );
		GetComponent<Renderer>().material.mainTexture = wct;
		wct.Play();
	}
	
	// Update is called once per frame
	void Update()
	{
	}
}
