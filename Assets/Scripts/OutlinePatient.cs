using UnityEngine;

public class OutlinePatient : MonoBehaviour
{
    Material originalMaterial;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalMaterial = this.GetComponentInChildren<SpriteRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OutlineObject()
    {
        SpriteRenderer renderer = this.GetComponentInChildren<SpriteRenderer>();
        renderer.material = new Material(GameManager.Instance.OutlineMaterial2D);
    }

    public void RemoveOutline()
    {
        SpriteRenderer renderer = this.GetComponentInChildren<SpriteRenderer>();
        renderer.material = originalMaterial;
    }
}
