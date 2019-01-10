using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloodGameManager : MonoBehaviour
{
	[SerializeField] private Transform player;
	[SerializeField] private float blockDist;
	[SerializeField] private float minDistToEdge;
	[SerializeField] private Transform map;

	int streets, avenues, horCount = 5;
	//Vector3[] path = new Vector3[12];

	private PlayerCar playerCar;
	private Dictionary<Vector3, Vector3> rightTurn;

	private void Start()
	{
		playerCar = player.GetComponent<PlayerCar>();

		rightTurn = new Dictionary<Vector3, Vector3>
		{
			{ Vector3.forward, Vector3.right },
			{ Vector3.right, -Vector3.forward },
			{ -Vector3.forward, Vector3.left },
			{ Vector3.left, Vector3.forward }
		};

		List<Vector3> path = GeneratePath(player.position, Vector3.forward, 12);
		path.ForEach(i => Debug.Log(i));
	}

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

		return dist > minDistToEdge;
	}
}
