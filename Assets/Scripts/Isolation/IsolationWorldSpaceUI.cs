using TMPro;
using UnityEngine;

public class IsolationWorldSpaceUI : MonoBehaviour
{
    public int Index;
    public bool IsActive = true;
    public GameObject isolationRoomUIPrefab;

    public GameObject activeUI;
    public TextMeshProUGUI buildingProgressText;

    public void SetUIActive(float process)
    {
        if (process >= 1f)
        {
            IsActive = true;
            activeUI.SetActive(true);
        }
        else
        {
            IsActive = false;
            activeUI.SetActive(false);
            buildingProgressText.text = $"{(int)(process * 100f)}%";
        }
    }
}
