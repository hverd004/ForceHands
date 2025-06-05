using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RagDolling : MonoBehaviour
{

    private Rigidbody[] ragdollRigidbody;
    //private Collider[] ragdollCollider;

    private bool isRagdollActive = false;
    void Start()
    {
        // Get all Rigidbody and Collider components from the ragdoll
        ragdollRigidbody = GetComponentsInChildren<Rigidbody>();
        //ragdollCollider = GetComponentsInChildren<Collider>();

        // Initially, set all ragdoll parts to non-kinematic (for ragdoll to work)
        SetRagdollState(isRagdollActive);
    }

    // Toggle ragdoll on and off
    public void ToggleRagDoll()
    {
        Debug.Log("firing");
        isRagdollActive = !isRagdollActive;
        SetRagdollState(isRagdollActive);
    }

    // Enable/Disable ragdoll
    private void SetRagdollState(bool state)
    {
        foreach (Rigidbody rb in ragdollRigidbody)
        {
            rb.isKinematic = !state; // If ragdoll is active, make Rigidbody non-kinematic
        }
    }
}
