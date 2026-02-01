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

    public void OnIsolationRoomBuilding(int index, float process)
    {
        if (process >= 1f)
        {
            IsolationRoomUIs[index].IsActive = true;
            IsolationRoomUIs[index].SetUIActive(process);
        }
    }

    void Start()
    {
        IsolationRooms = FindObjectsByType<IsolationRoom>(FindObjectsSortMode.None);
        //IsolationRoomUIs = FindObjectsByType<IsolationWorldSpaceUI>(FindObjectsSortMode.None);
        for (int i = 0; i < IsolationRooms.Length; i++)
        {
            IsolationRooms[i].Index = i;
            //IsolationRoomUIs[i].Index = i;
            //IsolationRoomUIs[i].IsActive = IsolationRooms[i].IsActive;
        }
    }

    void Update() { }

}
