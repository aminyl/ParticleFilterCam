using UnityEngine;
using System.Collections;

public class PF_2 : MonoBehaviour
{
	public class NormalRandom
	{
		private readonly float _mean;
		private readonly float _standardDeviation;
		
		public NormalRandom (float mean, float standardDeviation)
		{
			_mean = mean;
			_standardDeviation = standardDeviation;
		}
		
		public float NextDouble ()
		{
			const int count = 12;
			float[] numbers = new float[count];
			for (int i = 0; i < count; ++i) {
				numbers [i] = Random.Range (0.0f, 1.0f);
			}
			float sum = 0;
			for (int i = 0; i < numbers.Length; i++)
				sum += numbers [i];
			return (sum - 6.0f) * _standardDeviation + _mean;
		}
	}

	public WebCamBehaviour wcb;
	WebCamTexture wct;
	int width, height;
	Texture2D texture;
	Color32[] c;
	public GameObject objParticle;
	GameObject[] objParticles;
	public int particleNum = 100;
	particle_type[] particles;
	public GameObject container;
	float timer = 0;
	NormalRandom nrnd = new NormalRandom (0, 1);


	// int max_step = 400;
	float dt = 0.05f;
	int M = 200;
	float[,] A, B;
	float[,] x, z, u;
	float[,] s_sigma, o_sigma;
	float sigma;
	float[,] Kai, Kai_bar;
	float[] w;

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
		particles = new particle_type[particleNum];
		objParticles = new GameObject[particleNum];
		for (int i = 0; i < particleNum; i++) {
			particles [i] = new particle_type (Random.Range (0, width), Random.Range (0, height), Random.Range (0.0f, 1.0f));
			objParticles [i] = Instantiate (objParticle, particles [i].pos (), objParticle.transform.rotation) as GameObject;
			objParticles [i].transform.parent = container.transform;
		}

		A = new[,]{
			{1.0f, 0},
			{0, 1.0f}};
		B = new[,]{
			{dt, 0},
			{0, dt}};
		x = new[,]{{0.0f}, {0.0f}};
		z = new[,]{{0.0f}, {0.0f}};
		u = new[,]{{1.0f}, {1.0f}};
		s_sigma = new[,]{
			{0.1f, 0},
			{0, 0.1f}};
		o_sigma = new[,]{
			{0.1f, 0},
			{0, 0.1f}};
		sigma = o_sigma [0, 0];

