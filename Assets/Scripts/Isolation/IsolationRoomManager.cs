using UnityEngine;

public class IsolationRoomManager : MonoBehaviour // Singleton
{
    public static IsolationRoomManager Instance;
    public IsolationRoom[] IsolationRooms;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        IsolationRoom.Capacity = RoomCapacity;
        IsolationRoom.SingleIsolationDuration = SingleIsolationDuration;
        IsolationRoom.TotalBuildingClicks = RoomBuildingClicks;
    }

    [Header("Parameters")]
    public int RoomCapacity = 1;
    public float SingleIsolationDuration = 2f;
    public int RoomBuildingClicks = 20;
    public float InfectionRate = 0.1f; // Lower infection rate in isolation

    [Header("State")]
    public int Count_Isolated = 0;

    void Start()
    {
        IsolationRooms = FindObjectsByType<IsolationRoom>(FindObjectsSortMode.None);
        for (int i = 0; i < IsolationRooms.Length; i++)
        {
            IsolationRooms[i].Index = i;
        }
    }

    void Update() { }

}
