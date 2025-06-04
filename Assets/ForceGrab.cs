using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class ForceGrab : MonoBehaviour
{
    public GameObject hand;
    public GameObject detector;
    public float detectionDistance = 10f;
    public GameObject cube;
    public float followSpeed = 10f;

    public float positionForce = 500f;
    public float rotationForce = 10f;
    public float pullForce = 2f;
    public float stopDistance = 1.5f;

    private bool grabbing = false;
    private Vector3 localOffset;
    private bool crushing = false;
    private bool growing = false;
    private bool pushing = false;
    private bool pulling = false;

    void OnDrawGizmos()
    {
        if (detector == null) return;

        float radius = 0.35f;
        float halfHeight = 0.5f;
        float distance = detectionDistance;

        Vector3 direction = -detector.transform.up.normalized;
        Vector3 center = detector.transform.position;
        Vector3 offset = detector.transform.up * halfHeight;

        Vector3 p1Start = center + offset;
        Vector3 p2Start = center - offset;

        Vector3 p1End = p1Start + direction * distance;
        Vector3 p2End = p2Start + direction * distance;

        // Draw the sweep capsule
        Gizmos.color = Color.cyan;

        // Start and end sphere ends
        Gizmos.DrawWireSphere(p1Start, radius);
        Gizmos.DrawWireSphere(p2Start, radius);
        Gizmos.DrawWireSphere(p1End, radius);
        Gizmos.DrawWireSphere(p2End, radius);

        // Draw side lines connecting start and end capsule surfaces
        DrawCapsuleSweepLines(p1Start, p2Start, p1End, p2End, radius);
    }
    void DrawCapsuleSweepLines(Vector3 p1Start, Vector3 p2Start, Vector3 p1End, Vector3 p2End, float radius)
    {
        int segments = 8;
        Vector3 up = (p1Start - p2Start).normalized;
        Vector3 forward = Vector3.Slerp(up, -up, 0.5f).normalized;
        Vector3 right = Vector3.Cross(up, forward).normalized;

        for (int i = 0; i < segments; i++)
        {
            float angle = (360f / segments) * i * Mathf.Deg2Rad;
            Vector3 offset = Mathf.Cos(angle) * right + Mathf.Sin(angle) * forward;

            Vector3 startTop = p1Start + offset * radius;
            Vector3 startBottom = p2Start + offset * radius;

            Vector3 endTop = p1End + offset * radius;
            Vector3 endBottom = p2End + offset * radius;

            Gizmos.DrawLine(startTop, endTop);
            Gizmos.DrawLine(startBottom, endBottom);
        }
    }
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    void FixedUpdate()
    {
        if (cube != null)
        {
            //lerp method
            /*if (grabbing)
            {
                Vector3 targetPos = hand.transform.TransformPoint(localOffset);
                cube.GetComponent<Rigidbody>().useGravity = false;
                cube.GetComponent<Rigidbody>().MovePosition(Vector3.Lerp(cube.GetComponent<Rigidbody>().position, targetPos, Time.fixedDeltaTime * followSpeed));

                Quaternion targetRot = hand.transform.rotation;
                cube.GetComponent<Rigidbody>().MoveRotation(Quaternion.Slerp(cube.GetComponent<Rigidbody>().rotation, targetRot, Time.fixedDeltaTime * followSpeed));
            }*/
            if (grabbing)
            {
                Rigidbody rb = cube.GetComponent<Rigidbody>();
                rb.useGravity = false;

                Vector3 targetPos = hand.transform.TransformPoint(localOffset);
                Vector3 toTarget = targetPos - rb.position;

                // Position correction using force
                rb.AddForce(toTarget * positionForce * Time.fixedDeltaTime, ForceMode.Acceleration);

                // Rotation correction using torque
                Quaternion targetRot = hand.transform.rotation;
                Quaternion deltaRot = targetRot * Quaternion.Inverse(rb.rotation);

                // Convert delta rotation to axis-angle
                deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180f) angle -= 360f; // Normalize

                // Apply torque
                Vector3 torque = axis * angle * Mathf.Deg2Rad * rotationForce;
                rb.AddTorque(torque, ForceMode.Acceleration);
            }
            if (crushing)
            {
                Vector3 targetScale = new Vector3(0.05f, 0.05f, 0.05f);

                if (Vector3.Distance(cube.transform.localScale, targetScale) > 0.001f)
                {
                    cube.transform.localScale = Vector3.Lerp(cube.transform.localScale, targetScale, Time.deltaTime * 2f);
                    cube.GetComponent<Grabbable>().Transform.transform.localScale = cube.transform.localScale;
                }
            }
            else if (growing)
            {
                Vector3 targetScale = new Vector3(1f, 1f, 1f);

                if (Vector3.Distance(cube.transform.localScale, targetScale) > 0.001f)
                {
                    cube.transform.localScale = Vector3.Lerp(cube.transform.localScale, targetScale, Time.deltaTime * 2f);
                    cube.GetComponent<Grabbable>().Transform.transform.localScale = cube.transform.localScale;
                }
            }

            if (pushing)
            {
                Vector3 pushDirection = (cube.transform.position - hand.transform.position).normalized;
                cube.GetComponent<Rigidbody>().AddForce(pushDirection * pullForce, ForceMode.Impulse);
            }
            else if (pulling)
            {
                Rigidbody rb = cube.GetComponent<Rigidbody>();
                float distance = Vector3.Distance(hand.transform.position, cube.transform.position);

                if (distance > stopDistance)
                {
                    Vector3 pullDir = (hand.transform.position - cube.transform.position).normalized;
                    rb.AddForce(pullDir * pullForce, ForceMode.Impulse);
                }
                else
                {
                    // Stop the cube just before the hand by zeroing out velocity
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }

    public void findObject(bool front)
    {
        float radius = 0.35f;
        float halfHeight = 0.5f;
        float distance = detectionDistance;

        Vector3 direction = front ? -detector.transform.up.normalized : detector.transform.up.normalized;

        Vector3 center = detector.transform.position;
        Vector3 offset = detector.transform.up * halfHeight;

        Vector3 p1Start = center + offset;
        Vector3 p2Start = center - offset;

        int samples = 10; // Adjust for accuracy/performance

        GameObject closestObject = null;
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;

            Vector3 p1 = Vector3.Lerp(p1Start, p1Start + direction * distance, t);
            Vector3 p2 = Vector3.Lerp(p2Start, p2Start + direction * distance, t);

            Collider[] hits = Physics.OverlapCapsule(p1, p2, radius);

            foreach (Collider col in hits)
            {
                if (col.CompareTag("Interactable") && col.attachedRigidbody != null)
                {
                    float dist = Vector3.Distance(center, col.transform.position);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        closestObject = col.gameObject;
                    }
                }
            }
        }

        if (closestObject != null)
        {
            cube = closestObject;
        }

        if (cube != null)
        {
            Debug.Log("Found object inside capsule sweep volume: " + cube.name);
        }
    }
    public void grab()
    {
        Debug.Log("Grabbed");
        findObject(true);
        grabbing = true;
        localOffset = hand.transform.InverseTransformPoint(cube.transform.position);
    }

    public void grabRelease()
    {
        Debug.Log("Released Grab");
        grabbing = false;
        cube.GetComponent<Rigidbody>().useGravity = true;
    }

    public void crush()
    {
        Debug.Log("Crushing");
        findObject(true);
        crushing = true;
    }
    public void crushRelease()
    {
        Debug.Log("Released Crush");
        crushing = false;
    }

    public void grow()
    {
        Debug.Log("Growing");
        findObject(true);
        growing = true;
    }
    public void growRelease()
    {
        Debug.Log("Released Grow");
        growing = false;
    }

    public void push()
    {
        Debug.Log("Pushing");
        findObject(true);
        pushing = true;
    }

    public void pushRelease()
    {
        Debug.Log("Released Push");
        pushing = false;
    }
    public void pull()
    {
        Debug.Log("Pulling");
        findObject(false);
        pulling = true;
    }

    public void pullRelease()
    {
        Debug.Log("Released Pull");
        pulling = false;
    }
}