		Kai = new float[2, M];
		Kai_bar = new float[3, M];
		w = new float[M];
		for (int j = 0; j < 2; j++)
			for (int i = 0; i < M; i++)
				Kai [j, i] = 0;
		for (int j = 0; j < 3; j++)
			for (int i = 0; i < M; i++)
				Kai_bar [j, i] = 0;
		for (int i = 0; i < M; i++)
			w [i] = 1.0f / M;
	}

	float[,] repmat21M(float[,] x){
		float[,] res = new float[2, M];
		for(int i = 0; i < M; i++){
			res[0, M] = x[0, 0];
			res[1, M] = x[1, 0];
		}
		return res;
	}
	float[,] randn(int x, int y){
		float[,] res = new float[x, y];
		for(int i = 0; i < x; i++)
			for(int j = 0; j < y; j++)
				res[i, j] = nrnd.NextDouble();
		return res;
	}

	void Update ()
	{
		if (timer < 2.0f) {
			timer += Time.deltaTime;
			return;
		}
		
		wct = wcb.wct;
		c = wct.GetPixels32 ();
		width = wcb.width;
		height = wcb.height;
		GetComponent<Renderer>().material.mainTexture = wct;


		// ------------------------------------------------------------------------
		// 真値.
		// 状態更新.
		x = dotVec(dotVec(dotVec(A ,x), dotVec(B ,u)), dotVec(s_sigma, randn(2, 1)));

		// 観測.
		z = dotVec(dotVec(o_sigma, randn(2, 1)), x);
		
		// ------------------------------------------------------------------------
		// particle filter
		// サンプリング
		Kai = dotVec(dotVec(dotVec(A ,Kai) ,repmat21M(dotVec(B, u))) ,dotVec(s_sigma ,randn(2, M)));
		
		// 尤度を計算
		for(int i = 0; i < M; i++)
			w[i] = 1.0f/(Mathf.Sqrt(2.0f* Mathf.PI) * sigma)
				* Mathf.Exp(
					-(
					(Kai[1, i] - z[1, 1])*(Kai[1, i] - z[1, 1])
				              /(2.0f * sigma * sigma)
					)
					)
				/(Mathf.Sqrt(2.0f*Mathf.PI)*sigma)
					* Mathf.Exp(
					-(Kai[2,i]-z[2,1])*(Kai[2,i]-z[2,1])/(2.0f*sigma*sigma)
					);

		// 重みを正規化
		float w_sum = 0;
		for(int i = 0; i < M; i++)
			w_sum += w[i];
		for(int i = 0; i < M; i++)
			w[i] /= w_sum;
		
		// Kai_barに入れておく
		for(int i = 0; i < M; i++)
			for(int j = 0; j < 2; j++)
				Kai_bar[j, i] = Kai[j, i];
		for(int i = 0; i < M; i++)
				Kai_bar[2, i] = w[i];

		// リサンプリング
		// 重みを入れる箱(無駄が多い)
		float[,] box = new float[M + 1, 1];
		for (int i = 0; i < M - 1; i++)
			box[i + 1, 0] = box[i, 0] + w[i];

		box[M, 0] = 1.0f;
		
		// くじ引き
		for (int i = 0; i < M; i++){
			float r = Random.Range(0.0f, 1.0f);
			int num = 0;
		
			// 乱数がどの箱かを調べる(遅い)
			for (int j = 0; j < M; j++){
				if((box[j, 0] < r) && (r < box[j + 1, 0])){
					num = j;
				}
			}
			// 新たなKaiに代入
			Kai[0, i] = Kai_bar[0, num];
			Kai[1, i] = Kai_bar[1, num];
		}
			
			
		// ------------------------------------------------------------------------
		// 描画
			

		// パーティクル
		for (int i = 0; i < M; i++) {
			objParticles [i].transform.position = new Vector3(Kai[0,i], Kai[1, i], 0);
			// objParticles [i].renderer.material.color = c [Kai[1,i] * width + Kai[0, i]];
		}
	}
	
	void setParticlePos ()
	{
		for (int i = 0; i < particleNum; i++) {
			objParticles [i].transform.position = particles [i].pos ();
			objParticles [i].GetComponent<Renderer>().material.color = c [particles [i].y * width + particles [i].x];
		}
	}
	
	void setParticlePos_ChaseColor ()
	{
		//		for(int i = 0; i < particleNum; i++)
		//			objParticles[i].transform.position = particles[i].pos();
		int pn = 0;
		for (int j = 0; j < height; j+=2) {
			for (int i = 0; i < width; i+=2) {
				//				objParticles [pn].transform.position = new Vector3 (i, j, 0);
				//				objParticles [pn].renderer.material.color = c [j * width + i];
				//				pn++;
				
				if (c [j * width + i].r > 150 && c [j * width + i].g > 150 && c [j * width + i].b < 80 && pn < particleNum) {
					// Debug.Log (c[j * width + i]);
					objParticles [pn].transform.position = new Vector3 (i, j, 0);
					objParticles [pn].GetComponent<Renderer>().material.color = c [j * width + i];
					pn++;
				}
			}
		}
		for (int i = pn; i < particleNum; i++) {
			objParticles [i].transform.position = new Vector3 (0, 0, 0);
		}
	}
	
	bool resample (particle_type[] particles)
	{
		// 累積重みの計算.
		float[] weights = new float[particles.Length];
		weights [0] = particles [0].weight;
		for (int i = 1; i < weights.Length; i++)
			weights [i] = weights [i - 1] + particles [i].weight;
		
		// 重みを基準にパーティクルをリサンプリングして重みを1.0に.
		particle_type[] tmp_particles = particles;
		for (int i = 0; i < particles.Length; i++) {
			// float weight = nrnd.NextDouble() * weights[weights.Length - 1];
			// float weight = Mathf.Abs(nrnd.NextDouble()) * weights[weights.Length - 1];
			float weight = Random.Range (-0.1f, 0.1f) * weights [weights.Length - 1];
			int n = 0;
			while (weights[++n] < weight)
				;
			particles [i] = tmp_particles [n];
			particles [i].weight = 1.0f;
		}
		return true;
	}
	
	bool predict (particle_type[] particles)
	{
		float variance = 13.0f;
		// 位置の予測.
		// 「次状態もほぼ同じ位置(ほとんど動かない)」と仮定、分散(13.0)は実験的に決定.
		for (int i = 0; i < particles.Length; i++) {
			float vx = Random.Range (0.0f, 1.0f) * variance;
			float vy = Random.Range (0.0f, 1.0f) * variance;
			
			particles [i].x += (int)vx;
			particles [i].y += (int)vy;
		}
		return true;
	}
	
	float likelihood (int x, int y, Color32[] c)
	{
		int _width = 30;
		int _height = 30;
		// 今回はパーティクルを中心とした30x30の矩形領域に指定した範囲の色の存在率を尤度とした.
		int count = 0;
		int p = 0;
		for (int j = y - _height / 2; j < y + _height / 2; j ++) {
			for (int i = x - _width / 2; i < x + _width / 2; i ++) {
				if (IsInImage (i, j) && IsInColor (i, j, c [p]))
					count++;
				p++;
			}
		}
		if (count == 0)
			return 0.0001f;
		else
			return (float)count / (_width * _height);
	}
	
	bool weight (particle_type[] particles, Color32[] c)
	{
		// 尤度に従いパーティクルの重みを決定する.
		float sum_weight = 0;
		for (int i = 0; i < particles.Length; i++) {
			particles [i].weight = likelihood (particles [i].x, particles [i].y, c);
			sum_weight += particles [i].weight;
		}
		// 重みの正規化.
		for (int i = 0; i < particles.Length; i++)
			particles [i].weight = (particles [i].weight / sum_weight) * particles.Length;
		
		return true;
	}
	
	particle_type measure (particle_type[] particles)
	{
		float x = 0;
		float y = 0;
		float weight = 0;
		// 重み和.
		for (int i = 0; i < particles.Length; i++) {
			x += particles [i].x * particles [i].weight;
			y += particles [i].y * particles [i].weight;
			weight += particles [i].weight;
		}
		// 正規化.
		return new particle_type ((int)(x / weight), (int)(y / weight), 1);
	}
	
	// (x,y)が画像内に収まっているかどうか.
	bool IsInImage (int x, int y)
	{
		return(0 <= x && x < width && 0 <= y && y < height);
	}
	
	// color(RGB値)が指定した範囲の色か.
	bool IsInColor (int i, int j, Color32 c)
	{
		return(c.r < 200 && c.g < 200 && c.b < 200);
	}
	float[,] addVec(float[,] x, float[,] y){
		int xy = x.GetLength (0), xx = x.GetLength (1);
		int yy = y.GetLength (0), yx = y.GetLength (1);
		if(!(xx == yx && xy == yy)){
			Debug.Log ("配列の長さが適切ではない @ addVec");
			return null;
		}
		float[,] res = new float[xy, xx];
		
		for(int j = 0; j < xy; j++){
			for(int i = 0; i < xx; i++){
				res[j, i] = x[j, i] + y[j, i];
			}
		}
		return res;
	}
	
	float[,] dotVec(float[,] x, float[,] y){
		int xy = x.GetLength (0), xx = x.GetLength (1);
		int yy = y.GetLength (0), yx = y.GetLength (1);
		if(xx != yy){
			Debug.Log ("配列の長さが適切ではない @ dotVec");
			Debug.Log ("xx:" + xx + ", " + "xy:" + xy + ", " + "yx:" + yx + ", " + "yy:" + yy + ", ");
			return null;
		}
		float[,] res = new float[xy, yx];
		
		for(int k = 0; k < xy; k++){
			for(int j = 0; j < yx; j++){
				float sum = 0;
				for(int i = 0; i < xx; i++){
					sum += x[k, i] * y[i, j];
				}
				res[k, j] = sum;
			}
		}
		return res;
	}
	
	float[,] scVec(float[,] x, float y){
		return scVec(y, x);
	}
	
	float[,] scVec(float x, float[,] y){
		int yy = y.GetLength (0), yx = y.GetLength (1);
		float[,] res = new float[yy, yx];
		
		for(int j = 0; j < yy; j++){
			for(int i = 0; i < yx; i++){
				res[j, i] = x * y[j, i];
			}
		}
		return res;
	}}
