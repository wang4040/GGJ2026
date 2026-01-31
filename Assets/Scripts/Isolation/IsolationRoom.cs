using System.Collections;
using UnityEngine;
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

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public bool StartIsolate(PatientBehaviour patientB)
    {
        if (patientB != null)
        {
            Destroy(patientB.gameObject);
            StartCoroutine(Isolate());
            return true;
        }
        Debug.LogWarning(
            $"Patient {patientB.Data.Id} cannot be isolated in this isolation room."
        );
        return false;
    }

    private IEnumerator Isolate()
    {
        // start isolation process
        IsIsolating = true;
        yield return new WaitForSeconds(SingleIsolationDuration);
        // isolation complete
        IsIsolating = false;
        //IsolationRoomManager.Instance.RegisterSucessfulIsolation();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsActive)
            return;
        if (other.CompareTag("Child") || other.CompareTag("Old"))
        {
            if (IsIsolating)
            {
                Debug.LogWarning("Isolation room is currently busy.");
                return;
            }
            PatientBehaviour patientB = other.GetComponent<PatientBehaviour>();
            if (patientB != null)
            {
                StartIsolate(patientB);
            }
            else
            {
                Debug.LogWarning("PatientBehaviour component not found on the colliding object.");
            }
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
            + $"Isolating: {IsIsolating}";

        GUIStyle style = new GUIStyle();
        style.normal.textColor = IsIsolating ? Color.red : (IsActive ? Color.green : Color.gray);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 12;

        Handles.Label(labelPos, info, style);

        // Draw a sphere to indicate status
        Gizmos.color = IsIsolating ? Color.red : (IsActive ? Color.green : Color.gray);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
#endif
}
