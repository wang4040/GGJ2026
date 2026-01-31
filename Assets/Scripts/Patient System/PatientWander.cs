using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
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

    private Vector3 targetPosition;
    private float stateTimer;
    private bool isWandering;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartWandering();
    }

    // Update is called once per frame
    void Update()
    {
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

    void StartWandering()
    {
        isWandering = true;
        stateTimer = Random.Range(minWanderTime, maxWanderTime);
        PickRandomTarget();

    }

    void StartIdling()
    {
        isWandering = false;
        stateTimer = Random.Range(minIdleTime, maxIdleTime);
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
