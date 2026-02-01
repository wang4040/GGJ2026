using PatientSystem;
using UnityEngine;

public class IncineratorRoomManager : MonoBehaviour // Singleton
{
    public static IncineratorRoomManager Instance;
    public IncineratorRoom[] IncineratorRooms;

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

        IncineratorRoom.Capacity = RoomCapacity;
        IncineratorRoom.SingleIncineratorDuration = SingleIncineratorDuration;
        IncineratorRoom.TotalBuildingClicks = RoomBuildingClicks;
    }

    [Header("Parameters")]
    public int RoomCapacity = 1;
    public float SingleIncineratorDuration = 2f;
    public int RoomBuildingClicks = 20;

    [Header("State")]
    public int Count_Incinerated = 0;

    void Start()
    {
        IncineratorRooms = FindObjectsByType<IncineratorRoom>(FindObjectsSortMode.None);
        for (int i = 0; i < IncineratorRooms.Length; i++)
        {
            IncineratorRooms[i].Index = i;
        }
    }

    void Update() { }

    public void RegisterSucessfulIncineration()
    {
        Count_Incinerated++;
    }
}
