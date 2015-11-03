using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class CamTexture : MonoBehaviour
{
	public WebCamBehaviour wcb;
	int width, height;
	Texture2D texture;

	public GameObject testCube;

	Color32[] UpdateTextureTest (Color32[] c)
	{
		Color32[] rc = new Color32[width * height];
		int p = 0;
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				rc [p].r = (byte)(c [p].r + (byte)Random.Range (0, 10));
				rc [p].g = (byte)(c [p].g + (byte)Random.Range (0, 10));
				rc [p].b = (byte)(c [p].b + (byte)Random.Range (0, 10));
				rc [p].a = 255;
				p++;
			}
		}
		return rc;
	}

	Color32[] UpdateTextureTest_2 (Color32[] c)
	{
		Color32[] rc = new Color32[width * height];
		for (int i = 0; i < width * height; i++) {
			rc [i].r = c [i].g;
			rc [i].g = c [i].r;
			rc [i].b = 0;
			rc [i].a = 255;
			if(i % width > 10 && i / width > 220){
				rc [i].r = rc [i].g = rc [i].b = 0;
				rc [i].a = 255;
			}
		}
		return rc;
	}

	// Use this for initialization
	void Start ()
	{
		width = wcb.width;
		height = wcb.height;
		GetComponent<Renderer>().material.mainTexture = wcb.wct;
	}
	void Update(){
		if(Input.GetMouseButtonDown (0)){
			Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			int x = (int)mPos.x, y = (int)mPos.y;
			Color32[] cm = wcb.wct.GetPixels32();
			Vector3 cmean = Vector3.zero;
			int mrange = 2;
			for(int j = -mrange; j < mrange; j++){
				for(int i = -mrange; i < mrange; i++){
					cmean.x += cm[(y + j) * width + (x + i)].r;
					cmean.y += cm[(y + j) * width + (x + i)].g;
					cmean.z += cm[(y + j) * width + (x + i)].b;
				}
			}
			Color32 cc = new Color32();
			// cc = wcb.wct.GetPixels32()[y * width + x];
			cc.r = (byte)(cmean.x / (mrange * mrange * 4));
			cc.g = (byte)(cmean.y / (mrange * mrange * 4));
			cc.b = (byte)(cmean.z / (mrange * mrange * 4));
			cc.a = (byte)255;
			testCube.GetComponent<Renderer>().material.color = cc;
		}

	}
	
	// Update is called once per frame
	void OnGUI ()
	{
//		Color32[] rc = UpdateTextureTest_2(wcb.wct.GetPixels32());
//		texture = new Texture2D (width, height);
//		texture.SetPixels32 (rc);
//		texture.Apply ();
//		renderer.material.mainTexture = texture;
	}
}