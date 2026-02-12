using PatientSystem;
using TMPro;
using UnityEngine;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public TextMeshProUGUI CrawlingText;
    public TextMeshProUGUI DeadText;
    public TextMeshProUGUI DayText;
    public TextMeshProUGUI RoomText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    public void UpdateUI()
    {
        CrawlingText.text = (
            GameManager.Instance.NumSeverePatients + GameManager.Instance.NumCrazyPatients
        ).ToString();
        // + "/" + (Patient.GetAllPatientsEver().ToString());
        DeadText.text = $"{Patient.GetAllDeathsEver()}" + "/" + $"{GameManager.Instance.NumDeadPatientsForEnding}";
        DayText.text = GameManager.Instance.DayCount.ToString();
        //RoomText.text = IsolationRoomManager.Instance.IsolationRooms.Length.ToString() + "/12";
    }

    public void SetRoomText(int currentRooms, int totalRooms)
    {
        RoomText.text = currentRooms.ToString() + "/" + totalRooms.ToString();
    }
}
