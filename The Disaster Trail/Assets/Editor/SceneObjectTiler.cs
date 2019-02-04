using UnityEditor;
using UnityEngine;

public class SceneObjectTiler : EditorWindow
{
    private GameObject obj;
    private int num;
    private float totalDis, offsetDis = 0;
    private bool showObjError = false;

    [MenuItem("Window/SceneObject Tiler")]
    public static void ShowWindow()
    {
        GetWindow<SceneObjectTiler>("SceneObject Tiler");
    }

    // Updates the editor window 10 times per second
    // Lets the user see the currently selected object even if the cursor is not in
    // the editor window
    private void OnInspectorUpdate()
    {
        Repaint();    
    }

    private void OnGUI ()
    {
        GUILayout.Label("SceneObject Tiler", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Duplicates a selected scene object several times and groups them under a new gameobject.", MessageType.None);

        GUILayout.Label("Currently selected object: " + (obj == null ? "None" : obj.name), EditorStyles.label);

        obj = Selection.activeGameObject;
        num = EditorGUILayout.IntField("Number of tiles", num);
        offsetDis = EditorGUILayout.FloatField("Spacing distance", offsetDis);

        // Allow only scene objects and not asset prefabs
        if (obj != null && obj.scene.rootCount == 0)
        {
            obj = null;
        }

        if (obj != null)
        {
            showObjError = false;
        }

        if (showObjError)
        {
            EditorGUILayout.HelpBox("No object selected, please select a scene object.", MessageType.Error);
        }

        if (obj != null)
        {
            Renderer mesh = obj.GetComponent<Renderer>();
            if (mesh == null)
            {
                EditorGUILayout.HelpBox("Object does not have a mesh renderer. Spacing between each object will be equal to the distance.", MessageType.Info);
            }
        }

        if (GUILayout.Button("Tile SceneObject"))
        {

            if (obj == null)
            {
                showObjError = true;
            } else
            {
                // Creates an empty gameobject settings its rotation and position to match the prefab
                GameObject group = new GameObject("GameObject Group");
                Selection.activeGameObject = group;
                group.transform.position = obj.transform.position;
                group.transform.rotation = obj.transform.rotation;

                // Get the meshes's bounding box to position the mesh properly
                Renderer mesh = obj.GetComponent<Renderer>();

                // Calculate proper spacing between each object if it has a mesh renderer, otherwise
                // use the offset distance
                if (mesh == null)
                {
                    totalDis = offsetDis;
                } else
                {
                    // Set the original scene object to zero rotation temporarily to get correct bound size
                    obj.transform.rotation = Quaternion.Euler(Vector3.zero);
                    Vector3 size = mesh.bounds.size;
                    // Revert the original object rotation and parent it under the gameobject group 
                    obj.transform.rotation = group.transform.rotation;
                    // Set total distance between each instance
                    totalDis = size.z + offsetDis;
                }

                obj.transform.parent = group.transform;

                // Instantiates objects taking into account the mesh bounds
                // Instances are parented under the empty gameobject
                for (int i = 1; i < num; i++)
                {
                    GameObject clone = Instantiate(obj, Vector3.zero, group.transform.rotation, group.transform);
                    clone.transform.localPosition = new Vector3(0, 0, totalDis * i);
                }
                group = null;
            }
            obj = null;

        }
    }
}
