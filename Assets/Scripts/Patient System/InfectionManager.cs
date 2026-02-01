using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PatientSystem;
using UnityEngine;

public class InfectionManager : MonoBehaviour
{
    [SerializeField]
    private float explosionRadius = 5f;

    void OnEnable()
    {
        PatientEvents.OnExploded += HandleExplosion;
    }

    void OnDisable()
    {
        PatientEvents.OnExploded -= HandleExplosion;
    }

    private void HandleExplosion(int explodedPatientID)
    {
        // Find the exploded patienbt's GameObject
        PatientBehaviour exploded = FindPatientBehaviourById(explodedPatientID);
        if (exploded == null)
        {
            return;
        }

        Vector3 explosionCenter = exploded.transform.position;

        // For all patients in the range increase their infection level
        PatientBehaviour[] allPatients = FindObjectsByType<PatientBehaviour>( FindObjectsSortMode.None);
        foreach (PatientBehaviour patientBehaviour in allPatients)
        {
            // Skip the exploded patient itself
            if (patientBehaviour.Data.Id == explodedPatientID)
                continue;

            // Skip all patients that are already exploded or incinerating
            if (patientBehaviour.Data.Level >= Level.Exploded)
                continue;

            // Check if within the range
            float distance = Vector3.Distance(explosionCenter, patientBehaviour.transform.position);
            if (distance <= explosionRadius)
            {
                patientBehaviour.Data.IncreaseLevel();
            }
        }
    }

    private PatientBehaviour FindPatientBehaviourById(int patientId)
    {
        PatientBehaviour[] allPatients = FindObjectsByType<PatientBehaviour>(
            FindObjectsSortMode.None
        );
        foreach (PatientBehaviour patient in allPatients)
        {
            if (patient.Data != null && patient.Data.Id == patientId)
            {
                return patient;
            }
        }
        return null;
    }
}
