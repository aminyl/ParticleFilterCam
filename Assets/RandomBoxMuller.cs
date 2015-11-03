using UnityEngine;
using System.Collections;

public class RandomBoxMuller : MonoBehaviour
{
	public float next (float mu = 0.0f, float sigma = 1.0f, bool getCos = true)
	{
		float rand, rand2 = Random.value;
		while ((rand = Random.value) == 0.0f);
		float normrand = Mathf.Sqrt (-2.0f * Mathf.Log (rand));
		if (getCos)
			normrand *= Mathf.Cos (2.0f * Mathf.PI * rand2);
		else
			normrand *= Mathf.Sin (2.0f * Mathf.PI * rand2);
		return normrand * sigma + mu;
	}
	
	public float[] nextPair (float mu = 0.0f, float sigma = 1.0f)
	{
		float[] normrand = new float[2];
		float rand, rand2 = Random.value;
		while ((rand = Random.value) == 0.0f);
		normrand [0] = Mathf.Sqrt (-2.0f * Mathf.Log (rand)) * Mathf.Cos (2.0f * Mathf.PI * rand2);
		normrand [0] = normrand [0] * sigma + mu;
		normrand [1] = Mathf.Sqrt (-2.0f * Mathf.Log (rand)) * Mathf.Sin (2.0f * Mathf.PI * rand2);
		normrand [1] = normrand [1] * sigma + mu;
		return normrand;
	}
}
