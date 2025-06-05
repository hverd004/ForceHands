using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enableRagdolling : MonoBehaviour
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
            this.gameObject.GetComponent<RagDolling>().ToggleRagDoll();
            fired = true;
        }
    }
}
