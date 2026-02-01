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

        Debug.Log(
            $"Start to find the closest isolation room."
        );
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
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 targetPos = new Vector3(
                    closestRoom.transform.position.x, 
                    transform.position.y, 
                    closestRoom.transform.position.z
                );
                Debug.Log(
                    $"Moving patient {GetComponent<PatientBehaviour>().Data.Id} to closest isolation room #{closestRoom.Index} at position {targetPos}."
                );
                rb.MovePosition(targetPos);
            }
            PatientBehaviour patientB = GetComponent<PatientBehaviour>();
            // if (patientB != null)
            // {
            //     patientB.Data.InIsolation();
            //     Debug.Log(
            //         $"Invoke InIsolation for Patient {patientB.Data.Id}."
            //     );
            // }
            Debug.Log(
                $"Closest isolation room for Patient {patientB.Data.Id} is Isolation Room #{closestRoom.Index}."
            );
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
