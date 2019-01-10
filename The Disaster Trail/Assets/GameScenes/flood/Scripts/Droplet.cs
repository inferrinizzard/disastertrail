using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Droplet : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(detectHeight());
        StartCoroutine(faceCamera());
    }

    // Reset the rain droplets position, set its velocity to zero, & turn off its renderer when it is near the ground
    private IEnumerator detectHeight()
    {
        while(true)
        {
            if (transform.position.y < 0) {
                Rigidbody rb = GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                GetComponent<MeshRenderer>().enabled = false;
                transform.localPosition = Vector3.zero;
            }
            yield return null;
        }
    }

    // Make the rain droplet always face the main camera
    private IEnumerator faceCamera()
    {
        while (true)
        {
            transform.LookAt(Camera.main.transform);
            transform.localEulerAngles = new Vector3(90, transform.localEulerAngles.y, 0);
            transform.localPosition = new Vector3(0, transform.localPosition.y, 0);
            yield return null;
        }
    }
}
