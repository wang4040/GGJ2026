using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IncineratorCD : MonoBehaviour
{
    public GameObject kilnRed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartIncineratorCooldown(float t)
    {
        // Implementation for starting the incinerator cooldown
        kilnRed.SetActive(true);
        StartCoroutine(incinCD(t));
    }

    IEnumerator incinCD(float t)
    {
        float elapsed = 0f;
        
        while (elapsed < t)
        {
            elapsed += Time.deltaTime;
            float fillAmount = Mathf.Clamp01(1f - (elapsed / t));
            
            // Apply fill amount to your UI or visual element
            // Example: GetComponent<Image>().fillAmount = fillAmount;
            GetComponent<Image>()!.fillAmount = fillAmount;

            yield return null;
        }

        kilnRed.SetActive(false);
    }
}
