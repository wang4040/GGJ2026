using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using PatientSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using System.Collections;

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
    public bool isChasing = false;

    // Animator reference
    private Animator animator;

    // Bark state tracking (Crazy patients bark before crawling)
    public float crazyIdlDuration = 4f;
    public bool hasBarked = false;
    public bool isBarking = false;
    public float barkDuration = 2f;
    private float barkTimer = 0f;
    private bool crazyIdle = false;
    private bool tryChase = false;

    public bool stop = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        patientBehaviour = GetComponent<PatientBehaviour>();
        animator = GetComponentInChildren<Animator>();
        StartWandering();
    }

    // Update is called once per frame
    void Update()
    {
        if (stop)
        {
            return;
        }

        if (patientBehaviour == null || patientBehaviour.Data == null)
            return;

        // Patients in Isolation room just idle, no wandering
        if (patientBehaviour.Data.Room == Room.Isolation)
        {
            if (isWandering)
            {
                StartIdling();
            }
            return;
        }

        // Check if patient is Crazy - switch to chase mode
        if (patientBehaviour.Data.CanBite || (patientBehaviour.Data.Level == Level.Exploded))
        {
            if (!crazyIdle) StartCoroutine(WaitThenBark());
            if (tryChase) UpdateChaseMode();
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
            OnFlip(targetPosition);
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

    IEnumerator WaitThenBark()
    {
        crazyIdle = true;
        animator.Play("CrazyIdle");
        yield return new WaitForSeconds(crazyIdlDuration);
        animator.Play("Bark");
        yield return new WaitForSeconds(barkDuration);
        animator.Play("CrazyCrawl");
        tryChase = true;
    }

    void UpdateChaseMode()
    {
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
                OnFlip(targetPosition);
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
        OnFlip(targetPos);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, crazyMoveSpeed * Time.deltaTime);

        // Check if close enough to bite
        //float distance = Vector3.Distance(transform.position, targetPos);
        //if (distance <= biteRadius)
        //{
        //    PerformBite();
        //}
    }

    void FindBiteTarget()
    {
        PatientBehaviour[] allPatients = FindObjectsByType<PatientBehaviour>(FindObjectsSortMode.None);
        System.Collections.Generic.List<PatientBehaviour> validTargets = new();

        foreach (PatientBehaviour p in allPatients)
        {
            // Don't target self, only target patients that can be bitten, and not in Isolation room
            if (p != patientBehaviour && p.Data != null && p.Data.CanBeBitten && p.Data.Room != Room.Isolation)
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
        chaseTarget.GetComponent<PatientWander>().stop = true;
        chaseTarget.GetComponentInChildren<Animator>()?.SetBool("isWalking", false);
        animator.Play("Bite");
    }

    public void StartWandering()
    {
        isWandering = true;
        stateTimer = Random.Range(minWanderTime, maxWanderTime);
        PickRandomTarget();

        // Update animator
        if (animator != null)
            animator.SetBool("isWalking", true);
    }

    public void StartIdling()
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PatientBehaviour>() == chaseTarget && patientBehaviour.Data.CanBite && isChasing)
        {
            Debug.Log("Collision detected with chase target!");
            tryChase = false;
            PerformBite();
        }
        else
        {
            Debug.Log("patientBehavior.Data.CanBite: " + patientBehaviour.Data.CanBite);
            Debug.Log("isChasing: " + isChasing);
        }
    }

    public void Explode()
    {
        stop = true;
        Debug.Log($"[BITE] Crazy patient {patientBehaviour.Data.Id} bit patient {chaseTarget.Data.Id}!");
        chaseTarget.GetComponent<PatientWander>().stop = false;
        chaseTarget.GetComponent<PatientWander>().StartWandering();
        chaseTarget.Data.OnBitten();
        chaseTarget = null;

        StartCoroutine(WaitThenExplode());
    }

    IEnumerator WaitThenExplode()
    {
        yield return new WaitForSeconds(1.5f);
        animator.Play("Explode");
        patientBehaviour.Data.Level = Level.Exploded;
    }

    void OnFlip(Vector3 targetPos)
    {
        // Flip sprite based on movement direction
        if (targetPos.x < transform.position.x)
        {
            // Moving left - flip to face left
            transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
        }
        else if (targetPos.x > transform.position.x)
        {
            // Moving right - face right
            transform.localScale = new Vector3(-1f, transform.localScale.y, transform.localScale.z);
        }
    }
}
