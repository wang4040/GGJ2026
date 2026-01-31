using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    private Camera mainCamera;
    public float rotationSpeed = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (mainCamera == null)
            return;

        // Calculate direction from camera to sprite (away from camera)
        Vector3 directionAwayFromCamera = transform.position - mainCamera.transform.position;

        // Calculate the rotation needed to face away from the camera
        Quaternion targetRotation = Quaternion.LookRotation(directionAwayFromCamera);

        // Extract only x and z rotation by zeroing the y component
        Vector3 targetEuler = targetRotation.eulerAngles;
        targetEuler.y = transform.eulerAngles.y;
        targetRotation = Quaternion.Euler(targetEuler);

        // Smoothly rotate towards the away direction
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
