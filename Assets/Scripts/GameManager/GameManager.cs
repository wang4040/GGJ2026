using System.Collections.Generic;
using PatientSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct BigGameEvent
{
    public int Index;
    public int Num_newPatients;
    public int Num_newSeverePatients;
    public float IntervalToNextEvent;
}

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
    public float BigEventInterval = 10f;
    public List<BigGameEvent> BigGameEvents;
    public int CurrentBigEventIndex = 0;

    [SerializeField]
    private float globalTimer = 0f;
    private bool isTimerRunning = false;

    [Header("Patient Settings")]
    public GameObject ChildPrefab;
    public GameObject OldPrefab;
    public Transform PatientSpawnPoint;

    [Header("Parameters")]
    public Material OutlineMaterial2D;
    public float HallInfectionRate = 0.3f;

    [Header("New Patient Parameters")]
    public PatientType NewPatientType = PatientType.Child;
    public float TimerDuration = 10f;
    public float GracePeriod = 8f;
    public float MaskMultiplier = 0.5f;
    public Level StartingInfectionLevel = Level.Normal;

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
    }

    void HandlePatientExploded(int patientId)
    {
        // Handle explosion
    }

    private void Update()
    {
        if (IsDebugMode)
            Testing();
        if (isTimerRunning && BigGameEvents.Count > 0)
        {
            globalTimer += Time.deltaTime;
            if (globalTimer >= BigEventInterval)
            {
                //BigGameEventManager.Instance.TriggerBigGameEvent(CurrentBigEventIndex);
                RegisterPatientForBigEvent(BigGameEvents[CurrentBigEventIndex]);
                BigEventInterval =
                    BigGameEvents[CurrentBigEventIndex].IntervalToNextEvent;
                CurrentBigEventIndex++;
                if (CurrentBigEventIndex >= BigGameEvents.Count)
                {
                    CurrentBigEventIndex = 0;
                }
                globalTimer = 0f;
            }
        }
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

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    #endregion

    #region Patient Management
    public void RegisterPatients(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            int typeIndex = Random.Range(0, 1);
            GameObject prefab = (typeIndex < 0.5f) ? ChildPrefab : OldPrefab;
            Patient newPatient = Instantiate(
                    prefab,
                    PatientSpawnPoint.position,
                    PatientSpawnPoint.rotation
                )
                .GetComponent<PatientBehaviour>()
                .Data;
            newPatient.Type = (typeIndex < 0.5f) ? PatientType.Child : PatientType.Old;
            newPatient.TimerDuration = TimerDuration;
            newPatient.GracePeriod = GracePeriod;
            newPatient.MaskMultiplier = MaskMultiplier;
            newPatient.Level = StartingInfectionLevel;
        }
    }

    public void RegisterPatientForBigEvent(BigGameEvent bigEvent)
    {
        for (int i = 0; i < bigEvent.Num_newPatients; i++)
        {
            int typeIndex = Random.Range(0, 1);
            GameObject prefab = (typeIndex < 0.5f) ? ChildPrefab : OldPrefab;
            Patient newPatient = Instantiate(
                    prefab,
                    PatientSpawnPoint.position,
                    PatientSpawnPoint.rotation
                )
                .GetComponent<PatientBehaviour>()
                .Data;
            newPatient.Type = (typeIndex < 0.5f) ? PatientType.Child : PatientType.Old;
            newPatient.TimerDuration = TimerDuration;
            newPatient.GracePeriod = GracePeriod;
            newPatient.MaskMultiplier = MaskMultiplier;
            newPatient.Level = StartingInfectionLevel;
        }
        for (int i = 0; i < bigEvent.Num_newSeverePatients; i++)
        {
            int typeIndex = Random.Range(0, 1);
            GameObject prefab = (typeIndex < 0.5f) ? ChildPrefab : OldPrefab;
            Patient newPatient = Instantiate(
                    prefab,
                    PatientSpawnPoint.position,
                    PatientSpawnPoint.rotation
                )
                .GetComponent<PatientBehaviour>()
                .Data;
            newPatient.Type = (typeIndex < 0.5f) ? PatientType.Child : PatientType.Old;
            newPatient.TimerDuration = TimerDuration;
            newPatient.GracePeriod = GracePeriod;
            newPatient.MaskMultiplier = MaskMultiplier;
            newPatient.Level = Level.Severe;
        }
    }
    #endregion

    public void Testing()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            RegisterPatients();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            RegisterPatients();
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
    }
}
