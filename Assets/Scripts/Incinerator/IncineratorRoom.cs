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

    public bool StartIncinerate(PatientBehaviour patientB)
    {
        if (patientB != null)
        {
            Count_IncineratedPatients++;
            foreach (IsolationRoom room in IsolationRoomManager.Instance.IsolationRooms)
            {
                room.GetComponent<OutlinePatient>()?.RemoveOutline();
            }
            foreach (IncineratorRoom room in IncineratorRoomManager.Instance.IncineratorRooms)
            {
                room.GetComponent<OutlinePatient>()?.RemoveOutline();
            }
            Destroy(patientB.gameObject);
            StartCoroutine(Incinerate());
            return true;
        }
        Debug.LogWarning("PatientBehaviour component not found on the colliding object.");
        return false;
    }

    private IEnumerator Incinerate()
    {
        // start incineration process
        IsIncinerating = true;
        GameObject.FindFirstObjectByType<IncineratorCD>().StartIncineratorCooldown(SingleIncineratorDuration);
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
            if (StartIncinerate(patientB))
            {
                // Debug.Log(
                //     $"Patient {patientB.Data.Id} started incineration in Incinerator Room #{Index}."
                // );
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
        style.normal.textColor = IsIncinerating ? Color.yellow : (IsActive ? Color.red : Color.gray);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 12;

        Handles.Label(labelPos, info, style);

        // Draw a sphere to indicate status
        Gizmos.color = IsIncinerating ? Color.yellow : (IsActive ? Color.red : Color.gray);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
#endif
}
