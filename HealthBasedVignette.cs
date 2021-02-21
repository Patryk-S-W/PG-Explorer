
using UnityEngine;
using UnityEngine.PostProcessing;
[RequireComponent(typeof(PostProcessingBehaviour))]
public class HealthBasedVignette : MonoBehaviour
{
    public PostProcessingBehaviour behaviour; 
    public float healthBasedSpeedMultiplier = 1;
    void Awake()
    {
        behaviour.profile = Instantiate(behaviour.profile);
    }
    void SetVignetteSmoothness(float value)
    {
        VignetteModel.Settings vignette = behaviour.profile.vignette.settings;
        vignette.smoothness = value;
        behaviour.profile.vignette.settings = vignette;
    }
    void Update()
    {
        Player player = Player.localPlayer;
        if (!player) return;
        float healthPercent = player.health.Percent();
        float speed = 1 + (1 - healthPercent) * healthBasedSpeedMultiplier; 
        float wave = Mathf.Abs(Mathf.Sin(Time.realtimeSinceStartup * speed));
        SetVignetteSmoothness((1 - healthPercent) * (0.5f + (wave / 2f)));
    }
}