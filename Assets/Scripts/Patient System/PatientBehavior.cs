using UnityEngine;
using PatientSystem;

public class PatientBehaviour : MonoBehaviour
{
    // Reference to the data class
    public Patient Data { get; private set; }
    
    // Optional: set initial level in Inspector
    [SerializeField] private Level initialLevel = Level.Normal;

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
                return IsolationRoomManager.Instance != null ? IsolationRoomManager.Instance.InfectionRate : 0.1f;
            case Room.Incinerator:
                return 0f;
            default:
                return GameManager.Instance != null ? GameManager.Instance.HallInfectionRate : 0.3f;
        }
    }
}
