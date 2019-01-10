using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Priority_Queue;

public class FloodGameManager : MonoBehaviour
{
	[SerializeField] private Transform startObj, endObj, player;
	[SerializeField] private int numBlocks;
	[SerializeField] private float blockDist;
	[SerializeField] private float minBlocksFromEdge;
	[SerializeField] private Transform map;

	int streets, avenues, horCount = 5;

	private PlayerCar playerCar;
	private AdjacencyList<Vector3> roadGraph;
	private List<Vector3> path;

	private readonly Vector3[] intToV3 = {
		Vector3.forward,
		Vector3.right,
		-Vector3.forward,
		Vector3.left
	};

	private readonly Dictionary<Vector3, int> v3ToInt = new Dictionary<Vector3, int>
	{
		{ Vector3.forward, 0 },
		{ Vector3.right, 90 },
		{ -Vector3.forward, 180 },
		{ Vector3.left, 270 }
	};

	private readonly Dictionary<Vector3, Vector3> rightTurn = new Dictionary<Vector3, Vector3>
	{
		{ Vector3.forward, Vector3.right },
		{ Vector3.right, -Vector3.forward },
		{ -Vector3.forward, Vector3.left },
		{ Vector3.left, Vector3.forward }
	};

	private void Start()
	{
		playerCar = player.GetComponent<PlayerCar>();
		player.position = startObj.position;
		roadGraph = new AdjacencyList<Vector3>(Vector3.zero);

		for (int i = 0; i < numBlocks; i++)
		{
			for (int j = 0; j < numBlocks; j++)
			{
				Vector3 pos = new Vector3(i * blockDist, 0, j * blockDist);
				foreach (Vector3 neighbor in GetNeighbors(pos))
				{
					roadGraph.AddEdge(pos, neighbor);
				}
			}
		}

		//Remove edges until there is no path found
		List<KeyValuePair<Vector3, Vector3>> removedEdges = new List<KeyValuePair<Vector3, Vector3>>();
		while (AStar(roadGraph, startObj.position, endObj.position, out List<Vector3> pathIter))
		{
			path = pathIter;
			Vector3 start = roadGraph.GetRandomVertex();
			List<Vector3> neighbors = roadGraph.FindNeighbours(start);
			Vector3 neighbor = neighbors[Random.Range(0, neighbors.Count)];
			roadGraph.RemoveEdge(start, neighbor);
			removedEdges.Add(new KeyValuePair<Vector3, Vector3>(start, neighbor));
		}

		//Add the last removed edge back in
		KeyValuePair<Vector3, Vector3> lastEdge = removedEdges.Last();
		removedEdges.RemoveAt(removedEdges.Count - 1);
		roadGraph.AddEdge(lastEdge.Key, lastEdge.Value);

		//face the player's car in the direction of the first waypoint
		float startDir = v3ToInt[(path[1] - path[0]) / blockDist];
		player.eulerAngles = new Vector3(0f, startDir, 0f);

		/*
		foreach (KeyValuePair<Vector3, Vector3> edge in removedEdges)
		{
			//Instantiate something on this edge
		}
		*/
		
		StartCoroutine(UpdatePathOnDelay(.5f));
	}

	private IEnumerator UpdatePathOnDelay(float delay)
	{
		while (true)
		{
			Vector3 vert = GetNextIntersection();
			AStar(roadGraph, vert, endObj.position, out path);
			if (path.Count >= 2)
			{
				Vector3 nextDir = (path[1] - path[0]) / blockDist;
				Debug.Log(TurnDirection(nextDir));
			}

			yield return new WaitForSeconds(delay);
		}
	}

	private string TurnDirection(Vector3 nextDir)
	{
		Vector3 approachDir = intToV3[(int)Mathf.Round(player.eulerAngles.y / 90f) % 4];
		if (approachDir == nextDir)
			return "Continue Straight";

		if (rightTurn[approachDir] == nextDir)
			return "Turn Right";

		if (-rightTurn[approachDir] == nextDir)
			return "Turn Left";

		return "Turn Around";
	}

	private Vector3 GetNextIntersection()
	{
		int roundedYDir = (int)Mathf.Round(player.eulerAngles.y / 90f) % 4;
		Vector3 dest;
		switch (roundedYDir)
		{
			case 0:
				dest = new Vector3
				(
					Mathf.Round(player.position.x / blockDist) * blockDist,
					0f,
					Mathf.Floor((player.position.z + blockDist) / blockDist) * blockDist
				);
				break;
			case 1:
				dest = new Vector3
				(
					Mathf.Floor((player.position.x + blockDist) / blockDist) * blockDist,
					0f,
					Mathf.Round(player.position.z / blockDist) * blockDist
				);
				break;
			case 2:
				dest = new Vector3
				(
					Mathf.Round(player.position.x / blockDist) * blockDist,
					0f,
					Mathf.Ceil((player.position.z - blockDist) / blockDist) * blockDist
				);
				break;
			case 3:
				dest = new Vector3
				(
					Mathf.Ceil((player.position.x - blockDist) / blockDist) * blockDist,
					0f,
					Mathf.Round(player.position.z / blockDist) * blockDist
				);
				break;
			default:
				dest = Vector3.zero;
				break;
		}

		return dest;
	}

