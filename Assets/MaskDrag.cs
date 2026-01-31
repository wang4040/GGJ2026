using UnityEngine;
using UnityEngine.EventSystems;

public class MaskDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("Prefab that will be instantiated and follow the mouse while mouse button 0 is held.")]
    public GameObject prefab;

    [Tooltip("Camera used to convert screen->world. If null, Camera.main is used.")]
    public Camera targetCamera;

    [Tooltip("Distance from the camera (in world units) used for ScreenToWorldPoint. For orthographic cameras, set to the desired world Z offset from the camera.")]
    public float distanceFromCamera = 10f;

    [Tooltip("If true, the instantiated object will be destroyed when the mouse button is released.")]
    public bool destroyOnRelease = false;

    private GameObject currentInstance;
    private bool isDragging;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void Update()
    {
        // While dragging and left mouse held, move the instance to the mouse position
        if (isDragging && currentInstance != null)
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 worldPos = targetCamera != null
                    ? targetCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distanceFromCamera))
                    : new Vector3(mousePos.x, mousePos.y, distanceFromCamera);
                currentInstance.transform.position = worldPos;
            }
            else
            {
                // Mouse released without pointer up event (safety)
                EndDrag();
            }
        }
    }

    // Called when pointer is pressed on this UI element (requires an EventSystem + GraphicRaycaster)
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (prefab == null)
            return;

        if (targetCamera == null)
            targetCamera = Camera.main;

        // Instantiate prefab and begin tracking
        currentInstance = Instantiate(prefab);
        isDragging = true;

        // Immediately position under mouse
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = targetCamera != null
            ? targetCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distanceFromCamera))
            : new Vector3(mousePos.x, mousePos.y, distanceFromCamera);
        currentInstance.transform.position = worldPos;
    }

    // Called when pointer is released on this UI element
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        EndDrag();
    }

    private void EndDrag()
    {
        isDragging = false;
        if (destroyOnRelease && currentInstance != null)
        {
            Destroy(currentInstance);
        }
        currentInstance = null;
    }
}
