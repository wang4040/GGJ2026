using UnityEngine;

public class ExplodeEvent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnExplode()
    {
        GetComponentInParent<PatientWander>().Explode();
    }
}
