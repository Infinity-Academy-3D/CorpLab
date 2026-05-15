using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ForkliftVignette : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private ForkliftController forklift;
    [SerializeField] private Volume postProcessVolume;

    [Header("Configurações")]
    [SerializeField] private float maxIntensity = 0.45f;
    [SerializeField] private float fadeInSpeed  = 4f;
    [SerializeField] private float fadeOutSpeed = 2f;
    [SerializeField] private float movementThreshold = 0.001f;

    private Vignette vignette;
    private Vector3 lastPosition;

    void Start()
    {
        if (postProcessVolume == null)
        {
            Debug.LogWarning("[ForkliftVignette] Nenhum Post Process Volume atribuído.");
            return;
        }

        if (!postProcessVolume.profile.TryGet(out vignette))
        {
            Debug.LogWarning("[ForkliftVignette] Vignette não encontrado no perfil do Volume. Adicione o override Vignette ao perfil.");
            return;
        }

        vignette.intensity.Override(0f);
        lastPosition = transform.position;
    }

    void Update()
    {
        if (vignette == null) return;

        float moved = Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        bool isMoving = forklift.IsOccupied && moved > movementThreshold;

        float target  = isMoving ? maxIntensity : 0f;
        float speed   = isMoving ? fadeInSpeed  : fadeOutSpeed;
        float current = vignette.intensity.value;

        vignette.intensity.Override(Mathf.Lerp(current, target, speed * Time.deltaTime));
    }

    void OnDisable()
    {
        if (vignette != null)
            vignette.intensity.Override(0f);
    }
}
