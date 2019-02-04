using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Priority_Queue;
using UnityEngine.UI;

public class FloodGameManager : MonoBehaviour
{
	[SerializeField] private Transform startObj, endObj, player;
	[SerializeField] private int numBlocksHorizontal;
	[SerializeField] private int numBlocksVertical;
	[SerializeField] private float blockDist;
	[SerializeField] private float roadWidth;
	[SerializeField] private float pathUpdateDelay;
	[SerializeField] private float endGameCheckDelay;
	[SerializeField] private LayerMask playerLayer;
	[SerializeField] private List<GameObject> roadBlockPrefabs;
	[SerializeField] private List<GameObject> roadObstaclePrefabs;
	[SerializeField] private float giveDirectionDistance;
	[SerializeField]
	[Tooltip("The maximum angle away from the intersection the player can look before the turn signal doesn't show up")]
	private float maxLookTurnAngle;

	[SerializeField] private GameObject FloodWaterPlane;


	[Header("UI Direction Arrows")] [SerializeField] private Sprite leftArrow;
	[SerializeField] private Sprite rightArrow;
	[SerializeField] private Sprite upArrow;
	[SerializeField] private Sprite turnAroundArrow;
	[SerializeField] private Image directionArrow;

	[Header("Mobile Controls")] [SerializeField] private List<GameObject> mobileControls;

	private PlayerCar playerCar;
	private AdjacencyList<Vector3> referenceGraph;
	private AdjacencyList<Vector3> pathingGraph;
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
#if UNITY_IOS || UNITY_ANDROID
		mobileControls.ForEach(i => i.SetActive(true));
#endif

		playerCar = player.GetComponent<PlayerCar>();
		player.position = startObj.position;
		referenceGraph = new AdjacencyList<Vector3>(Vector3.zero);
		pathingGraph = new AdjacencyList<Vector3>(Vector3.zero);


		for (int i = 0; i < numBlocksHorizontal; i++)
		{
			for (int j = 0; j < numBlocksVertical; j++)
			{
				Vector3 pos = new Vector3(i * blockDist, 0, j * blockDist);
				foreach (Vector3 neighbor in GetNeighbors(pos))
				{
					referenceGraph.AddEdge(pos, neighbor);
					pathingGraph.AddEdge(pos, neighbor);
				}
			}
		}

		//Remove edges until there is no path found
		List<KeyValuePair<Vector3, Vector3>> removedEdges = new List<KeyValuePair<Vector3, Vector3>>();
		while (AStar(referenceGraph, startObj.position, endObj.position, out List<Vector3> pathIter))
		{
			path = pathIter;
			Vector3 start = referenceGraph.GetRandomVertex();
			List<Vector3> neighbors = referenceGraph.FindNeighbours(start);
			Vector3 neighbor = neighbors[Random.Range(0, neighbors.Count)];
			referenceGraph.RemoveEdge(start, neighbor);
			removedEdges.Add(new KeyValuePair<Vector3, Vector3>(start, neighbor));
		}

		//Add the last removed edge back in
		KeyValuePair<Vector3, Vector3> lastEdge = removedEdges.Last();
		removedEdges.RemoveAt(removedEdges.Count - 1);
		referenceGraph.AddEdge(lastEdge.Key, lastEdge.Value);

		//face the player's car in the direction of the first waypoint
		float startDir = v3ToInt[(path[1] - path[0]).normalized];
		player.eulerAngles = new Vector3(0f, startDir, 0f);

		List<Vector3> startNeighborsMids = referenceGraph.FindNeighbours(startObj.position).Select(i => (startObj.position + i) / 2).ToList();

		//spawn in roadblock prefabs along 3/4s of blocked edges
		int endIdx = removedEdges.Count * 3 / 4;
		for (int i = 0; i < endIdx; i++)
		{
			KeyValuePair<Vector3, Vector3> edge = removedEdges[i];
			GameObject toInstantiate = roadBlockPrefabs[Random.Range(0, roadBlockPrefabs.Count)];
			Vector3 midpoint = (edge.Key + edge.Value) / 2;
			if (startNeighborsMids.Any(x => x == midpoint))
				continue;

			Quaternion rot = Quaternion.Euler(0f, Mathf.Abs((edge.Key - edge.Value).x) > 0f ? 90f : 0f, 0f);
			Instantiate(toInstantiate, midpoint, rot);
			pathingGraph.RemoveEdge(edge.Key, edge.Value);
		}

		//spawn in obstacles along valid edges
		HashSet<Vector3> edgeSet = new HashSet<Vector3>();

		//prevent items from spawning on edges surrounding player's spawn point
		startNeighborsMids.ForEach(i => edgeSet.Add(i));
		foreach (Vector3 vertex in referenceGraph.GetAllEdges())
		{
			foreach (Vector3 neighbor in referenceGraph.FindNeighbours(vertex))
			{
				Vector3 mid = (vertex + neighbor) / 2;
				if (!edgeSet.Contains(mid) && Random.Range(0, 2) > 0)
				{
					GameObject toInstantiate = roadObstaclePrefabs[Random.Range(0, roadObstaclePrefabs.Count)];
					bool onX = Mathf.Abs((vertex - neighbor).x) > 0;
					float randStreetOffsetLong = Random.Range(-blockDist / 2, blockDist / 2);
					float randStreetOffsetWide = Random.Range(-roadWidth / 4, roadWidth / 4);
					Vector3 boundedRandomPosInStreet = mid;
					boundedRandomPosInStreet += onX ? new Vector3(randStreetOffsetLong, 0f, randStreetOffsetWide) : new Vector3(randStreetOffsetWide, 0f, randStreetOffsetLong);
					Quaternion rot = Quaternion.Euler(0f, onX ? Random.Range(60, 120) : Random.Range(-30, 30), 0f);
					Instantiate(toInstantiate, boundedRandomPosInStreet, rot);
				}
				edgeSet.Add(mid);
			}
		}


