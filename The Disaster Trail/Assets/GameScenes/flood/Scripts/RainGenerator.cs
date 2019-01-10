using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainGenerator : MonoBehaviour
{
    [SerializeField] private float rainSpeed = 100;
    [SerializeField] private GameObject rainDroplet;
    [SerializeField] private int poolAmount= 100;
    private int count = 0;
    // Start is called before the first frame update
    void Start()
    {        
        // Create a pool of rain drops to generate rain from
        for (int i=0; i < poolAmount; i++)
        {
            GameObject drop = Instantiate(rainDroplet, transform.position, rainDroplet.transform.rotation, transform);
            drop.GetComponent<MeshRenderer>().enabled = false; 
        }
        StartCoroutine(GenerateDroplet());
    }

    // Creates droplets by adding force and enabling the mesh renderer of each droplet of the rain droplet pool
    private IEnumerator GenerateDroplet()
    {
        while (true)
        {
            if (count == poolAmount)
            {
                count = 0;
            }
            Rigidbody rb = transform.GetChild(count).GetComponentInChildren<Rigidbody>();
            MeshRenderer rend = transform.GetChild(count).GetComponentInChildren<MeshRenderer>();
            rend.enabled = true;
            rb.AddForce(new Vector3(0, -rainSpeed, 0));

            yield return new WaitForSeconds(0.1f);

            count++;
        }
    }
}
