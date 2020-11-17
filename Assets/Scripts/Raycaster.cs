using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum Physicscasttype
{
    Raycast,
    Spherecast
    //Boxcast
}

[System.Serializable]
public class UnityEventRaycastDirection : UnityEvent<RayTransform> { }
[System.Serializable]
public class UnityEventPosition : UnityEvent<Vector3, Quaternion> { }

public class RayTransform
{
    public Vector3 rayPosition;
    public Vector3 rayDirection;
    public float rayDistance;

    public RayTransform() { }
    public RayTransform(Vector3 pos, Vector3 dir, float dist)
    {
        rayPosition = pos;
        rayDirection = dir;
        rayDistance = dist;
    }
}

public class Raycaster : MonoBehaviour
{
    [Header("Raycast Stats")]
    [SerializeField] private LayerMask raycastLayers;
    [SerializeField] private float rayDistance=100f;
    [SerializeField] [Range(-1,1000)] private int maxRayBounces=-1;
    [SerializeField] private float rayBounceDelay = 0.1f;

    [Header("Visuals")]
    [SerializeField] private TrailRenderer raycastTrail;
    [SerializeField] private LineRenderer raycastLine;
    [SerializeField] private float trailLifetime = 0.1f;

    [Header("Events")]
    [SerializeField] private UnityEventPosition onTrailReachingTarget;
    [SerializeField] private UnityEventPosition onRayHittingTarget;

    public bool startAutomatically = true;

    private RayTransform gizmoRayTransform;

    private void Start()
    {
        if (startAutomatically)
            StartRaycastloop();
    }

    public void StartRaycastloop(RayTransform rayTransform)
    {
        if (rayTransform != null)
            Raycast(rayTransform);
        else
            StartRaycastloop();
    }
    public void StartRaycastloop()
    {
        Raycast(new RayTransform(transform.position, transform.forward, rayDistance));
    }

    void Raycast(RayTransform rayTransform)
    {
        if (raycastLine!=null)
        {
            raycastLine.useWorldSpace = true;
        }

        StartCoroutine(RaycastLoop(rayTransform));
    }

    IEnumerator RaycastLoop(RayTransform rayTransform)
    {
        int bounces = 0;
        while (bounces < maxRayBounces || maxRayBounces == -1)
        {
            rayTransform = ShootRay(rayTransform);

            yield return new WaitForSeconds(0.1f);
        }
    }

    RayTransform ShootRay(RayTransform currentRayTransform)
    {
        RaycastHit rayHit;

        // Ray hit
        if (Physics.Raycast(currentRayTransform.rayPosition, currentRayTransform.rayDirection, out rayHit, currentRayTransform.rayDistance, raycastLayers, QueryTriggerInteraction.Ignore))
        { 
            Debug.DrawRay(currentRayTransform.rayPosition, currentRayTransform.rayDirection * rayHit.distance, Color.red);

            if (onRayHittingTarget != null)
                onRayHittingTarget.Invoke(rayHit.point, Quaternion.LookRotation(rayHit.normal));

            //rayHit.collider.gameObject.GetComponent...

            //ray visuals
            StartCoroutine(MoveTrailAlongTheRay(currentRayTransform.rayPosition, rayHit.point, rayHit.normal));
            SetLineRendererPoints(currentRayTransform.rayPosition, rayHit.point);

            // current ray transform for gizmo
            gizmoRayTransform = new RayTransform(currentRayTransform.rayPosition, currentRayTransform.rayDirection, rayHit.distance);

            // next ray transform
            return new RayTransform(rayHit.point, Vector3.Reflect(currentRayTransform.rayDirection, rayHit.normal).normalized, currentRayTransform.rayDistance);
        }
        else // Ray didn't hit
        {
            // current ray transform for gizmo
            gizmoRayTransform = new RayTransform(currentRayTransform.rayPosition, currentRayTransform.rayDirection, rayHit.distance);

            // next ray transform
            return new RayTransform(currentRayTransform.rayPosition + currentRayTransform.rayDirection * currentRayTransform.rayDistance, currentRayTransform.rayDirection, currentRayTransform.rayDistance);
        }
    }

    IEnumerator MoveTrailAlongTheRay(Vector3 startPosition, Vector3 toPosition, Vector3? normalDirection)
    {
        if (raycastTrail == null)
            yield break;

        float timer = 0f;

        while (timer < rayBounceDelay) 
        {
            raycastTrail.transform.position = Vector3.Lerp(startPosition, toPosition, timer / rayBounceDelay);

            timer += Time.deltaTime;
            yield return null;
        }
        raycastTrail.transform.position = Vector3.Lerp(startPosition, toPosition, 0.5f);

        if (normalDirection != null && onTrailReachingTarget != null)
        {
            onTrailReachingTarget.Invoke(toPosition, Quaternion.LookRotation((Vector3)normalDirection));
        }
    }

    void SetLineRendererPoints(Vector3 startPosition, Vector3 toPosition)
    {
        if (raycastLine == null)
            return;

        raycastLine.SetPosition(0, startPosition);
        raycastLine.SetPosition(1, toPosition);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false || gizmoRayTransform == null)
            return;

        // create transform matrix out of the current ray's position, direction & scale
        Gizmos.matrix = Matrix4x4.TRS(gizmoRayTransform.rayPosition, Quaternion.LookRotation(gizmoRayTransform.rayDirection), Vector3.one);
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(Vector3.zero, 1f);

        //switch (PhysicscastType)
        //{
        //    case Physicscasttype.Raycast:
        //        Gizmos.DrawLine(Vector3.zero, (Vector3.forward * gizmoRayTransform.rayDistance * 1f));
        //        break;
        //    case Physicscasttype.Spherecast:
        //        Gizmos.DrawSphere(Vector3.zero, 1f);
        //        break;
        //}


    }
}
