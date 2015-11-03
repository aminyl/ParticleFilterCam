using UnityEngine;
using System.Collections;

public class Calc : MonoBehaviour
{
	public GameObject plot;
	NormalRandom randn_a = new NormalRandom (0.0f, 1.0f);
	// NormalRandom randn_b = new NormalRandom (3.0f, 1.0f);
	float a, b, p, x;
	// int[] xs = new int[3000];
	float minr, maxr;

	Hashtable h = new Hashtable();


	// Use this for initialization
	void Start ()
	{
		h.Add(123,1);
		Debug.Log (h[123]);
		h[123]=(int)h[123]+1;

		Debug.Log (h[123]);
		Debug.Log (h[1]=1);
		Debug.Log (h[1]);

	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetMouseButtonDown (0)) {
			mnPlotTest ();
		}
	}

	void mnPlotTest () // Nのヒストグラム.
	{
		int n = 10000;
		for (int i = 0; i < n; i++) {
			int x = (int)((randn_a.NextDouble () + randn_a.NextDouble () + 20)* 10.0f);
			if(h[x]==null)
				h[x] = 1;
			else
				h[x] = (int)h[x] + 1;
		}
		for (int i = -3000; i < 3000; i++){
			if(h[i] != null)
				Instantiate (plot, new Vector3 ((float)i, (float)((int)h[i])/n*1000, 0), transform.rotation);
		}
	}

	void nPlotTest () // Nのグラフ描画.
	{
		for (int i = 0; i < 1000; i++) {
			a = 0.5f;
			b = 1.0f;
			x = Random.Range (-5.0f, 5.0f);
			p = pxw (x, a, b);
			Instantiate (plot, new Vector3 (x / 2 - 5, 5 * p, 0), transform.rotation);
		}
	}

	float pxw (float x, float a, float b)
	{
		return (1 - a) * n (x) + a * n (x - b);
	}

	float n (float x)
	{
		return 1 / Mathf.Sqrt (2 * 3.1415f) * Mathf.Exp (-x * x / 2);
	}

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
			float sum = 0;
			for (int i = 0; i < count; ++i)
				sum += Random.Range (0.0f, 1.0f);
			return (sum - 6.0f) * _standardDeviation + _mean;
		}
	}
}
