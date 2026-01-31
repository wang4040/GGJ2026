using UnityEngine;
using PatientSystem;

public class PatientBehaviour : MonoBehaviour
{
    // Reference to the data class
    public Patient Data { get; private set; }
    
    // Optional: set initial level in Inspector
    [SerializeField] private Level initialLevel = Level.Normal;

    void Awake()
    {
        // Determine type from tag
        PatientType type;
        if (CompareTag("Child"))
        {
            type = PatientType.Child;
        }
        else if (CompareTag("Old"))
        {
            type = PatientType.Old;
        }
        else
        {
            Debug.LogWarning($"Unknown tag '{tag}' on {name}, defaulting to Old");
            type = PatientType.Old;
        }

        // Create the patient data
        Data = new Patient(type, initialLevel);
    }

    void OnDestroy()
    {
        // Remove from registry when GameObject is destroyed
        if (Data != null)
        {
            Patient.Remove(Data.Id);
        }
    }
}