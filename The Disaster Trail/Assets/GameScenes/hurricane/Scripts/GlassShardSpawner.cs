using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassShardSpawner : MonoBehaviour
{
    [SerializeField] private Vector2 Size = new Vector2(4, 3);

    [SerializeField] private GameObject glassShard;
    [SerializeField] private float shardSpeed = 100;
    [SerializeField] private int divisions = 4;
    private GameObject clone;
    private Rigidbody rb;

    public void spawnGlass()
    {
        // horizontal and vertical distances between each spawn point
        float xStepSize = Size.x / divisions;
        float yStepSize = Size.y / divisions;

        // Spawn shards evenly spread out in the wire gizmo
        for (int x = 0; x < divisions+1; x++)
        {
            for (int y = 0; y < divisions+1; y++)
            {
                clone = Instantiate(glassShard, transform.position, glassShard.transform.rotation, transform);
                SetGlobalScale(clone.transform, new Vector3(1, 1, 1));
                clone.transform.localPosition = new Vector3(0, (y * yStepSize) - (Size.y / 2), (x * xStepSize) - (Size.x / 2));
                rb = clone.GetComponent<Rigidbody>();
                rb.AddForce(transform.right * shardSpeed * Random.Range(0.5f, 1.5f));
            }
        }
    }

    public void SetGlobalScale(Transform transform, Vector3 globalScale)
    {
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
    }

    private void OnDrawGizmos()
    {
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.matrix = rotationMatrix;
        Gizmos.color = new Color(1, 0, 0, 1);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(0, Size.y, Size.x));
    }
}
