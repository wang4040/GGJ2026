using UnityEngine;

public class MouseRay : MonoBehaviour
{
    [Tooltip("Camera used to cast the ray. If null, Camera.main is used.")]
    public Camera targetCamera;

    [Tooltip("Layers that the raycast will hit. Use this to filter out UI, ground, etc.")]
    public LayerMask layerMask = ~0;

    [Tooltip("Maximum distance for the raycast.")]
    public float maxDistance = 100f;

    [Tooltip("Distance from the camera at which the hit object will be held while dragging.")]
    public float dragDistance = 5f;

    [Tooltip("Draw the ray in the Scene view for debugging.")]
    public bool debugRay = true;

    private Transform draggedTransform;
    private Rigidbody draggedRigidbody;
    private bool draggedWasKinematic;
    private bool isDragging;
    private OutlinePatient currPatientOutline;

    // World Y value locked while dragging
    private float lockedY;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void Update()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
        if (debugRay)
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green, 1f);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, layerMask))
        {
            // Hovering over patient
            if (hitInfo.collider.gameObject.GetComponent<PatientBehaviour>() != null)
            {
                if (currPatientOutline != null && currPatientOutline != hitInfo.collider.gameObject.GetComponent<OutlinePatient>())
                {
                    // New outline target, remove old outline
                    currPatientOutline?.RemoveOutline();
                    currPatientOutline = null;
                }
                currPatientOutline = hitInfo.collider.gameObject.GetComponent<OutlinePatient>();
                currPatientOutline?.OutlineObject();
                if (Input.GetMouseButtonDown(0))
                {
                    // Start dragging this transform
                    draggedTransform = hitInfo.collider.transform;
                    draggedTransform.GetComponent<PatientBehaviour>().Data.OnDragging();

                    // Lock Y to current world Y of the object
                    lockedY = draggedTransform.position.y;

                    // If it has a Rigidbody, make it kinematic while dragging so we can move it cleanly
                    draggedRigidbody = draggedTransform.GetComponent<Rigidbody>();
                    if (draggedRigidbody != null)
                    {
                        draggedWasKinematic = draggedRigidbody.isKinematic;
                        draggedRigidbody.linearVelocity = Vector3.zero;
                        draggedRigidbody.angularVelocity = Vector3.zero;
                        draggedRigidbody.isKinematic = true;
                    }

                    // Immediately move the object to the ray point on the locked Y plane (or fallback)
                    draggedTransform.position = GetPointOnYPlane(ray, lockedY, dragDistance);

                    isDragging = true;
                }
            }
            else // Moved off a patient, remove outline
            {
                if (currPatientOutline != null)
                {
                    currPatientOutline.RemoveOutline();
                    currPatientOutline = null;
                }
            }

            //  Hovering over isolation room
            if (hitInfo.collider.gameObject.GetComponent<IsolationRoom>() != null)
            {
                if (Input.GetMouseButtonDown(0) && !isDragging)
                {
                    hitInfo.collider.gameObject.GetComponent<IsolationRoom>().ClickBuilding();
                }
            }
        }
        else // Not hovering over anything, remove outline
        {
            if (currPatientOutline != null)
            {
                currPatientOutline?.RemoveOutline();
                currPatientOutline = null;
            }
        }

        // While holding, keep moving the dragged object with the mouse
        if (isDragging && draggedTransform != null)
        {
            if (Input.GetMouseButton(0))
            {
                draggedTransform.position = GetPointOnYPlane(ray, lockedY, dragDistance);
            }
            else
            {
                // Mouse released without using GetMouseButtonUp path; end drag
                draggedTransform.GetComponent<PatientBehaviour>().Data.StopDragging();
                EndDrag();
            }
        }

        // End drag on mouse up
        if (Input.GetMouseButtonUp(0) && isDragging && draggedTransform != null)
        {
            draggedTransform.GetComponent<PatientBehaviour>()?.Data.StopDragging();
            EndDrag();
        }
    }

    // Compute intersection of ray with horizontal plane y = targetY.
    // If ray is nearly parallel or intersection is behind the origin, fall back to ray.GetPoint(fallbackDistance)
    // and enforce the locked Y on that fallback so the object keeps the same Y value.
    private Vector3 GetPointOnYPlane(Ray ray, float targetY, float fallbackDistance)
    {
        float dy = ray.direction.y;
        const float eps = 1e-6f;

        if (Mathf.Abs(dy) < eps)
        {
            Vector3 fallback = ray.GetPoint(fallbackDistance);
            fallback.y = targetY;
            return fallback;
        }

        float t = (targetY - ray.origin.y) / dy;
        if (float.IsNaN(t) || float.IsInfinity(t) || t < 0f)
        {
            Vector3 fallback = ray.GetPoint(fallbackDistance);
            fallback.y = targetY;
            return fallback;
        }

        return ray.GetPoint(t);
    }

    private void EndDrag()
    {
        if (draggedRigidbody != null)
        {
            draggedRigidbody.isKinematic = draggedWasKinematic;
            draggedRigidbody = null;
        }

        draggedTransform = null;
        isDragging = false;
    }
}
