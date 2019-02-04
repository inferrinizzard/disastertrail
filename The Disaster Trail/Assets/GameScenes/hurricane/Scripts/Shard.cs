using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shard : MonoBehaviour
{
    [SerializeField] private float life = 3;
    private float currentHealth, opacity;
    private Renderer rend;

    private void Start()
    {
        currentHealth = life;
        rend = GetComponent<Renderer>();
    }

    private void Update()
    {
        // Destroy glass shard object after some time has passed

        opacity = Mathf.Lerp(0, 1, currentHealth / life);
        rend.material.SetFloat("_Opacity", opacity);

        currentHealth -= Time.deltaTime;
        if (currentHealth < 0)
        {
            Destroy(gameObject);
        }
    }
}
