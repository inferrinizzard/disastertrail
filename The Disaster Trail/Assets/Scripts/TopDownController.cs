using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInventory))]
public class TopDownController : MonoBehaviour
{


    [SerializeField] private float speed = 50f;
    [SerializeField] private float rotSpeed = 10f;
    [SerializeField] private float wallCheckDistance;
    //[SerializeField] private float distance = 1f;
    [SerializeField] private LayerMask NPCLayer;

    private Rigidbody rb;
    private Plane ground;
    private PlayerInventory inventory;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ground = new Plane(Vector3.up, Vector3.zero);
        //upperFlashlightIntensity = upperFlashlight.GetComponent<Light>().intensity;
        inventory = GetComponent<PlayerInventory>();

    }

    void Update()
    {       
        
        //NPCCheck(Input.GetKeyDown(KeyCode.E), distance, NPCLayer);

        if (Input.GetKeyDown(KeyCode.P))
        {
            inventory.DebugInventory();
        }
    }

    void FixedUpdate()
    {
        float hor = Input.GetAxis("Horizontal");
        float ver = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(hor, 0, ver);
        if (move.magnitude > 0)
        {
            rb.AddForce(move * speed);
        }

        //adjust player rotation to stay looking towards the mouse
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float enter;
        if (ground.Raycast(ray, out enter))
        {
            Vector3 point = ray.GetPoint(enter);
            Vector3 dir = transform.position - point;
            float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(90f, 0f, angle + 90f), rotSpeed * Time.fixedDeltaTime);
        }

     
    }


    void PickupCheck(bool pickup, float radius, LayerMask layer)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, layer);
        if (hits.Length > 0)
        {
            //pickupText.gameObject.SetActive(true);
            Collider hit = hits.First();
            Vector3 screenPos = Camera.main.WorldToScreenPoint(hit.transform.position);
            //pickupText.position = screenPos;

            if (pickup)
            {
                GameItem item = hit.gameObject.GetComponent<PickupableObject>().item;
                inventory.AddToInventory(item);
                //hurricanePickups.Remove(hit.gameObject);
                Destroy(hit.gameObject);
            }
        }
        else
        {
            //pickupText.gameObject.SetActive(false);
        }
    }

    /*
    void NPCCheck(bool talk, float distance, LayerMask layer)
    {
        RaycastHit npc;
        if (talk && Physics.Raycast(transform.position, transform.up, out npc, distance, layer))
        {
     
            Vector3 screenPos = Camera.main.WorldToScreenPoint(npc.transform.position);

            if (talk)
            {
                DialogueTrigger dialogue = npc.collider.gameObject.GetComponent<DialogueTrigger>();
                dialogue.triggerDialogue();

            }
        }
       
    }
    */











}
