using TMPro;
using UnityEngine;

public class IsolationWorldSpaceUI : MonoBehaviour
{
    public TextMeshProUGUI buildingProgressText;

    public void SetUIActive(float process)
    {
        if (process >= 1f)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            if (!this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(true);
            }
            buildingProgressText.text = $"{(int)(process * 100f)}%";
        }
    }
}
