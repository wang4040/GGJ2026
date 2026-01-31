using PatientSystem;
using UnityEngine;

public class IncineratorRoomManager : MonoBehaviour // Singleton
{
    public static IncineratorRoomManager Instance;

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

    [Header("Parameters")]
    public int RoomCapacity = 1;
    public float SingleIncineratorDuration = 2f;
    public int RoomBuildingClicks = 20;

    [Header("State")]
    public int Count_Incinerated = 0;

    void Start()
    {
        IncineratorRoom.Capacity = RoomCapacity;
        IncineratorRoom.SingleIncineratorDuration = SingleIncineratorDuration;
        IncineratorRoom.TotalBuildingClicks = RoomBuildingClicks;
    }

    void Update() { }

    public void RegisterSucessfulIncineration()
    {
        Count_Incinerated++;
    }
}
