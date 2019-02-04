using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowTriggers : MonoBehaviour
{
    [SerializeField] private float waitTime = 5f;
    [SerializeField] private int chance = 10;

    public bool broken { get; private set; }
    private bool tarped;
    private bool repaired;
    private bool inCoroutine = false;
    [SerializeField] private int health = 1;

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        broken = false;
    }

    private void Update()
    {
        if (!inCoroutine) StartCoroutine(WaitToBreak());
    }


    private IEnumerator WaitToBreak()
    {
        inCoroutine = true;
        yield return new WaitForSecondsRealtime(waitTime);
        inCoroutine = false;
        if (!broken && Random.Range(0, chance) == 1)
        {
            if(health == 1)
            {
                transform.parent.GetChild(0).GetComponent<GlassShardSpawner>().spawnGlass();
                broken = true;
            }
            health = health > 0 ? health--: 0;
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if(health <= 1 && other.gameObject.GetComponent<PlayerTopDown>().CanRepair(this.gameObject))
            {
                
                other.GetComponent<PlayerTopDown>().ShowRepairText(transform.position);
                other.gameObject.GetComponent<PlayerTopDown>().repairReady = true;
            }

            if(health >= 2) other.GetComponent<PlayerTopDown>().HideRepairText();




        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerTopDown>().repairReady = false;
            other.GetComponent<PlayerTopDown>().HideRepairText();
        }
    }

    public void Repair()
    {
        transform.parent.GetChild(4).GetComponent<WindowTarpAnim>().ScaleTarp();
        transform.parent.GetChild(5).GetComponent<WindowWoodCoverAnim>().BoardWindow();
        if (health <= 1)
        {
            health++;
            transform.parent.GetChild(1).GetComponent<Renderer>().material.SetFloat("_Opacity", 1);
        }        

    }
}
