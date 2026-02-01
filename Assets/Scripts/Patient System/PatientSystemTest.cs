using UnityEngine;
using PatientSystem;

public class PatientSystemTest : MonoBehaviour
{
    void Start()
    {
        // Subscribe to events for logging
        PatientEvents.OnLevelChanged += OnPatientLevelChanged;
        PatientEvents.OnExploded += OnPatientExploded;

        // Log all patients in scene
        PatientBehaviour[] patients = FindObjectsOfType<PatientBehaviour>();
        Debug.Log($"[TEST] Found {patients.Length} patients - time is flowing");
        foreach (var p in patients)
        {
            Debug.Log($"[TEST] {p.Data}");
        }
    }

    void OnPatientLevelChanged(int id, Level oldLevel, Level newLevel)
    {
        string direction = newLevel > oldLevel ? "WORSE" : "BETTER";
        Debug.Log($"[TEST] Patient {id}: {oldLevel} -> {newLevel} ({direction})");
    }

    void OnPatientExploded(int id)
    {
        Debug.Log($"[TEST] Patient {id} EXPLODED!");
    }

    void OnDestroy()
    {
        PatientEvents.OnLevelChanged -= OnPatientLevelChanged;
        PatientEvents.OnExploded -= OnPatientExploded;
    }
}