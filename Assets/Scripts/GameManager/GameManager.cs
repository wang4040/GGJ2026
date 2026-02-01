using System.Collections;
using System.Collections.Generic;
using PatientSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct BigGameEvent
{
    public int NumNewNormalPatients;
    public int NumNewSlightPatients;
    public int NumNewMediumPatients;
    public int NumNewSeverePatients;
    public float IntervalToNextEvent;
}

[System.Serializable]
public struct TutorialSample
{
    public string Description;
    public Level PatientLevel;
}

public enum GameState
{
    MainMenu,
    Tutorial,
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
    public float DayInterval = 10f;
    public int DayCount = 0;
    public int NumNewPatientsPerDay = 5;
    public float BigEventInterval = 20f;
    public List<BigGameEvent> BigGameEvents;
    public int CurrentBigEventIndex = 0;

    [SerializeField]
    private float globalTimer = 0f;
    private bool isTimerRunning = false;
    private bool isPaused = false;

    [Header("Patient Settings")]
    public GameObject ChildPrefab;
    public GameObject OldPrefab;
    public Transform PatientSpawnPoint;
    public float WanderPatientProportion = 0.4f;

    [Header("Patient Numbers")]
    public int NumAllPatients;
    public int NumNormalPatients;
    public int NumSlightPatients;
    public int NumMediumPatients;
    public int NumSeverePatients;
    public int NumCrazyPatients;
    public int NumExplodedPatients;
    public int NumIncineratedPatients;

    [Header("Parameters")]
    public Material OutlineMaterial2D;
    public float HallInfectionRate = 0.3f;

    [Header("New Patient Parameters")]
    public PatientType NewPatientType = PatientType.Child;
    public float TimerDuration = 5f;
    public float GracePeriod = 8f;
    public float MaskMultiplier = 0.5f;
    public Level StartingInfectionLevel = Level.Normal;

    [Header("Tutorial Settings")]
    public float TutorialPatientInterval = 5f;
    public float MaxTutorialDuration = 90f;
    public PatientsTrakerOnUI patientsTracker;
    public TutorialSample[] TutorialPatientLevels = new TutorialSample[] {
        new TutorialSample { Description = "Normal Infection", PatientLevel = Level.Normal },
        new TutorialSample { Description = "Slight Infection", PatientLevel = Level.Slight },
        new TutorialSample { Description = "Medium Infection", PatientLevel = Level.Medium },
        new TutorialSample { Description = "Severe Infection", PatientLevel = Level.Severe },
    };

    #region Main Game Loop
    private void Start()
    {
        // Subscribe to patient events
        PatientEvents.OnLevelChanged += HandlePatientLevelChanged;
        PatientEvents.OnExploded += HandlePatientExploded;
        if (BigGameEvents == null)
        {
            BigGameEvents = new List<BigGameEvent>();
        }
        CurrentState = GameState.MainMenu;
        if (PatientSpawnPoint == null)
        {
            PatientSpawnPoint = this.transform;
        }
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
        var counts = Patient.GetPatientCountsByLevel();
        NumNormalPatients = counts.ContainsKey(Level.Normal) ? counts[Level.Normal] : 0;
        NumSlightPatients = counts.ContainsKey(Level.Slight) ? counts[Level.Slight] : 0;
        NumMediumPatients = counts.ContainsKey(Level.Medium) ? counts[Level.Medium] : 0;
        NumSeverePatients = counts.ContainsKey(Level.Severe) ? counts[Level.Severe] : 0;
        NumCrazyPatients = counts.ContainsKey(Level.Crazy) ? counts[Level.Crazy] : 0;
        NumExplodedPatients = counts.ContainsKey(Level.Exploded) ? counts[Level.Exploded] : 0;
    }

    void HandlePatientExploded(int patientId)
    {
        // Handle explosion
    }

    private void Update()
    {
        NumAllPatients = Patient.GetAllPatients().Count;
        if (IsDebugMode)
        {
            Testing();
        }
        
        if (isTimerRunning && CurrentState == GameState.Playing && BigGameEvents.Count > 0)
        {
            globalTimer += Time.deltaTime;
            if (globalTimer % DayInterval < Time.deltaTime)
            {
                if (globalTimer >= BigEventInterval)
                {
                    RegisterPatientForBigEvent(BigGameEvents[CurrentBigEventIndex]);
                    BigEventInterval = BigGameEvents[CurrentBigEventIndex].IntervalToNextEvent;
                    CurrentBigEventIndex++;
                    if (CurrentBigEventIndex >= BigGameEvents.Count)
                    {
                        CurrentBigEventIndex = 0;
                    }
                    globalTimer = 0f;
                    Debug.Log("Big event triggered.");
                }
                else
                {
                    RegisterSomePatients(NumNewPatientsPerDay, StartingInfectionLevel);
                    Debug.Log("Normal patient influx triggered.");
                }
                DayCount++;
            }
        }

        if (isTimerRunning && CurrentState == GameState.Tutorial)
        {
            globalTimer += Time.deltaTime;
            if (globalTimer >= MaxTutorialDuration)
            {
                EndTutorial();
            }
        }
    }

