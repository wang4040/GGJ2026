using System.Collections;
using UnityEngine;
using PatientSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class IsolationRoom : MonoBehaviour
{
    public static int Capacity = 1;
    public static int TotalBuildingClicks = 20;
    public static float SingleIsolationDuration = 2f;

    [Header("Instance State")]
    public int Index = 0;
    public bool IsActive = true;
    public int Count_BuildingClicked = 0; // may not be used
    public bool IsIsolating = false;
    public Patient CurrentPatient = null;

    void Start()
    {
        if (IsActive)
        {
            Count_BuildingClicked = TotalBuildingClicks;
        }
        else
        {
            Count_BuildingClicked = 0;
        }
    }

    void Update() { }

    public bool StartIsolate(PatientBehaviour patientB)
    {
        if (patientB != null)
        {
            patientB.Data.InIsolation();
            IsIsolating = true;
            return true;
        }
        Debug.LogWarning("PatientBehaviour component not found on the colliding object.");
        return false;
    }

    private bool EndIsolate(PatientBehaviour patientB)
    {
        if (patientB != null)
        {
            patientB.Data.OutIsolation();
            Debug.Log($"Patient exited isolation state for room {this.Index}.");

            IsIsolating = false;
            return true;
        }
        Debug.LogWarning("PatientBehaviour component not found on the colliding object.");
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsActive)
            return;
        if (other.CompareTag("Child") || other.CompareTag("Old"))
        {
            if (IsIsolating || CurrentPatient != null)
            {
                Debug.LogWarning("Isolation room is currently busy.");
                return;
            }

            PatientBehaviour patientB = other.GetComponent<PatientBehaviour>();
            if (StartIsolate(patientB))
            {
                CurrentPatient = patientB.Data;
                IsolationRoomDetector detector = other.GetComponent<IsolationRoomDetector>();
                detector?.DetectedIsolationRooms.Add(this);
                // Debug.Log(
                //     $"Patient {patientB.Data.Id} started isolation in Isolation Room #{Index}."
                // );
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsActive)
            return;
        if (other.CompareTag("Child") || other.CompareTag("Old"))
        {
            PatientBehaviour patientB = other.GetComponent<PatientBehaviour>();
            if (patientB == null)
                return;

            // If isolating but CurrentPatient was cleared (e.g., spurious OnTriggerExit on mouse release), restore it
            if (IsIsolating && CurrentPatient == null)
            {
                CurrentPatient = patientB.Data;
            }

            // Ensure patient remains in isolation state while physically in the room
            if (IsIsolating && CurrentPatient != null && CurrentPatient.Id == patientB.Data.Id)
            {
                if (patientB.Data.Room != Room.Isolation)
                {
                    patientB.Data.InIsolation();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsActive)
            return;
        if (other.CompareTag("Child") || other.CompareTag("Old"))
        {
            if (
                !IsIsolating
                || (
                    CurrentPatient != null
                    && (other.GetComponent<PatientBehaviour>().Data.Id != CurrentPatient.Id)
                )
            )
            {
                return;
            }
            PatientBehaviour patientB = other.GetComponent<PatientBehaviour>();
            IsolationRoomDetector detector = other.GetComponent<IsolationRoomDetector>();
            if (detector != null && detector.DetectedIsolationRooms != null && patientB != null)
            {
                //EndIsolate(patientB);
                detector.DetectedIsolationRooms.Remove(this);
                if (detector.DetectedIsolationRooms.Count <= 1 && Input.GetMouseButton(0))
                {
                    EndIsolate(patientB);
                }
                CurrentPatient = null;
            }
        }
    }

    public void ClickBuilding()
    {
        if (IsActive)
            return;

        Count_BuildingClicked++;
        IsolationRoomManager.Instance.OnIsolationRoomBuilding(Index, Count_BuildingClicked / (float)TotalBuildingClicks);
        if (Count_BuildingClicked >= TotalBuildingClicks)
        {
            IsActive = true;
            //Debug.Log($"Isolation Room #{Index} has been activated.");
        }
    }

    public float GetBuildingRate()
    {
        return (float)Count_BuildingClicked / (float)TotalBuildingClicks;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Display info above the isolation room
        Vector3 labelPos = transform.position + Vector3.up * 2f;

        string info =
            $"Isolation Room #{Index}\n"
            + $"Active: {IsActive}\n"
            // + $"Building: {Count_BuildingClicked}/{TotalBuildingClicks}\n"
            + $"Current Patient: {(CurrentPatient != null ? CurrentPatient.Id.ToString() : "None")}\n"
            + $"Is Isolating: {IsIsolating}\n";

        GUIStyle style = new GUIStyle();
        style.normal.textColor = IsIsolating ? Color.yellow : (IsActive ? Color.blue : Color.gray);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 12;

        Handles.Label(labelPos, info, style);

        // Draw a sphere to indicate status
        Gizmos.color = IsIsolating ? Color.yellow : (IsActive ? Color.blue : Color.gray);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
#endif
}
