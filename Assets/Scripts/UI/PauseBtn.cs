using UnityEngine;
using UnityEngine.UI;

public class PauseBtn : MonoBehaviour
{
    UnityEngine.UI.Button pauseButton;
    public GameObject PauseIcon;
    public GameObject ResumeIcon;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pauseButton = GetComponent<UnityEngine.UI.Button>();
        pauseButton.onClick.AddListener(GameManager.Instance.TogglePause);
        PauseIcon.SetActive(true);
        ResumeIcon.SetActive(false);
        GameManager.Instance.OnGamePaused.AddListener(updatePauseState);
        GameManager.Instance.OnGameResumed.AddListener(updatePauseState);
    }

    private void updatePauseState()
    {
        if (GameManager.Instance.IsPaused)
        {
            PauseIcon.SetActive(false);
            ResumeIcon.SetActive(true);
        }
        else
        {
            PauseIcon.SetActive(true);
            ResumeIcon.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        pauseButton.onClick.RemoveAllListeners();
        GameManager.Instance.OnGamePaused.RemoveListener(updatePauseState);
        GameManager.Instance.OnGameResumed.RemoveListener(updatePauseState);
    }
}
