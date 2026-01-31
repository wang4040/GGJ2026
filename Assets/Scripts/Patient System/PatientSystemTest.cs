using UnityEngine;
using PatientSystem;

public class PatientSystemTest : MonoBehaviour
{
    void Start()
    {
        // Reset any existing patients
        Patient.Reset();
        PatientEvents.ClearAll();

        // Subscribe to events
        PatientEvents.OnLevelChanged += (id, oldLvl, newLvl) =>
            Debug.Log($"Patient {id}: {oldLvl} -> {newLvl}");
        
        PatientEvents.OnExploded += (id) =>
            Debug.Log($"Patient {id} EXPLODED!");

        // Create test patients
        Patient child = new Patient(PatientType.Child, Level.Normal);
        Patient old = new Patient(PatientType.Old, Level.Slight);

        Debug.Log($"Created: {child}");
        Debug.Log($"Created: {old}");

        // Test infection progression
        child.IncreaseLevel();  // Normal -> Slight
        child.IncreaseLevel();  // Slight -> Medium
        child.IncreaseLevel();  // Medium -> Severe

        // Test mask
        child.ApplyMask();
        Debug.Log($"Infection chance with mask: {child.GetInfectionChance(0.3f)}");

        // Test recovery
        old.DecreaseLevel();  // Slight -> Normal
        Debug.Log($"Old patient recovered: {old}");
    }
}