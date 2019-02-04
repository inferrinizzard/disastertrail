using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowTarpAnim : MonoBehaviour
{
    [SerializeField] private int increments = 10;
    private float InitialScaleY;

    private void Start()
    {
        InitialScaleY = transform.localScale.y;
        transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y / 10, gameObject.transform.localScale.z);
    }

    // Scale the window by increments
    public void ScaleTarp()
    {
        gameObject.SetActive(true);
        if (transform.localScale.y <= InitialScaleY)
        {
            transform.localScale += new Vector3(0, gameObject.transform.localScale.y / 10, 0);
        }
    }
}
