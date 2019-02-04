using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassShardTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
			// Called by the wall window prefab when player steps into its break trigger
			// Spawns flying glass shards, make window material transparent then 
			// deactivates the window break trigger so glass doesn't keep flying after the window has been shattered
			SoundManager.instance.PlaySFX(SoundEffect.GlassBreak);
			transform.parent.GetChild(0).GetComponent<GlassShardSpawner>().spawnGlass();
			transform.parent.GetChild(1).GetComponent<Renderer>().material.SetFloat("_Opacity", 0);
			transform.parent.GetChild(2).gameObject.SetActive(false);
		}
    }
}
