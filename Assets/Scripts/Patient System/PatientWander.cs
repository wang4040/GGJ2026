using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using PatientSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PatientWander : MonoBehaviour
{

    [Header("Boundary Settings")]
    public Vector2 boundaryCenter = Vector2.zero;
    public Vector2 boundarySize = new Vector2(50f, 50f);


    [Header("Timing")]
    public float minWanderTime = 2f;
    public float maxWanderTime = 5f;
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float crazyMoveSpeed = 4f;

    [Header("Bite Settings")]
    public float biteRadius = 1f;

    private Vector3 targetPosition;
    private float stateTimer;
    private bool isWandering;

    // Chase mode for Crazy patients
    private PatientBehaviour patientBehaviour;
    private PatientBehaviour chaseTarget;
    private bool isChasing = false;

    // Animator reference
    private Animator animator;

    // Bark state tracking (Crazy patients bark before crawling)
    private bool hasBarked = false;
    private bool isBarking = false;
    public float barkDuration = 1f;
    private float barkTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        patientBehaviour = GetComponent<PatientBehaviour>();
        animator = GetComponent<Animator>();
        StartWandering();
    }

    // Update is called once per frame
    void Update()
    {
        if (patientBehaviour == null || patientBehaviour.Data == null)
            return;

        // Check if patient is Crazy - switch to chase mode
        if (patientBehaviour.Data.CanBite)
        {
            UpdateChaseMode();
            return;
        }

        // Normal wandering behavior - reset chase state
        isChasing = false;
        chaseTarget = null;
        hasBarked = false;
        isBarking = false;

        stateTimer -= Time.deltaTime;

        if (isWandering)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f || stateTimer <= 0)
            {
                StartIdling();
            }
        }
        else
        {
            if (stateTimer <= 0)
            {
                StartWandering();
            }
        }
    }

    void UpdateChaseMode()
    {
        // Handle bark animation before crawling (only once when becoming Crazy)
        if (!hasBarked)
        {
            if (!isBarking)
            {
                // Start barking
                isBarking = true;
                barkTimer = barkDuration;
                if (animator != null)
                {
                    animator.SetBool("isBark", true);
                    animator.SetBool("isWalking", false);
                }
                Debug.Log($"[BARK] Crazy patient {patientBehaviour.Data.Id} starts barking!");
            }

            // Wait for bark to finish
            barkTimer -= Time.deltaTime;
            if (barkTimer <= 0f)
            {
                hasBarked = true;
                isBarking = false;
                if (animator != null)
                    animator.SetBool("isBark", false);
                Debug.Log($"[BARK] Crazy patient {patientBehaviour.Data.Id} finished barking, now crawling!");
            }
            return; // Don't move while barking
        }

        // Find a target if we don't have one
        if (chaseTarget == null || !chaseTarget.Data.CanBeBitten)
        {
            FindBiteTarget();
        }

        // If we still don't have a target, wander randomly
        if (chaseTarget == null)
        {
            isChasing = false;
            stateTimer -= Time.deltaTime;
            if (isWandering)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, crazyMoveSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f || stateTimer <= 0)
                {
                    StartIdling();
                }
            }
            else if (stateTimer <= 0)
            {
                StartWandering();
            }
            return;
        }

        // Chase the target
        isChasing = true;
        if (animator != null)
            animator.SetBool("isWalking", true);
        Vector3 targetPos = chaseTarget.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, crazyMoveSpeed * Time.deltaTime);

        // Check if close enough to bite
        float distance = Vector3.Distance(transform.position, targetPos);
        if (distance <= biteRadius)
        {
            PerformBite();
        }
    }

    void FindBiteTarget()
    {
        PatientBehaviour[] allPatients = FindObjectsByType<PatientBehaviour>(FindObjectsSortMode.None);
        System.Collections.Generic.List<PatientBehaviour> validTargets = new();

        foreach (PatientBehaviour p in allPatients)
        {
            // Don't target self, and only target patients that can be bitten (level 0, 1, 2)
            if (p != patientBehaviour && p.Data != null && p.Data.CanBeBitten)
            {
                validTargets.Add(p);
            }
        }

        if (validTargets.Count > 0)
        {
            // Pick a random valid target
            int randomIndex = UnityEngine.Random.Range(0, validTargets.Count);
            chaseTarget = validTargets[randomIndex];
            Debug.Log($"[BITE] Crazy patient {patientBehaviour.Data.Id} targeting patient {chaseTarget.Data.Id}");
        }
        else
        {
            chaseTarget = null;
        }
    }

    void PerformBite()
    {
        if (chaseTarget == null || chaseTarget.Data == null)
            return;

        // Trigger bite animation
        if (animator != null)
        {
            animator.SetBool("isBite", true);
            animator.SetBool("isWalking", false);
        }

        Debug.Log($"[BITE] Crazy patient {patientBehaviour.Data.Id} bit patient {chaseTarget.Data.Id}!");
        chaseTarget.Data.OnBitten();

        // Crazy patient explodes after biting
        Debug.Log($"[BITE] Crazy patient {patientBehaviour.Data.Id} explodes after biting!");
        patientBehaviour.Data.Level = Level.Exploded;
        chaseTarget = null;
    }

    void StartWandering()
    {
        isWandering = true;
        stateTimer = Random.Range(minWanderTime, maxWanderTime);
        PickRandomTarget();

        // Update animator
        if (animator != null)
            animator.SetBool("isWalking", true);
    }

    void StartIdling()
    {
        isWandering = false;
        stateTimer = Random.Range(minIdleTime, maxIdleTime);

        // Update animator
        if (animator != null)
            animator.SetBool("isWalking", false);
    }

    void PickRandomTarget()
    {
        float halfWidth = boundarySize.x / 2f;
        float halfDepth = boundarySize.y / 2f;  

        float randomX = Random.Range(boundaryCenter.x - halfWidth, boundaryCenter.x + halfWidth);
        float randomZ = Random.Range(boundaryCenter.y - halfDepth, boundaryCenter.y + halfDepth);

        targetPosition = new Vector3(randomX, transform.position.y, randomZ);
    }
}
