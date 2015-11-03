using UnityEngine;
using System.Collections;

public class ParticleFilter : MonoBehaviour
{
	public WebCamBehaviour wcb;
	WebCamTexture wct;
	public GameObject res; // 認識結果,予想位置.
	int width, height;
	Texture2D texture;
	Color32[] c; // wcbからの入力画像.
	public GameObject objParticle;
	GameObject[] objParticles; // 表示用.
	public int particleNum = 100;
	particle_type[] particles;
	public GameObject container; // パーティクルがHierarchyを埋め尽くさないようにまとめる.
	public RandomBoxMuller rbm;
	GameObject[] inColorPos;
	public float variance = 2.0f; // パーティクルが次に動く量.
	Color32 targetColor = new Color32 (0, 0, 0, 0);
	public int judgeMode = 0;
	public bool showGroundTruth = true, showParticles = true, showRes = true;

	public struct particle_type
	{
		public int x, y;
		public float weight;

		public particle_type (int x, int y, float weight)
		{
			this.x = x;
			this.y = y;
			this.weight = weight;
		}

		public Vector3 pos ()
		{
			return new Vector3 (x, y, 0);
		}
	}

	void Start ()
	{
	}

	void Update ()
	{
		if (wct == null) { // webcamTexture情報取得,start関数とかに書きたい.
			wct = wcb.wct;
			if (wct != null)
				initialize ();
			return;
		}

		c = wct.GetPixels32 (); // カメラの画像読み込み.

		resample (); // リサンプリング.重みをリセットするのでweight()の前に呼ぶ.
        predict(); // 位置予測.
		weight(); // 重みづけ.
		if(showParticles)
			showParticle ();
		if(showRes)
			show_res (measure ()); // 認識結果を表示,検出した物体を囲う枠を表示する.
		// setParticlePos_ChaseColor ();

		if(showGroundTruth)
			showInColor ();
		if (Input.GetMouseButtonDown (0))
			targetColor = getClickedColor ();
	}

	void showParticle ()
	{
		for (int i = 0; i < particleNum; i++) {
			objParticles [i].transform.position = particles [i].pos ();
			// if(IsInImage(particles[i].x, particles[i].y)) objParticles[i].renderer.material.color = c[particles[i].y * width + particles[i].x]; // 色を付ける,見づらい.
		}
	}

	void setParticlePos_ChaseColor () // 特定の色に球を配置,テスト用.webcamの画素値は左下から右下に,上に拾われる.
	{
		int pn = 0;
		for (int j = 0; j < height; j += 2) {
			for (int i = 0; i < width; i += 2) {
				if (c [j * width + i].r > 150 && c [j * width + i].g > 150 && c [j * width + i].b < 80 && pn < particleNum) {
					objParticles [pn].transform.position = new Vector3 (i, j, 0);
					objParticles [pn].GetComponent<Renderer>().material.color = c [j * width + i];
					pn++;
				}
			}
		}
		for (int i = pn; i < particleNum; i++)
			objParticles [i].transform.position = new Vector3 (0, 0, 0);
	}

	void showInColor () // 真の値をもつ位置を表示.
	{
		int sum = 0;
		Color32[] rc = new Color32[width * height];
		for (int i = 0; i < width * height; i++) {
			rc [i] = c [i];
			if (IsInColor (c [i])) {
				sum++;
				rc [i] = Color.green;
			}
		}
		(texture = new Texture2D (width, height)).SetPixels32 (rc);
		texture.Apply ();
		GetComponent<Renderer>().material.mainTexture = texture;
		Debug.Log (sum);
	}

	void initialize ()
	{
		// オブジェクトの準備.
		particles = new particle_type[particleNum];
		objParticles = new GameObject[particleNum];
		for (int i = 0; i < particleNum; i++) {
			particles [i] = new particle_type (Random.Range (0, width), Random.Range (0, height), Random.Range (0.0f, 1.0f));
			objParticles [i] = Instantiate (objParticle, particles [i].pos (), objParticle.transform.rotation) as GameObject;
			objParticles [i].transform.parent = container.transform;
		}
		res = Instantiate (res, new Vector3 (-5, -5, 0), res.transform.rotation) as GameObject;

		// wcbからデータを受け取る.
		width = wcb.width;
		height = wcb.height;
		GetComponent<Renderer>().material.mainTexture = wct;
		c = wct.GetPixels32 (); // カメラの画像読み込み.

		particle_type max_particle = new particle_type (0, 0, 0);
		// 最も尤度の高いピクセルを探索.
		for (int j = 0; j < height; j++) {
			for (int i = 0; i < width; i++) {
				float weight = likelihood (i, j);
				if (weight > max_particle.weight)
					max_particle = new particle_type (i, j, weight);
			}
		}
		// すべてのパーティクルの値を最尤値で設定する.
		for (int i = 0; i < particleNum; i++)
			particles [i] = max_particle;
	}
	
