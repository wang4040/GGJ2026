using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PatientSystem;

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

    [Tooltip("Layers that the raycast will hit. Use this to filter out UI, ground, etc.")]
    public LayerMask layerMask = ~0;

    [Tooltip("Maximum distance for the raycast.")]
    public float maxDistance = 100f;

    [Tooltip("Number of clicks before mask is generated")]
    public int clicksPerMask = 10;

    [Tooltip("Number of masks per generation")]
    public int masksPerSuccess = 10;
    public int startingMasks = 5;

    public TextMeshProUGUI maskCountText;
    public Transform fillImage;

    private Canvas parentCanvas;
    private GameObject currentInstance;
    private bool isDragging;

    private int maskCount = 0;
    private int maskClickCount = 0;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        parentCanvas = GetComponentInParent<Canvas>();
        maskCount = startingMasks;
        maskCountText.text = "x" + maskCount.ToString();
    }

    void Update()
    {
        // While dragging and left mouse held, move the instance to the mouse position
        if (isDragging && currentInstance != null)
        {
            if (Input.GetMouseButton(0))
            {
                UpdateInstancePosition();
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

        if (prefab == null || maskCount <= 0)
            return;

        if (targetCamera == null)
            targetCamera = Camera.main;

        // Instantiate prefab and begin tracking
        currentInstance = Instantiate(prefab);
        // Parent to canvas if available so it becomes a UI element
        if (parentCanvas != null)
            currentInstance.transform.SetParent(parentCanvas.transform, false);
        else
            currentInstance.transform.SetParent(null, true);

        isDragging = true;

        // Immediately position under mouse
        UpdateInstancePosition();
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

        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green, 1f);

        // Perform raycast to see what we released over
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, layerMask))
        {
            if (hitInfo.collider.gameObject.GetComponent<PatientBehaviour>() != null)
            {
                // Apply mask to patient
                PatientBehaviour patient = hitInfo.collider.gameObject.GetComponent<PatientBehaviour>();
                SoundSys.PlaySound(patient.Data.Type == PatientType.Child ? "wear_mask_child" : "wear_mask_old");
                patient.Data.ApplyMask();
                maskCount--;
                maskCountText.text = "x" + maskCount.ToString();
            }
        }

        if (destroyOnRelease && currentInstance != null)
        {
            Destroy(currentInstance);
        }
        currentInstance = null;

        if (maskCount <= 0)
        {
            GetComponent<Image>().color = Color.gray; // Indicate no masks left;
        }
    }

    // Positions the currentInstance under the mouse as a UI element when possible.
    private void UpdateInstancePosition()
    {
        if (currentInstance == null)
            return;

        var rectTransform = currentInstance.GetComponent<RectTransform>();

        // If we have a Canvas and the instantiated object is a UI element (RectTransform)
        if (parentCanvas != null && rectTransform != null && parentCanvas.renderMode != RenderMode.WorldSpace)
        {
            // Use RectTransformUtility to convert screen point to canvas-local point
            RectTransform canvasRect = parentCanvas.transform as RectTransform;
            Camera canvasCamera = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, canvasCamera, out Vector2 localPoint);
            rectTransform.anchoredPosition = localPoint;
        }
        else if (parentCanvas != null && rectTransform != null && parentCanvas.renderMode == RenderMode.WorldSpace)
        {
            // For world-space canvas, convert screen to world and place object at that world position
            Vector3 screenPos = Input.mousePosition;
            Vector3 worldPos = (targetCamera != null)
                ? targetCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distanceFromCamera))
                : new Vector3(screenPos.x, screenPos.y, distanceFromCamera);
            currentInstance.transform.position = worldPos;
        }
        else
        {
            // Fallback for non-UI prefab: place in world using camera
            Vector3 screenPos = Input.mousePosition;
            Vector3 worldPos = (targetCamera != null)
                ? targetCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distanceFromCamera))
                : new Vector3(screenPos.x, screenPos.y, distanceFromCamera);
            currentInstance.transform.position = worldPos;
        }
    }

    public void AddMasks(int count)
    {
        maskCount += count;
        maskCountText.text = "x" + maskCount.ToString();
        if (maskCount > 0)
        {
            GetComponent<Image>().color = Color.white; // Indicate masks are available
        }
    }

    public void MaskClicked()
    {
        maskClickCount++;
        if (maskClickCount == clicksPerMask)
        {
            maskClickCount = 0;
            AddMasks(masksPerSuccess);

        }
        if (fillImage != null)
        {
            Vector3 newScale = new Vector3(fillImage.localScale.x, 1 + 2 * (float) maskClickCount / (float) clicksPerMask, fillImage.localScale.z);
            fillImage.localScale = newScale;
        }
    }
}
