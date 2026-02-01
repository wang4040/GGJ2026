using TMPro;
using UnityEngine;

public class StatManager : MonoBehaviour
{
    public TextMeshProUGUI CrawlingText;
    public TextMeshProUGUI DeadText;
    public TextMeshProUGUI DayText;
    public TextMeshProUGUI RoomText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CrawlingText.text = (GameManager.Instance.NumSeverePatients + GameManager.Instance.NumCrazyPatients).ToString() + "/" + (GameManager.Instance.NumAllPatients.ToString());
        DeadText.text = (GameManager.Instance.NumIncineratedPatients + GameManager.Instance.NumExplodedPatients).ToString();
        DayText.text = GameManager.Instance.DayCount.ToString();
        RoomText.text = IsolationRoomManager.Instance.IsolationRooms.Length.ToString() + "/12";
    }
}