		StartCoroutine(UpdatePathOnDelay(pathUpdateDelay));
		StartCoroutine(EndGameCheckOnDelay(endGameCheckDelay, endObj.position, new Vector3(roadWidth, roadWidth, roadWidth) / 2f, playerLayer));

		//Start raising the flood water plane
		StartCoroutine(RaiseFloodPlane());
	}

	private IEnumerator RaiseFloodPlane()
	{
		//Raise the flood water plane gradually until it reaches a certain height
		while (FloodWaterPlane.transform.position.y < 2)
		{
			FloodWaterPlane.transform.position += new Vector3(0, Time.deltaTime * 0.001f, 0);
			yield return null;
		}
	}

	private IEnumerator EndGameCheckOnDelay(float delay, Vector3 position, Vector3 halfScale, LayerMask layer)
	{
		while (Physics.OverlapBox(position, halfScale, Quaternion.identity, layer).Length == 0)
		{
			yield return new WaitForSeconds(delay);
		}
        Debug.Log("ending");

        //player has reached the end destination. Start end minigame processing
        GameManager.instance.FadeToBlack(.1f,() =>{
            GameManager.instance.LoadScene(2);
        });
	}

	#region PathFinding

	private IEnumerator UpdatePathOnDelay(float delay)
	{
		while (true)
		{
			Vector3 next = GetNextIntersection();
			Vector3 prev = GetPreviousIntersection();

			if (next != Vector3.zero)
			{
				if (!pathingGraph.ContainsEdge(next, prev) && Vector3.Distance(player.position, next) > Vector3.Distance(player.position, prev))
				{
					Debug.Log("Not in pathing graph, " + next + ", " + prev);
					path.ForEach(i => Debug.Log(i));
					directionArrow.sprite = turnAroundArrow;
					directionArrow.gameObject.SetActive(true);
				}
				else if (!referenceGraph.ContainsEdge(next, prev) && Vector3.Distance(player.position, next) > Vector3.Distance(player.position, prev))
				{
					//edge exists in pathing graph, but not in reference graph. Queue interaction to block edge in pathing graph
					pathingGraph.RemoveEdge(next, prev);
					GameObject toInstantiate = roadBlockPrefabs[Random.Range(0, roadBlockPrefabs.Count)];
					Vector3 midpoint = (next + prev) / 2;
					Quaternion rot = Quaternion.Euler(0f, Mathf.Abs((next - prev).x) > 0f ? 90f : 0f, 0f);
					Instantiate(toInstantiate, midpoint, rot);
				}
				else
				{
					AStar(pathingGraph, next, endObj.position, out path);
					if (path.Count >= 2)
					{
						Vector3 nextDir = (path[1] - path[0]).normalized;
						TurnDirection(next, nextDir);
						if (directionArrow.sprite == turnAroundArrow)
						{
							Debug.Log(next + ", " + prev);
							path.ForEach(i => Debug.Log(i));
						}
					}
					else
					{
						directionArrow.gameObject.SetActive(false);
					}
				}
			}
			else
			{
				directionArrow.gameObject.SetActive(false);
			}

			yield return new WaitForSeconds(delay);
		}
	}

	private void TurnDirection(Vector3 nextPos, Vector3 nextDir)
	{
		Vector3 approachDir = intToV3[(int)Mathf.Round(player.eulerAngles.y / 90f) % 4];
		if (approachDir == nextDir)
			directionArrow.sprite = upArrow;
		else if (rightTurn[approachDir] == nextDir)
			directionArrow.sprite = rightArrow;
		else if (-rightTurn[approachDir] == nextDir)
			directionArrow.sprite = leftArrow;
		else
			directionArrow.sprite = turnAroundArrow;

		directionArrow.gameObject.SetActive((nextPos - player.position).magnitude < giveDirectionDistance || directionArrow.sprite == turnAroundArrow);
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

		if (Vector3.Angle(player.transform.forward, dest - player.transform.position) > maxLookTurnAngle)
		{
			return Vector3.zero;
		}

		return dest;
	}

	private Vector3 GetPreviousIntersection()
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
					Mathf.Floor((player.position.z) / blockDist) * blockDist
				);
				break;
			case 1:
				dest = new Vector3
				(
					Mathf.Floor((player.position.x) / blockDist) * blockDist,
					0f,
					Mathf.Round(player.position.z / blockDist) * blockDist
				);
				break;
			case 2:
				dest = new Vector3
				(
					Mathf.Round(player.position.x / blockDist) * blockDist,
					0f,
					Mathf.Ceil((player.position.z) / blockDist) * blockDist
				);
				break;
			case 3:
				dest = new Vector3
				(
					Mathf.Ceil((player.position.x) / blockDist) * blockDist,
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
		if (pos.x + blockDist < blockDist * numBlocksHorizontal)
			neighbors.Add(new Vector3(pos.x + blockDist, 0, pos.z));

		if (pos.z - blockDist >= 0)
			neighbors.Add(new Vector3(pos.x, 0, pos.z - blockDist));
		if (pos.z + blockDist < blockDist * numBlocksVertical)
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

	#endregion
}