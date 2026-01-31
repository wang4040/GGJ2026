using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using PatientSystem;

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

    [Header("Parameters")]
    public Material OutlineMaterial2D;
    public float HallInfectionRate = 0.3f;

    #region Main Game Loop
    private void Start()
    {
        // Subscribe to patient events
        PatientEvents.OnLevelChanged += HandlePatientLevelChanged;
        PatientEvents.OnExploded += HandlePatientExploded;
    }

    private void OnDestroy()
    {
        // Unsubscribe from patient events
        PatientEvents.OnLevelChanged -= HandlePatientLevelChanged;
        PatientEvents.OnExploded -= HandlePatientExploded;
    }

    void HandlePatientLevelChanged(int patientId, Level oldLevel, Level newLevel)
    {
        // A patient's level changed - notify listeners
    }

    void HandlePatientExploded(int patientId)
    {
        // Handle explosion
    }

    private void Update()
    {
        if (IsDebugMode)
            Testing();
        if (isTimerRunning)
        {
            globalTimer += Time.deltaTime;
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
