using UnityEditor;
using UnityEngine;

public class StreetTiler : EditorWindow
{
    GameObject streetTile, parent;
    Vector3 pos, rot;
    int num;

    [MenuItem("Window/StreetTiler")]
    public static void ShowWindow()
    {
        GetWindow<StreetTiler>("StreetTiler");
    }

    private void OnGUI ()
    {
        GUILayout.Label("Street Tiler", EditorStyles.boldLabel);
        GUILayout.Label("Duplicates a specified street tile under the given parent.");
        streetTile = (GameObject)EditorGUILayout.ObjectField("Street Tile Prefab: ", streetTile, typeof(GameObject), false);
        parent = (GameObject)EditorGUILayout.ObjectField("Parent Object: ", parent, typeof(GameObject), true);

        num = EditorGUILayout.IntField("Number of tiles", num);
        pos = EditorGUILayout.Vector3Field("Start position relative to parent", pos);
        rot = EditorGUILayout.Vector3Field("Rotation relative to parent", rot);

        if (GUILayout.Button("Generate Street"))
        {
            // Get the meshes's bounding box in local space to position the mesh properly
            Mesh mesh = streetTile.GetComponent<MeshFilter>().sharedMesh;
            Vector3 size = mesh.bounds.size;
            for (int i=0; i < num; i++)
            {
                GameObject clone = Instantiate(streetTile, Vector3.zero, Quaternion.Euler(rot), parent.transform);
                clone.transform.localPosition = new Vector3(pos.x, pos.y, pos.z + (size.z * i));
            }
        }
    }
}
