using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorGradeTransition : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private Gradient lightColorProgression;
    [SerializeField] private float timeToColorChange = 10f;
    [SerializeField] private float startIntensity = 0f;
    [SerializeField] private float endIntensity = 2f;
    [SerializeField] private Light[] lights;
    [SerializeField] private Renderer[] colortargets;
    [SerializeField] private float emissionMultiplier = 2f;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmmisionColorId = Shader.PropertyToID("_Emission");
    private MaterialPropertyBlock propertyBlock;
    private float elapsedTime;
    private Bloom bloom;

    private ColorAdjustments colorAdjustments;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();

        if(volume == null || volume.profile == null || !volume.profile.TryGet(out bloom))
        {
            Debug.LogError("Assign a Volume whose profile has Bloom.", this);
            enabled = false;
            return;            
        }
        bloom.intensity.overrideState = true;
    }

    private void Update()
    {   
        elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedTime / timeToColorChange);
        Color currentColor = lightColorProgression.Evaluate(progress);
        foreach (Renderer target in colortargets)
        {
            if (target == null) continue;

            target.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, currentColor); // URP shaders
            propertyBlock.SetColor(ColorId, currentColor); // Built-in shader
            propertyBlock.SetColor(EmmisionColorId, currentColor * emissionMultiplier);
            target.SetPropertyBlock(propertyBlock);
        }

        foreach (Light lightSource in lights)
        {
            if (lightSource != null)
            {
                lightSource.color = currentColor;
            }
        }
        bloom.intensity.value = Mathf.Lerp(startIntensity, endIntensity, progress);
    }
}