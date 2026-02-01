using UnityEngine;

public class TutorialImage : MonoBehaviour
{
    public GameObject tutorialImagePanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
        {
            tutorialImagePanel.SetActive(false);
        }
    }

    public void OpenTutorialImage()
    {
        tutorialImagePanel.SetActive(true);
    }


}