	private List<Vector3> GetNeighbors(Vector3 pos)
	{
		List<Vector3> neighbors = new List<Vector3>();

		if (pos.x - blockDist >= 0)
			neighbors.Add(new Vector3(pos.x - blockDist, 0, pos.z));
		if (pos.x + blockDist < blockDist * numBlocks)
			neighbors.Add(new Vector3(pos.x + blockDist, 0, pos.z));

		if (pos.z - blockDist >= 0)
			neighbors.Add(new Vector3(pos.x, 0, pos.z - blockDist));
		if (pos.z + blockDist < blockDist * numBlocks)
			neighbors.Add(new Vector3(pos.x, 0, pos.z + blockDist));

		return neighbors;
	}

	private static bool AStar(AdjacencyList<Vector3> graph, Vector3 start, Vector3 goal, out List<Vector3> path)
	{
		//make sure that the start and end points are valid points in the graph
		if (!graph.Contains(start) || !graph.Contains(goal))
		{
			path = new List<Vector3>();
			return false;
		}

		bool success = false;
		Dictionary<Vector3, Vector3> from = new Dictionary<Vector3, Vector3>();
		HashSet<Vector3> closed = new HashSet<Vector3>();
		HashSet<Vector3> open = new HashSet<Vector3> { start };

		Dictionary<Vector3, float> gScore = new Dictionary<Vector3, float>();
		SimplePriorityQueue<Vector3, float> fScore = new SimplePriorityQueue<Vector3, float>();

		gScore.Add(start, 0);
		fScore.Enqueue(start, Vector3.Distance(start, goal));

		while (open.Count > 0)
		{
			Vector3 current = fScore.Dequeue();
			if (current == goal)
			{
				success = true;
				break;
			}

			open.Remove(current);
			closed.Add(current);

			foreach (Vector3 neighbor in graph.FindNeighbours(current))
			{
				if (closed.Contains(neighbor))
					continue;

				float dist = gScore[current] + Vector3.Distance(current, neighbor);

				if (!open.Contains(neighbor))
					open.Add(neighbor);
				else if (gScore.ContainsKey(neighbor) && dist >= gScore[neighbor])
					continue;

				from[neighbor] = current;
				gScore[neighbor] = dist;
				fScore.Enqueue(neighbor, dist + Vector3.Distance(neighbor, goal));
			}
		}

		path = FormPath(from, goal);
		return success;
	}

	private static List<Vector3> FormPath(Dictionary<Vector3, Vector3> from, Vector3 current)
	{
		List<Vector3> path = new List<Vector3> { current };
		while (from.ContainsKey(current))
		{
			current = from[current];
			path.Insert(0, current);
		}
		return path;
	}

	/*
	private List<Vector3> GeneratePath(Vector3 startPos, Vector3 startDir, int length)
	{
		if (length < 3)
		{
			Debug.LogWarning("GeneratePath called with length less than 3. Too short!", gameObject);
			return null;
		}

		List<Vector3> path = new List<Vector3> { startDir, startDir };

		Vector3 pos = startPos;
		Vector3 dir = startDir;
		for (int i = 2; i < length; i++)
		{
			Vector3 newDir = GenerateDirection(path, pos, dir);
			path.Add(newDir);

			dir = newDir;
			pos += newDir * blockDist;
		}

		return path;
	}

	private Vector3 GenerateDirection(List<Vector3> path, Vector3 pos, Vector3 dir)
	{
		if (path.Count < 2)
		{
			Debug.LogWarning("GenerateDirection called with path of length < 2", gameObject);
			return Vector3.zero;
		}

		List<Vector3> possibleDirs = new List<Vector3>();
		List<Vector3> priors = path.GetRange(path.Count - 2, 2);

		if (ValidDistToEdge(pos, dir))
		{
			possibleDirs.Add(dir);
		}

		Vector3 left = rightTurn.FirstOrDefault(x => x.Value == dir).Key;
		Vector3 right = rightTurn[dir];

		if (ValidDistToEdge(pos, left) && !priors.Contains(right))
		{
			possibleDirs.Add(left);
		}
		if (ValidDistToEdge(pos, right) && !priors.Contains(left))
		{
			possibleDirs.Add(right);
		}

		return possibleDirs[Random.Range(0, possibleDirs.Count)];
	}

	private bool ValidDistToEdge(Vector3 pos, Vector3 dir)
	{
		Vector3 bounds = map.GetComponent<Renderer>().bounds.extents;
		float dist = Mathf.Abs(Vector3.Dot(pos - map.position - bounds, dir));

		return dist > minBlocksFromEdge * blockDist;
	}
*/
}