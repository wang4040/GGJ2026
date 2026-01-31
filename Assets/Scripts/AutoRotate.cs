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

        // Calculate direction from sprite to camera
        Vector3 directionToCamera = mainCamera.transform.position - transform.position;

        // Calculate the rotation needed to face the camera
        Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);

        // Smoothly rotate towards the camera
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