	void resample ()
	{
		// 累積重みの計算.
		float[] weights = new float[particleNum];
		weights [0] = particles [0].weight;
		for (int i = 1; i < particleNum; i++)
			weights [i] = weights [i - 1] + particles [i].weight;

		// 重みを基準にパーティクルをリサンプリングして重みを1.0に.
		particle_type[] tmp_particles = new particle_type[particleNum];
		particles.CopyTo (tmp_particles, 0);

		for (int i = 0; i < particleNum; i++) {
			float weight = Random.value * weights [particleNum - 1];
			int n = 0;
			while (weights[++n] < weight);
			particles [i] = tmp_particles [n];
			// objParticles [i].transform.localScale = particles [i].weight*Vector3.one * 2;
			particles [i].weight = 1.0f;
		}
	}

	void predict () // 位置予測.
	{
		for (int i = 0; i < particleNum; i++) {
			particles [i].x += (int)(rbm.next () * variance);
			particles [i].y += (int)(rbm.next () * variance);
		}
	}

	float likelihood (int x, int y) // 粒子の周りにどれだけ目的の色があるかによって尤度を決定.
	{
		int _width = 30, _height = 30;
		// パーティクルを中心とした_width*_heightの矩形領域の色の存在率を尤度とする.
		int count = 0;
		for (int j = y - _height / 2; j < y + _height / 2; j += 1)
			for (int i = x - _width / 2; i < x + _width / 2; i += 1)
				if (IsInImage (i, j) && IsInColor (c [j * width + i]))
					count++;
		if (count == 0)
			return 0.0001f;
		else
			return (float)((float)count / (float)(_width * _height));
	}

	void weight () // 尤度に従いパーティクルの重みを決定する.
	{
		float sum_weight = 0;
		for (int i = 0; i < particleNum; i++)
			sum_weight += (particles [i].weight = likelihood (particles [i].x, particles [i].y));
		for (int i = 0; i < particleNum; i++) // 重みの正規化,しなくても動く.
			particles [i].weight = particles [i].weight / sum_weight * particleNum;
	}

	particle_type measure ()
	{
		float x = 0, y = 0, weight = 0.0001f;
		// 重み和.
		for (int i = 0; i < particleNum; i++) {
			x += particles [i].x * particles [i].weight;
			y += particles [i].y * particles [i].weight;
			weight += particles [i].weight;
		}
		// 正規化.
		return new particle_type ((int)(x / weight), (int)(y / weight), 1);
	}

	void show_res (particle_type particle) // 結果を出力.
	{
		res.transform.position = particle.pos ();
	}
	
	bool IsInImage (int x, int y) // (x,y)が画像内に収まっているかどうか.
	{
		return(0 <= x && x < width && 0 <= y && y < height);
	}
	
	bool IsInColor (Color32 c) // color(RGB値)が指定した範囲の色か.
	{
		if (judgeMode == 0)
			return IsAroundColor (c);
		else if (judgeMode == 1) {
			// return(c.r > 200 && c.g > 200 && c.b > 200);
			return(c.r > 180 && c.g > 180 && c.b < 120);
			// return(c.r > 200 && c.g > 200 && c.b < 100);
			// return(c.r < 50 && c.g < 50 && c.b < 50);
		}else
			return false;
	}

	bool IsAroundColor (Color32 c) // color(RGB値)が指定した範囲の色か,2.
	{
		int range = 50;
		return(c.r > targetColor.r - range && c.r < targetColor.r + range
			&& c.g > targetColor.g - range && c.g < targetColor.g + range
			&& c.b > targetColor.b - range && c.b < targetColor.b + range);
	}

	Color32 getClickedColor () // クリックした位置の色を取得.
	{
		Vector3 mPos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		int x = (int)mPos.x, y = (int)mPos.y;
		Color32[] cm = wcb.wct.GetPixels32 ();
		Vector3 cmean = Vector3.zero;
		int mrange = 2;
		for (int j = -mrange; j < mrange; j++) {
			for (int i = -mrange; i < mrange; i++) {
				cmean.x += cm [(y + j) * width + (x + i)].r;
				cmean.y += cm [(y + j) * width + (x + i)].g;
				cmean.z += cm [(y + j) * width + (x + i)].b;
			}
		}
		Color32 cc = new Color32 ();
		// cc = wcb.wct.GetPixels32()[y * width + x];
		cc.r = (byte)(cmean.x / (mrange * mrange * 4));
		cc.g = (byte)(cmean.y / (mrange * mrange * 4));
		cc.b = (byte)(cmean.z / (mrange * mrange * 4));
		cc.a = (byte)255;
		return cc;
	}

}
