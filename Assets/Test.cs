using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {
	public GameObject sp;
	public float[,] a, b, c;
	public RandomBoxMuller rbm;

	// Use this for initialization
	void Start () {
	}

	void calcRand(){
		Hashtable h = new Hashtable();
		for(int i = 0; i < 100000; i++){
			float r = rbm.next();
			int ri = (int)(r * 100);
			if(h[ri] != null)
				h[ri] = (int)h[ri] + 1;
			else
				h[ri] = 1;
		}
		for(int i = -1000; i < 1000; i++){
			// Debug.Log(rbm.next());
			if(h[i] != null)
				Instantiate (sp, new Vector3(i/2, (int)h[i], 0), transform.rotation);
		}
	}

	public class NormalRandom // 多分正規乱数を返す.BoxMuller.csを使っているので使わない.
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
	}

	// Update is called once per frame
	void Update () {

	}
}
