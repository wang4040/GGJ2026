using System.Collections.Generic;
using UnityEngine;

public class IsolationRoomDetector : MonoBehaviour
{
    public List<IsolationRoom> DetectedIsolationRooms;

    public IsolationRoom GetClosestIsolationRoom()
    {
        if (DetectedIsolationRooms == null || DetectedIsolationRooms.Count == 0)
        {
            return null;
        }

        IsolationRoom closestRoom = null;
        float closestDistance = float.MaxValue;

        foreach (IsolationRoom room in DetectedIsolationRooms)
        {
            if (room == null) continue;

            float distance = Vector3.Distance(transform.position, room.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestRoom = room;
            }
        }
        if (closestRoom != null)
        {
            transform.position = closestRoom.transform.position;
            PatientBehaviour patientB = GetComponent<PatientBehaviour>();
            if (patientB != null)
            {
                patientB.Data.InIsolation();
                Debug.Log(
                    $"Invoke InIsolation for Patient {patientB.Data.Id}."
                );
            }
        }
        return closestRoom;
    }

    private void Update()
    {
        if (GameManager.Instance.IsDebugMode && Input.GetKeyDown(KeyCode.M))
        {
            GetClosestIsolationRoom();
        }
    }
}
