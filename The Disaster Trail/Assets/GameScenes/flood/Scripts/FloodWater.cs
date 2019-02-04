using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloodWater : MonoBehaviour
{
	[SerializeField] private Texture2D heightMap;
	[SerializeField] private float displaceAmount;

	private Mesh mesh;
	private Vector3[] baseVertices;

	void Start()
	{
		mesh = GetComponent<MeshFilter>().mesh;
		baseVertices = mesh.vertices;

		int numVerts = (int)Mathf.Sqrt(baseVertices.Length);
		int k = baseVertices.Length - 1;

		float minX = float.MaxValue, minY = float.MaxValue;
		float maxX = float.MinValue, maxY = float.MinValue;

		for (int i = 0; i < baseVertices.Length; i++)
		{
			minX = Mathf.Min(baseVertices[i].x, minX);
			minY = Mathf.Min(baseVertices[i].y, minY);
			maxX = Mathf.Max(baseVertices[i].x, maxX);
			maxY = Mathf.Max(baseVertices[i].y, maxY);
		}

		for (int i = 0; i < baseVertices.Length; i++)
		{
			baseVertices[i] += Vector3.up * heightMap.GetPixel(
				Mathf.FloorToInt(heightMap.width * (baseVertices[i].x - minX) / (maxX - minX)), 
				Mathf.FloorToInt(heightMap.height * (baseVertices[i].y - minY) / (maxY - minY))
			).grayscale * displaceAmount;
		}

		mesh.vertices = baseVertices;
		GetComponent<MeshFilter>().sharedMesh = mesh;
		transform.GetChild(0).GetComponent<MeshCollider>().sharedMesh = mesh;
	}
}
