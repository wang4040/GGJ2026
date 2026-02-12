using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject CreditImg;

    public void OnStartButtonPressed()
    {
        SceneManager.LoadScene("Level1");
    }

    public void OnCreditsButtonPressed()
    {
        CreditImg.SetActive(true);
    }
}
