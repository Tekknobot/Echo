using UnityEngine;

public class ToonExplosion : MonoBehaviour {
    [Header("Explosion Settings")]
    public float explosionDuration = 1.0f;  // How long the explosion lasts
    public int particleCount = 50;          // Number of particles in the burst
    public float startSpeed = 5f;           // Initial speed of particles
    public float startSize = 1f;            // Starting size of particles
    public Color startColor = Color.yellow; // Base color (can be modified for different effects)
    
    [Header("References")]
    public Material toonMaterial;           // Assign your toon-style material

    private ParticleSystem explosionPS;

    void Start() {
        // Create and configure the explosion particle system
        explosionPS = CreateExplosionParticleSystem();
        explosionPS.Play();
        // Destroy the explosion GameObject after the duration is over
        Destroy(gameObject, explosionDuration + 0.5f);
    }

    ParticleSystem CreateExplosionParticleSystem() {
        // Create a new GameObject to hold the Particle System
        GameObject psObject = new GameObject("ToonExplosionPS");
        psObject.transform.parent = transform;
        psObject.transform.localPosition = Vector3.zero;
        
        // Add a ParticleSystem component
        ParticleSystem ps = psObject.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = explosionDuration;
        main.loop = false;
        main.startLifetime = explosionDuration * 0.5f;
        main.startSpeed = startSpeed;
        main.startSize = startSize;
        main.startColor = startColor;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // Emission: one burst at time 0
        var emission = ps.emission;
        emission.enabled = true;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, (short)particleCount)
        });
        
        // Shape: a sphere for a radial explosion
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        // Renderer: assign toon material
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = toonMaterial;
        // Optionally, set render mode to Billboard
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        // Return the configured ParticleSystem
        return ps;
    }
}
