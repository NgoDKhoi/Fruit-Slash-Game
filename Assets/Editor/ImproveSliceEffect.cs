using UnityEngine;
using UnityEditor;

public class ImproveSliceEffect
{
    private const string PrefabPath = "Assets/Prefabs/SliceEffect.prefab";

    [MenuItem("Tools/Upgrade SliceEffect Prefab")]
    public static void Upgrade()
    {
        // Load prefab
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"Cannot find prefab at {PrefabPath}");
            return;
        }
        var ps = prefab.GetComponent<ParticleSystem>();
        if (ps == null)
        {
            Debug.LogError("SliceEffect prefab missing ParticleSystem component.");
            return;
        }

        // ----- Main Module -----
        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = 0.6f;
        main.startSize = 2.5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 7f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.maxParticles = 50;
        main.gravityModifier = 0.5f;

        // Start Color: random between gold and warm pink
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.84f, 0f, 1f),   // gold
            new Color(1f, 0.4f, 0.6f, 1f)    // warm pink
        );

        // ----- Emission -----
        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 25)
        });

        // ----- Shape -----
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        // ----- Size over Lifetime (shrink to 0) -----
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.7f, 0.4f),
            new Keyframe(1f, 0f)
        ));

        // ----- Color over Lifetime (fade out alpha) -----
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient colGrad = new Gradient();
        colGrad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            });
        col.color = new ParticleSystem.MinMaxGradient(colGrad);

        // ===== FIX: Restore Default-Particle material =====
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            // Load Unity's built-in Default-Particle material
            Material defaultParticleMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
            if (defaultParticleMat != null)
            {
                renderer.material = defaultParticleMat;
                Debug.Log("Restored Default-Particle material.");
            }
            else
            {
                Debug.LogWarning("Could not find Default-Particle.mat. Please assign material manually.");
            }
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        // Mark prefab dirty and save
        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();
        Debug.Log("SliceEffect prefab upgraded successfully! Material restored + summer VFX applied.");
    }
}
