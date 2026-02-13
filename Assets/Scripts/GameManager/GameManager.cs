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
    public string Instruction;
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
            SoundSys.PlaySound("bgm_loop", loop: true);
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
    
    [Header("Unity Events")]
    public UnityEvent OnGamePaused;
    public UnityEvent OnGameResumed;

    [SerializeField]
    private float globalTimer = 0f;
    private bool isTimerRunning = false;
    public bool IsPaused = false;

    [Header("Control Flags")]
    public bool startGameFlag = false;

    [Header("Patient Settings")]
    public GameObject ChildPrefab;
    public GameObject OldPrefab;
    public Transform PatientSpawnPoint;
    public float WanderPatientProportion = 0.4f;

    [Header("Patient Numbers")]
    public int NumDeadPatientsForEnding = 100;
    public int NumAlivePatients;
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
    public GameObject TutorialInstructionUI;
    public float TutorialPatientInterval = 5f;
    public float MaxTutorialDuration = 90f;
    public PatientsTrakerOnUI patientsTracker;
    public TutorialSample[] TutorialPatientLevels = new TutorialSample[]
    {
        new TutorialSample
        {
            Description = "Normal Infection",
            Instruction = "Wear them the masks.",
            PatientLevel = Level.Normal,
        },
        new TutorialSample
        {
            Description = "Slight Infection",
            Instruction = "Wear them the masks, drag them to the isolation rooms.",
            PatientLevel = Level.Slight,
        },
        new TutorialSample
        {
            Description = "Medium Infection",
            Instruction = "Wear them the masks, drag them to the isolation rooms.",
            PatientLevel = Level.Medium,
        },
        new TutorialSample
        {
            Description = "Severe Infection",
            Instruction = "Drag them to the isolation rooms immediately!!",
            PatientLevel = Level.Severe,
        },
        new TutorialSample
        {
            Description = "Crazy Infection",
            Instruction = "They are wasted, drag them to the incinerator room immediately!!",
            PatientLevel = Level.Crazy,
        },
    };

    #region Main Game Loop
    private void Start()
    {
        // Subscribe to patient events
        PatientEvents.OnLevelChanged += HandlePatientLevelChanged;
        PatientEvents.OnExploded += HandlePatientExploded;
        PatientEvents.OnIncinerated += HandlePatientIncinerated;
        if (BigGameEvents == null)
        {
            BigGameEvents = new List<BigGameEvent>();
        }
        CurrentState = GameState.MainMenu;
        if (PatientSpawnPoint == null)
        {
            PatientSpawnPoint = this.transform;
        }
        if (TutorialInstructionUI != null)
        {
            TutorialInstructionUI.SetActive(false);
        }
        //patientsTracker.gameObject.SetActive(false);
        //TutorialInstructionUI.SetActive(false);
    }

    private void OnDestroy()
    {
        // Unsubscribe from patient events
        PatientEvents.OnLevelChanged -= HandlePatientLevelChanged;
        PatientEvents.OnExploded -= HandlePatientExploded;
        PatientEvents.OnIncinerated -= HandlePatientIncinerated;
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
        NumExplodedPatients = Patient.GetAllExplodedEver();
        // if (patientsTracker != null)
        // {
        //     patientsTracker.UpdateMarkerText(patientId, TutorialPatientLevels[i].Description);
        // }
        UIManager.Instance.UpdateUI();
    }

    void HandlePatientExploded(int patientId)
    {
        // Handle explosion
    }

    void HandlePatientIncinerated(int patientId)
    {
        // Handle incineration
    }

    private void Update()
    {
        NumAlivePatients = Patient.GetAlivePatients().Count;

        // Check start game flag
        if (startGameFlag && CurrentState == GameState.MainMenu)
        {
            StartGame();
            startGameFlag = false;
        }

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
                UIManager.Instance.UpdateUI();
            }
        }

        if (
            Patient.GetAllDeathsEver() == NumDeadPatientsForEnding
            && CurrentState == GameState.Playing
        )
        {
            EndGame();
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
        TutorialInstructionUI.SetActive(true);
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
            TMPro.TextMeshProUGUI instructionText =
                TutorialInstructionUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (instructionText != null)
            {
                instructionText.text = TutorialPatientLevels[i].Instruction;
            }
            if (i < TutorialPatientLevels.Length - 1)
            {
                yield return new WaitForSeconds(TutorialPatientInterval);
            }
        }
    }

    public void EndTutorial()
    {
        // patientsTracker.StopTracking();
        // TutorialInstructionUI.SetActive(false);
        ResetGame();
    }

    public void StartGame()
    {
        isTimerRunning = true;
        globalTimer = 0f;
        CurrentState = GameState.Playing;
        RegisterSomePatients(5, Level.Normal);
    }

    public void EndGame()
    {
        isTimerRunning = false;
        CurrentState = GameState.GameOver;
        SceneManager.LoadScene("MainMenu");
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        OnGameResumed?.Invoke();
    }

    public void TogglePause()
    {
        if (IsPaused)
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
        // Clear all static patient data
        Patient.Reset();

        // Reset game state
        globalTimer = 0f;
        isTimerRunning = false;
        IsPaused = false;
        DayCount = 0;
        CurrentBigEventIndex = 0;
        CurrentState = GameState.MainMenu;

        // Stop all coroutines
        StopAllCoroutines();

        // Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    #endregion

    #region Patient Management
    public void RegisterPatient(Level level)
    {
        float typeIndex = Random.Range(0f, 1f); // 0 for Child, 1 for Old
        Debug.Log($"Registering new patient of typeIndex {typeIndex} with level {level}");
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
        float typeIndex = Random.Range(0f, 1f); // 0 for Child, 1 for Old
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
