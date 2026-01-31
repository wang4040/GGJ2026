using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class IncineratorRoom : MonoBehaviour
{
    public static int Capacity = 2;
    public static int TotalBuildingClicks = 20;
    public static float SingleIncineratorDuration = 2f;

    [Header("Instance State")]
    public int Index = 0;
    public int Count_IncineratedPatients = 0;
    public int Count_BuildingClicked = 0;
    public bool isActive = true;
    public bool isIncinerating = false;

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
        isIncinerating = true;
        yield return new WaitForSeconds(SingleIncineratorDuration);

        // incineration complete
        isIncinerating = false;
        IncineratorRoomManager.Instance.RegisterSucessfulIncineration();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive)
            return;
        if (other.CompareTag("Child") || other.CompareTag("Old"))
        {
            if (isIncinerating)
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
            + $"Incinerated: {Count_IncineratedPatients}\n"
            + $"Building: {Count_BuildingClicked}/{TotalBuildingClicks}\n"
            + $"Active: {isActive}\n"
            + $"Incinerating: {isIncinerating}";

        GUIStyle style = new GUIStyle();
        style.normal.textColor = isIncinerating ? Color.red : (isActive ? Color.green : Color.gray);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 12;

        Handles.Label(labelPos, info, style);

        // Draw a sphere to indicate status
        Gizmos.color = isIncinerating ? Color.red : (isActive ? Color.green : Color.gray);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
#endif
}
