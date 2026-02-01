using PatientSystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PatientBehaviour : MonoBehaviour
{
    // Reference to the data class
    public Patient Data { get; private set; }

    // Optional: set initial level in Inspector
    [SerializeField]
    private Level initialLevel = Level.Normal;

    [SerializeField]
    private PatientWander wander;

    // Timer for infection ticks
    private float tickTimer;

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
        wander = GetComponent<PatientWander>();
        if (wander)
        {
            wander.enabled = Random.value < GameManager.Instance.WanderPatientProportion ? true : false;
        }
    }

    void OnDestroy()
    {
        // Remove from registry when GameObject is destroyed
        if (Data != null)
        {
            Patient.Remove(Data.Id);
        }
    }

    void Start()
    {
        // Initialize timer with patient's timer duration
        tickTimer = Data.TimerDuration;
    }

    void Update()
    {
        if (Data == null || Data.Level == Level.Exploded || Data.IsInIncinerator)
            return;

        // Countdown timer
        tickTimer -= Time.deltaTime;

        if (tickTimer <= 0f)
        {
            // Reset timer
            tickTimer = Data.TimerDuration;

            // Process infection tick - this will fire PatientEvents if level changes
            float roomInfectionRate = GetRoomInfectionRate(Data.Room);
            Data.ProcessInfectionTick(roomInfectionRate);
        }
    }

    float GetRoomInfectionRate(Room room)
    {
        switch (room)
        {
            case Room.Hall:
                return GameManager.Instance != null ? GameManager.Instance.HallInfectionRate : 0.3f;
            case Room.Isolation:
                return IsolationRoomManager.Instance != null
                    ? IsolationRoomManager.Instance.InfectionRate
                    : 0.1f;
            case Room.Incinerator:
                return 0f;
            default:
                return GameManager.Instance != null ? GameManager.Instance.HallInfectionRate : 0.3f;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Data == null)
            return;

        // Display patient info above the patient
        Vector3 labelPos = transform.position + Vector3.up * 1.5f;

        string info =
            $"ID: {Data.Id}\n"
            + $"Level: {Data.Level}\n"
            + $"Mask: {Data.HasMask}\n"
            + $"Dragging: {Data.IsDragging}\n"
            + $"Room: {Data.Room}";

        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 10;

        // Color based on infection level
        switch (Data.Level)
        {
            case Level.Normal:
                style.normal.textColor = Color.green;
                Gizmos.color = Color.green;
                break;
            case Level.Slight:
                style.normal.textColor = Color.yellow;
                Gizmos.color = Color.yellow;
                break;
            case Level.Medium:
                style.normal.textColor = new Color(1f, 0.5f, 0f); // Orange
                Gizmos.color = new Color(1f, 0.5f, 0f);
                break;
            case Level.Severe:
                style.normal.textColor = Color.red;
                Gizmos.color = Color.red;
                break;
            case Level.Crazy:
                style.normal.textColor = Color.magenta;
                Gizmos.color = Color.magenta;
                break;
            case Level.Exploded:
                style.normal.textColor = Color.black;
                Gizmos.color = Color.black;
                break;
            default:
                style.normal.textColor = Color.white;
                Gizmos.color = Color.white;
                break;
        }

        Handles.Label(labelPos, info, style);

        // Draw a sphere to indicate status
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        // Draw a larger sphere if has mask
        if (Data.HasMask)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.4f);
        }
    }
#endif
}
