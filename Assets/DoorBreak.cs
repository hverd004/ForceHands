using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBreak : MonoBehaviour
{
    bool fired = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!fired && other.CompareTag("Interactable"))
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
            this.transform.GetChild(1).gameObject.GetComponent<ParticleSystem>().Play();
            fired = true;
        }
    }
}
