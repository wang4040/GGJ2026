using UnityEngine;
using UnityEngine.Events;

public class IsolationRoomManager : MonoBehaviour // Singleton
{
    public static IsolationRoomManager Instance;
    public IsolationRoom[] IsolationRooms;
    public IsolationWorldSpaceUI[] IsolationRoomUIs;

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
        var roomsFound = FindObjectsByType<IsolationRoom>(FindObjectsSortMode.None);
        IsolationRooms = new IsolationRoom[roomsFound.Length];
        foreach (var room in roomsFound)
        {
            IsolationRooms[room.Index] = room;
        }
        SetIsolationRoomsUI();
    }

    void Update() { }

    public int GetActiveRoomsCount()
    {
        int count = 0;
        foreach (var room in IsolationRooms)
        {
            if (room != null && room.IsActive)
            {
                count++;
            }
        }
        return count;
    }

    public void SetIsolationRoomsUI()
    {
        StatManager.Instance.SetRoomText(GetActiveRoomsCount(), IsolationRooms.Length);
    }
}
