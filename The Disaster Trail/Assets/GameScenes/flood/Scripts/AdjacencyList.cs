using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class AdjacencyList<K>
{
	private List<List<K>> _vertexList = new List<List<K>>();
	private Dictionary<K, List<K>> _vertexDict = new Dictionary<K, List<K>>();
	private Random rand;

	public AdjacencyList(K rootVertexKey)
	{
		AddVertex(rootVertexKey);
		rand = new Random();
	}

	private List<K> AddVertex(K key)
	{
		List<K> vertex = new List<K>();
		_vertexList.Add(vertex);
		_vertexDict.Add(key, vertex);

		return vertex;
	}

	public void AddEdge(K startKey, K endKey)
	{
		List<K> startVertex = _vertexDict.ContainsKey(startKey) ? _vertexDict[startKey] : null;
		List<K> endVertex = _vertexDict.ContainsKey(endKey) ? _vertexDict[endKey] : null;

		if (startVertex == null)
			startVertex = AddVertex(startKey);

		if (endVertex == null)
			endVertex = AddVertex(endKey);

		if (!startVertex.Contains(endKey))
			startVertex.Add(endKey);

		if (!endVertex.Contains(startKey))
			endVertex.Add(startKey);
	}

	public void RemoveVertex(K key)
	{
		List<K> vertex = _vertexDict[key];

		//First remove the edges / adjacency entries
		int vertexNumAdjacent = vertex.Count;
		for (int i = 0; i < vertexNumAdjacent; i++)
		{
			K neighbourVertexKey = vertex[i];
			RemoveEdge(key, neighbourVertexKey);
		}

		//Lastly remove the vertex / adj. list
		_vertexList.Remove(vertex);
		_vertexDict.Remove(key);
	}

	public void RemoveEdge(K startKey, K endKey)
	{
		((List<K>)_vertexDict[startKey]).Remove(endKey);
		((List<K>)_vertexDict[endKey]).Remove(startKey);

		if (VertexDegree(startKey) == 0)
			RemoveVertex(startKey);

		if (VertexDegree(endKey) == 0)
			RemoveVertex(endKey);
	}

	public bool Contains(K key)
	{
		return _vertexDict.ContainsKey(key);
	}

	public bool ContainsEdge(K first, K second)
	{
		return _vertexDict.ContainsKey(first) && _vertexDict[first].Contains(second);
	}

	public int VertexDegree(K key)
	{
		return _vertexDict[key].Count;
	}

	public List<K> FindNeighbours(K key)
	{
		return _vertexDict[key];
	}

	public K GetRandomVertex()
	{
		return _vertexDict.ElementAt(rand.Next(0, _vertexDict.Count)).Key;
	}

	public List<K> GetAllEdges()
	{
		return _vertexDict.Keys.ToList();
	}
}