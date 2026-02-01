using System.Collections.Generic;
using UnityEngine;
using PatientSystem;
using TMPro;

public class PatientsTrakerOnUI : MonoBehaviour
{
    [Header("References")]
    public Camera targetCamera;
    public Canvas canvas;
    public RectTransform canvasRect;
    public GameObject trackerUIPrefab;

    [Header("Settings")]
    public Vector2 offset = Vector2.zero;
    public bool hideOffScreen = true;

    private bool isTracking = false;
    public Dictionary<int, GameObject> patientUIMarkers = new Dictionary<int, GameObject>();

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (canvasRect == null)
        {
            canvasRect = GetComponent<RectTransform>();
        }

        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
        }

        // Subscribe to patient events
        PatientEvents.OnIncinerated += OnPatientRemoved;
    }

    void OnDestroy()
    {
        PatientEvents.OnIncinerated -= OnPatientRemoved;
    }

    void Update()
    {
        if (isTracking)
        {
            UpdatePatientMarkers();
        }
    }

    void UpdatePatientMarkers()
    {
        // Get all patients
        var allPatients = Patient.GetAllPatients();

        // Update or create markers for each patient
        foreach (var kvp in allPatients)
        {
            int patientId = kvp.Key;
            Patient patient = kvp.Value;

            // Find the patient's GameObject in the scene
            PatientBehaviour[] allPatientBehaviours = FindObjectsByType<PatientBehaviour>(FindObjectsSortMode.None);
            PatientBehaviour targetPatient = null;

            foreach (var pb in allPatientBehaviours)
            {
                if (pb.Data.Id == patientId)
                {
                    targetPatient = pb;
                    break;
                }
            }

            if (targetPatient == null)
                continue;

            // Create UI marker if it doesn't exist
            if (!patientUIMarkers.ContainsKey(patientId))
            {
                GameObject marker = Instantiate(trackerUIPrefab, canvasRect);
                patientUIMarkers[patientId] = marker;
            }

            // Update marker position
            UpdateMarkerPosition(patientUIMarkers[patientId], targetPatient.transform);
        }

        // Remove markers for patients that no longer exist
        List<int> toRemove = new List<int>();
        foreach (var kvp in patientUIMarkers)
        {
            if (!allPatients.ContainsKey(kvp.Key))
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (int id in toRemove)
        {
            if (patientUIMarkers.ContainsKey(id))
            {
                Destroy(patientUIMarkers[id]);
                patientUIMarkers.Remove(id);
            }
        }
    }

    void UpdateMarkerPosition(GameObject marker, Transform target)
    {
        if (marker == null || target == null)
            return;

        // Convert world position to screen position
        Vector3 screenPos = targetCamera.WorldToScreenPoint(target.position);

        // Check if object is behind camera
        if (screenPos.z < 0)
        {
            if (hideOffScreen)
            {
                marker.SetActive(false);
            }
            return;
        }

        // Convert screen position to canvas position
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCamera,
            out canvasPos
        );

        // Apply offset
        canvasPos += offset;

        // Set position
        RectTransform markerRect = marker.GetComponent<RectTransform>();
        if (markerRect != null)
        {
            markerRect.anchoredPosition = canvasPos;
        }

        // Show marker if it was hidden
        if (!marker.activeSelf)
        {
            marker.SetActive(true);
        }
    }

    void OnPatientRemoved(int patientId)
    {
        if (patientUIMarkers.ContainsKey(patientId))
        {
            Destroy(patientUIMarkers[patientId]);
            patientUIMarkers.Remove(patientId);
        }
    }

    public void StartTracking()
    {
        isTracking = true;
    }

    public void StopTracking()
    {
        isTracking = false;
    }

    public void UpdateMarkerText(int patientId, string text)
    {
        Debug.Log($"Updating marker text for patient {patientId} to '{text}'");

        // Ensure marker exists
        if (!patientUIMarkers.ContainsKey(patientId))
        {
            // Try to find the patient and create marker
            Patient patient = Patient.Get(patientId);
            if (patient != null)
            {
                GameObject marker = Instantiate(trackerUIPrefab, canvasRect);
                patientUIMarkers[patientId] = marker;
                Debug.Log($"Created new marker for patient {patientId}");
            }
            else
            {
                Debug.LogWarning($"Patient {patientId} not found, cannot create marker");
                return;
            }
        }

        if (patientUIMarkers.ContainsKey(patientId))
        {
            GameObject marker = patientUIMarkers[patientId];
            TextMeshProUGUI tmpText = marker.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = text;
                Debug.Log($"Successfully updated text for patient {patientId}");
            }
            else
            {
                Debug.LogWarning($"TextMeshProUGUI not found in marker for patient {patientId}");
            }
        }
    }

    public TextMeshProUGUI GetMarkerText(int patientId)
    {
        if (patientUIMarkers.ContainsKey(patientId))
        {
            GameObject marker = patientUIMarkers[patientId];
            return marker.GetComponentInChildren<TextMeshProUGUI>();
        }
        return null;
    }
}
