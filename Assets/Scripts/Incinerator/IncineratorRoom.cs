using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class IncineratorRoom : MonoBehaviour
{
    public static int Capacity = 1;
    public static int TotalBuildingClicks = 20;
    public static float SingleIncineratorDuration = 2f;

    [Header("Instance State")]
    public int Index = 0;
    public bool IsActive = true;
    public int Count_IncineratedPatients = 0;
    public int Count_BuildingClicked = 0; // may not be used
    public bool IsIncinerating = false;

    void Start()
    {
        
    }

    void Update() { }

    public bool StartIncinerate(PatientBehaviour patientB)
    {
        if (patientB != null)
        {
            Count_IncineratedPatients++;
            Destroy(patientB.gameObject);
            StartCoroutine(Incinerate());
            return true;
        }
        Debug.LogWarning(
            $"Patient {patientB.Data.Id} cannot be incinerated in this incinerator room."
        );
        return false;
    }

    private IEnumerator Incinerate()
    {
        // start incineration process
        IsIncinerating = true;
        yield return new WaitForSeconds(SingleIncineratorDuration);

        // incineration complete
        IsIncinerating = false;
        IncineratorRoomManager.Instance.RegisterSucessfulIncineration();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsActive)
            return;
        if (other.CompareTag("Child") || other.CompareTag("Old"))
        {
            if (IsIncinerating)
            {
                Debug.LogWarning("Incinerator is currently busy.");
                return;
            }
            PatientBehaviour patientB = other.GetComponent<PatientBehaviour>();
            if (patientB != null)
            {
                StartIncinerate(patientB);
            }
            else
            {
                Debug.LogWarning("PatientBehaviour component not found on the colliding object.");
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Display info above the incinerator
        Vector3 labelPos = transform.position + Vector3.up * 2f;

        string info =
            $"Incinerator #{Index}\n"
            + $"Active: {IsActive}\n"
            + $"Number of Incinerated: {Count_IncineratedPatients}\n"
            // + $"Building: {Count_BuildingClicked}/{TotalBuildingClicks}\n"
            + $"Is Incinerating: {IsIncinerating}";

        GUIStyle style = new GUIStyle();
        style.normal.textColor = IsIncinerating ? Color.red : (IsActive ? Color.green : Color.gray);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 12;

        Handles.Label(labelPos, info, style);

        // Draw a sphere to indicate status
        Gizmos.color = IsIncinerating ? Color.red : (IsActive ? Color.green : Color.gray);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
#endif
}
