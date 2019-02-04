using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowWoodCoverAnim : MonoBehaviour
{
    [SerializeField] private int increments = 20;
    private float opacity = 0;

    private void Start()
    {
        GetComponent<Renderer>().material.SetFloat("_Opacity", 0);
    }

    public void BoardWindow()
    {
        gameObject.SetActive(true);
        if (opacity < 1)
        {
            opacity += 1f / increments;
            GetComponent<Renderer>().material.SetFloat("_Opacity", opacity);
        }
    }
}
