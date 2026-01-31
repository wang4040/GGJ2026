using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Playing,
    GameOver,
}

public class GameManager : MonoBehaviour
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

    [Header("Patient Settings")]
    public GameObject PatientPrefab;

    private void Start()
    {
        
    }

    private void Update()
    {
        if (IsDebugMode)
            Testing();
    }

    public void StartGame()
    {
        
    }

    public void EndGame()
    {
        
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddPatient(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            Instantiate(PatientPrefab);
        }
    }

    public void Testing()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddPatient();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }
    }
}
