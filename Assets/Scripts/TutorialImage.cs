using UnityEngine;

public class TutorialImage : MonoBehaviour
{
    public GameObject tutorialImagePanel;

    void Start()
    {
        tutorialImagePanel.SetActive(false);
        GameManager.Instance.OnGamePaused.AddListener(ToggleTutorialImage);
        GameManager.Instance.OnGameResumed.AddListener(ToggleTutorialImage);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
        {
            tutorialImagePanel.SetActive(false);
        }
    }

    public void ToggleTutorialImage()
    {
        if (!tutorialImagePanel.activeSelf)
        {
            tutorialImagePanel.SetActive(true);
        }
        else
        {
            tutorialImagePanel.SetActive(false);
        }
    }


    void OnDestroy()
    {
        GameManager.Instance.OnGamePaused.RemoveListener(ToggleTutorialImage);
        GameManager.Instance.OnGameResumed.RemoveListener(ToggleTutorialImage);
    }
}