    public void StartTutorial()
    {
        isTimerRunning = true;
        globalTimer = 0f;
        CurrentState = GameState.Tutorial;
        patientsTracker.StartTracking();
        StartCoroutine(TutorialPatientSpawn());
    }

    private IEnumerator TutorialPatientSpawn()
    {
        for (int i = 0; i < TutorialPatientLevels.Length; i++)
        {
            int patientId = RegisterPatientAndGetId(TutorialPatientLevels[i].PatientLevel);
            // Update tracker UI with description
            if (patientsTracker != null)
            {
                patientsTracker.UpdateMarkerText(patientId, TutorialPatientLevels[i].Description);
            }
            if (i < TutorialPatientLevels.Length - 1)
            {
                yield return new WaitForSeconds(TutorialPatientInterval);
            }
        }
    }

    public void EndTutorial()
    {
        patientsTracker.StopTracking();
        ResetGame();
    }
    public void StartGame()
    {
        isTimerRunning = true;
        globalTimer = 0f;
        CurrentState = GameState.Playing;
    }

    public void EndGame()
    {
        isTimerRunning = false;
        CurrentState = GameState.GameOver;
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    #endregion

    #region Patient Management
    public void RegisterPatient(Level level)
    {
        int typeIndex = Random.Range(0, 1); // 0 for Child, 1 for Old
        GameObject prefab = (typeIndex < 0.5f) ? ChildPrefab : OldPrefab;
        GameObject initedPatientObj = Instantiate(
            prefab,
            PatientSpawnPoint.position,
            PatientSpawnPoint.rotation
        );

        Patient newPatient = initedPatientObj.GetComponent<PatientBehaviour>().Data;
        newPatient.Type = (typeIndex < 0.5f) ? PatientType.Child : PatientType.Old;
        newPatient.TimerDuration = TimerDuration;
        newPatient.GracePeriod = GracePeriod;
        newPatient.MaskMultiplier = MaskMultiplier;
        newPatient.Level = level;
        // Apply force in the direction PatientSpawnPoint is facing
        Rigidbody rb = initedPatientObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(PatientSpawnPoint.forward * 2f, ForceMode.Impulse);
        }
    }

    public int RegisterPatientAndGetId(Level level)
    {
        int typeIndex = Random.Range(0, 1); // 0 for Child, 1 for Old
        GameObject prefab = (typeIndex < 0.5f) ? ChildPrefab : OldPrefab;
        GameObject initedPatientObj = Instantiate(
            prefab,
            PatientSpawnPoint.position,
            PatientSpawnPoint.rotation
        );

        Patient newPatient = initedPatientObj.GetComponent<PatientBehaviour>().Data;
        newPatient.Type = (typeIndex < 0.5f) ? PatientType.Child : PatientType.Old;
        newPatient.TimerDuration = TimerDuration;
        newPatient.GracePeriod = GracePeriod;
        newPatient.MaskMultiplier = MaskMultiplier;
        newPatient.Level = level;
        // Apply force in the direction PatientSpawnPoint is facing
        Rigidbody rb = initedPatientObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(PatientSpawnPoint.forward * 2f, ForceMode.Impulse);
        }
        return newPatient.Id;
    }
    public void RegisterSomePatients(int count = 1, Level level = Level.Normal)
    {
        StartCoroutine(RegisterPatientsWithDelay(count, level));
    }

    private IEnumerator RegisterPatientsWithDelay(int count, Level level)
    {
        for (int i = 0; i < count; i++)
        {
            RegisterPatient(level);
            if (i < count - 1) // Don't wait after the last one
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    public void RegisterPatientForBigEvent(BigGameEvent bigEvent)
    {
        RegisterSomePatients(bigEvent.NumNewNormalPatients, Level.Normal);
        RegisterSomePatients(bigEvent.NumNewSlightPatients, Level.Slight);
        RegisterSomePatients(bigEvent.NumNewMediumPatients, Level.Medium);
        RegisterSomePatients(bigEvent.NumNewSeverePatients, Level.Severe);
    }
    #endregion

    public void Testing()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            RegisterPatient(StartingInfectionLevel);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            RegisterPatient(StartingInfectionLevel);
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (CurrentState == GameState.Playing)
            {
                EndGame();
            }
            else
            {
                StartGame();
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartTutorial();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            EndTutorial();
        }
    }
}
