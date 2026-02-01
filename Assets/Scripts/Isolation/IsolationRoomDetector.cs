using System.Collections.Generic;
using UnityEngine;

public class IsolationRoomDetector : MonoBehaviour
{
    public List<IsolationRoom> DetectedIsolationRooms;

    [Tooltip("Maximum distance to snap to an isolation room center")]
    public float SnapDistance = 5f;

    public IsolationRoom GetClosestIsolationRoom()
    {
        // Check all isolation rooms from manager, not just detected ones
        // This avoids race conditions where OnTriggerExit clears the list before we can snap
        if (IsolationRoomManager.Instance == null || IsolationRoomManager.Instance.IsolationRooms == null)
        {
            return null;
        }

        Debug.Log($"Start to find the closest isolation room.");
        IsolationRoom closestRoom = null;
        float closestDistance = float.MaxValue;

        foreach (IsolationRoom room in IsolationRoomManager.Instance.IsolationRooms)
        {
            if (room == null || !room.IsActive) continue;

            float distance = Vector3.Distance(transform.position, room.transform.position);
            if (distance < closestDistance && distance <= SnapDistance)
            {
                closestDistance = distance;
                closestRoom = room;
            }
        }
        if (closestRoom != null)
        {
            Vector3 targetPos = new Vector3(
                closestRoom.transform.position.x,
                transform.position.y,
                closestRoom.transform.position.z
            );
            Debug.Log(
                $"Snapping patient {GetComponent<PatientBehaviour>().Data.Id} to isolation room #{closestRoom.Index} at position {targetPos}."
            );
            // Use transform.position for immediate snap (MovePosition doesn't work well outside FixedUpdate)
            transform.position = targetPos;
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
