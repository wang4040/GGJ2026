using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Playing,
    GameOver,
}

public class GameManager : MonoBehaviour // Singleton
{
    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("Game Settings")]
#if UNITY_EDITOR
    public bool IsDebugMode = true;
#else
    public bool IsDebugMode = false;
#endif
    public GameState CurrentState = GameState.MainMenu;

    [SerializeField]
    private float globalTimer = 0f;
    private bool isTimerRunning = false;

    [Header("Patient Settings")]
    public GameObject ChildPrefab;
    public GameObject OldPrefab;

    [HideInInspector]
    public UnityEvent UpdatePatientState;

    [Header("Parameters")]
    public Material OutlineMaterial2D;
    public float UpdateInterval = 5f;

    #region Main Game Loop
    private void Start() { }

    private void Update()
    {
        if (IsDebugMode)
            Testing();
        if (isTimerRunning)
        {
            globalTimer += Time.deltaTime;
            if (globalTimer >= UpdateInterval)
            {
                globalTimer = 0f;
                UpdatePatientState?.Invoke();
            }
        }
    }

    public void StartGame()
    {
        isTimerRunning = true;
        globalTimer = 0f;
    }

    public void EndGame()
    {
        isTimerRunning = false;
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    #endregion

    #region Patient Management
    public void RegisterPatient(GameObject prefab, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            Instantiate(prefab);
        }
    }
    #endregion

    public void Testing()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            RegisterPatient(ChildPrefab);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            RegisterPatient(OldPrefab);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }
    }
}
