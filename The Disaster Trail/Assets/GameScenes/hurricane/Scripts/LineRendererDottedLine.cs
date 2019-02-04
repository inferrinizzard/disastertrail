using UnityEngine;
using UnityEngine.UI.Extensions;

public class LineRendererDottedLine : MonoBehaviour
{
	[SerializeField] private RectTransform hurricane;
	[SerializeField] private RectTransform house;
	private UILineRenderer lr;

    void Start()
    {
		lr = GetComponent<UILineRenderer>();
    }

    void Update()
    {
		lr.Points = new Vector2[] { hurricane.localPosition, house.localPosition };
    }
}
