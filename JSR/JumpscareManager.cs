using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Jumpscare manager, mis haldab jumpscare'ide näitamist
/// </summary>
public class JumpscareManager : MonoBehaviour
{
    public static JumpscareManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private Image jumpscareImage;
    [SerializeField] private Image screenOverlay;
    [SerializeField] private Canvas jumpscareCanvas;
    
    [Header("Camera Shake")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float shakeIntensity = 0.5f;
    [SerializeField] private float shakeDuration = 0.5f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    
    private bool isShowingJumpscare = false;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Algseadistus
        if (jumpscareCanvas != null)
        {
            jumpscareCanvas.gameObject.SetActive(false);
        }
        
        if (jumpscareImage != null)
        {
            jumpscareImage.gameObject.SetActive(false);
        }
        
        if (screenOverlay != null)
        {
            screenOverlay.gameObject.SetActive(false);
        }
    }
    
    public void TriggerJumpscare(JumpscareTrigger trigger)
    {
        if (isShowingJumpscare) return; // Ära näita uut, kui juba näidatakse
        
        StartCoroutine(ShowJumpscare(trigger));
    }
    
    private IEnumerator ShowJumpscare(JumpscareTrigger trigger)
    {
        isShowingJumpscare = true;
        
        // Aktiveeri canvas
        if (jumpscareCanvas != null)
        {
            jumpscareCanvas.gameObject.SetActive(true);
        }
        
        // Visuaalne jumpscare
        if (trigger.GetJumpscareType() == JumpscareTrigger.JumpscareType.Visual)
        {
            if (jumpscareImage != null && trigger.GetJumpscareImage() != null)
            {
                jumpscareImage.sprite = trigger.GetJumpscareImage();
                jumpscareImage.gameObject.SetActive(true);
                
                // Fade in
                StartCoroutine(FadeImage(jumpscareImage, true, 0.1f));
            }
        }
        
        // Ekraan overlay (punane värv)
        if (screenOverlay != null)
        {
            screenOverlay.gameObject.SetActive(true);
            StartCoroutine(FadeImage(screenOverlay, true, 0.1f));
        }
        
        // Heli
        if (audioSource != null && trigger.GetJumpscareSound() != null)
        {
            audioSource.PlayOneShot(trigger.GetJumpscareSound(), trigger.GetVolume());
        }
        
        // Camera shake
        if (playerCamera != null)
        {
            StartCoroutine(CameraShake(shakeDuration, shakeIntensity));
        }
        
        // Oota
        yield return new WaitForSeconds(trigger.GetDuration());
        
        // Fade out
        if (jumpscareImage != null)
        {
            StartCoroutine(FadeImage(jumpscareImage, false, 0.6f));
        }
        
        if (screenOverlay != null)
        {
            StartCoroutine(FadeImage(screenOverlay, false, 0.6f));
        }
        
        yield return new WaitForSeconds(0.6f);
        
        // Deaktiveeri
        if (jumpscareImage != null)
        {
            jumpscareImage.gameObject.SetActive(false);
        }
        
        if (screenOverlay != null)
        {
            screenOverlay.gameObject.SetActive(false);
        }
        
        if (jumpscareCanvas != null)
        {
            jumpscareCanvas.gameObject.SetActive(false);
        }
        
        isShowingJumpscare = false;
    }
    
    private IEnumerator FadeImage(Image image, bool fadeIn, float duration)
    {
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsed = 0f;
        
        Color color = image.color;
        color.a = startAlpha;
        image.color = color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            color.a = alpha;
            image.color = color;
            yield return null;
        }
        
        color.a = endAlpha;
        image.color = color;
    }
    
    private IEnumerator CameraShake(float duration, float intensity)
    {
        Vector3 originalPosition = playerCamera.transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            playerCamera.transform.localPosition = originalPosition + new Vector3(x, y, 0f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        playerCamera.transform.localPosition = originalPosition;
    }
    
    public void SetPlayerCamera(Camera camera)
    {
        playerCamera = camera;
    }
}

