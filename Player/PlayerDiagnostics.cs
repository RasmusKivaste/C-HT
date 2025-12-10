using UnityEngine;

/// <summary>
/// Diagnostiline skript, mis kontrollib Player'i seadistust
/// Lisa see ajutiselt Player GameObject'ile, et kontrollida, mis on valesti
/// </summary>
public class PlayerDiagnostics : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== PLAYER DIAGNOSTIKA ===");
        
        // Kontrolli Character Controller
        CharacterController cc = GetComponent<CharacterController>();
        if (cc == null)
        {
            Debug.LogError("❌ Character Controller puudub!");
        }
        else
        {
            Debug.Log("✅ Character Controller on olemas");
            if (!cc.enabled)
            {
                Debug.LogError("❌ Character Controller on välja lülitatud!");
            }
        }
        
        // Kontrolli SimplePlayerController
        SimplePlayerController spc = GetComponent<SimplePlayerController>();
        if (spc == null)
        {
            Debug.LogError("❌ SimplePlayerController puudub!");
        }
        else
        {
            Debug.Log("✅ SimplePlayerController on olemas");
            if (!spc.enabled)
            {
                Debug.LogError("❌ SimplePlayerController on välja lülitatud!");
            }
        }
        
        // Kontrolli Camera
        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null)
        {
            Debug.LogError("❌ Camera puudub Player'i lastes!");
        }
        else
        {
            Debug.Log($"✅ Camera leitud: {cam.name}");
        }
        
        // Kontrolli positsiooni
        if (transform.position.y <= 0)
        {
            Debug.LogWarning($"⚠️ Player Y positsioon on {transform.position.y} - võib olla liiga madal!");
        }
        else
        {
            Debug.Log($"✅ Player Y positsioon: {transform.position.y}");
        }
        
        Debug.Log("=== DIAGNOSTIKA LÕPP ===");
    }
}

