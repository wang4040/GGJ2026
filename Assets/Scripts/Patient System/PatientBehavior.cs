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

    // Animator reference
    private Animator animator;

    // Timer for infection ticks
    private float tickTimer;

    // Grace period timer for Crazy patients
    private float gracePeriodTimer;
    private bool isInGracePeriod = false;
    

    public Room CurrentRoom;
    

    // Track last level for animator updates
    private Level lastAnimatorLevel;

    // Mask gameobject reference and state tracking
    private GameObject maskObject;
    private Animator maskAnimator;
    private bool lastHasMask = false;

    void Awake()
    {
        lastAnimatorLevel = initialLevel;

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

        // Find and setup mask FIRST (disable it before getting main animator)
        Transform maskTransform = transform.Find("mask");
        if (maskTransform != null)
        {
            maskObject = maskTransform.gameObject;
            maskAnimator = maskTransform.GetComponent<Animator>();
            maskObject.SetActive(false); // Initialize mask as inactive
        }

        // Get main animator reference AFTER disabling mask (so it doesn't pick up mask animator)
        animator = GetComponentInChildren<Animator>();

        // Create the patient data
        Data = new Patient(type, initialLevel);
        //wander = GetComponent<PatientWander>();
        //if (wander != null)
        //{
        //    wander.StartIdling();
        //    wander.enabled = Random.value < GameManager.Instance.WanderPatientProportion ? true : false;
        //}

        // Initialize timer with patient's timer duration
        tickTimer = Data.TimerDuration;
    }

    void OnDestroy()
    {
        // Remove from registry when GameObject is destroyed
        if (Data != null)
        {
            Patient.Remove(Data.Id);
        }
    }

    void Start() { }

    void Update()
    {
        if (Data == null)
            return;
        CurrentRoom = Data.Room;
        // Update animator level parameter when level changes (including Exploded for explosion animation)
        if (
            animator != null /*&& Data.Level != lastAnimatorLevel*/
        )
        {
            animator.SetInteger("level", (int)Data.Level);
            lastAnimatorLevel = Data.Level;
        }

        // Update mask visibility when HasMask changes
        if (maskObject != null && Data.HasMask != lastHasMask)
        {
            maskObject.SetActive(Data.HasMask);
            lastHasMask = Data.HasMask;

            // Immediately sync mask animator when mask becomes active
            if (Data.HasMask && maskAnimator != null && animator != null)
            {
                maskAnimator.SetBool("isWalking", animator.GetBool("isWalking"));
            }
        }

        // Continuously sync mask animator isWalking with main animator
        if (maskAnimator != null && animator != null && Data.HasMask)
        {
            maskAnimator.SetBool("isWalking", animator.GetBool("isWalking"));
        }

        // Countdown mask timer and remove mask when expired
        if (Data.HasMask)
        {
            Data.MaskTimer -= Time.deltaTime;
            if (Data.MaskTimer <= 0f)
            {
                Data.RemoveMask();
            }
        }

        // Stop processing if exploded or incinerated
        if (Data.Level == Level.Exploded || Data.IsInIncinerator)
            return;

        // Handle grace period for Crazy patients
        if (Data.Level == Level.Crazy)
        {
            if (!isInGracePeriod)
            {
                // Start grace period when becoming Crazy
                isInGracePeriod = true;
                gracePeriodTimer = Data.GracePeriod;
                Debug.Log(
                    $"[GRACE] Patient {Data.Id} entered Crazy state, grace period started ({Data.GracePeriod}s)"
                );
            }

            // Countdown grace period
            gracePeriodTimer -= Time.deltaTime;

            if (gracePeriodTimer <= 0f)
            {
                // Grace period over - explode!
                Debug.Log($"[GRACE] Patient {Data.Id} grace period ended - EXPLODING!");
                if (GetComponentInChildren<Animator>() != null)
                {
                    SoundSys.PlaySound("explode");
                    GetComponentInChildren<Animator>().Play("Explode");
                }
                if (GetComponent<PatientWander>() != null)
                {
                    GetComponent<PatientWander>().stop = true;
                }
                Data.Level = Level.Exploded;
                return;
            }
        }
        else
        {
            // Reset grace period state if not Crazy
            isInGracePeriod = false;
        }

        // Countdown infection timer
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
