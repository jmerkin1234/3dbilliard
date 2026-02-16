using UnityEngine;
using Billiards.Cue;
using Billiards.GameState;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Billiards.Debug
{
    /// <summary>
    /// Verifies and auto-fixes all component references and physics materials for the pool table.
    /// Run from Unity menu: Tools > Verify Pool Table Setup
    /// </summary>
    public class VerifyPoolTableSetup
    {
#if UNITY_EDITOR
        [MenuItem("Tools/Verify Pool Table Setup")]
        public static void VerifyAndFixSetup()
        {
            UnityEngine.Debug.Log("=== Pool Table Setup Verification ===");

            GameObject poolTable = GameObject.Find("PoolTableFixed");
            if (poolTable == null)
            {
                UnityEngine.Debug.LogError("PoolTableFixed not found in scene!");
                return;
            }

            // Find key objects
            Transform cueball = poolTable.transform.Find("cueball");
            Transform cuestick = poolTable.transform.Find("cuestick");
            Transform felt = poolTable.transform.Find("felt");
            GameObject gameManager = GameObject.Find("GameManager");

            if (cueball == null) { UnityEngine.Debug.LogError("Cueball not found!"); return; }
            if (cuestick == null) { UnityEngine.Debug.LogError("Cuestick not found!"); return; }
            if (gameManager == null) { UnityEngine.Debug.LogError("GameManager not found!"); return; }

            int fixedCount = 0;

            // === 1. Assign Physics Materials ===
            UnityEngine.Debug.Log("\n--- Assigning Physics Materials ---");

            PhysicsMaterial ballMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/PhysicsMaterials 1/Ball.physicMaterial");
            PhysicsMaterial feltMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/PhysicsMaterials 1/Felt.physicMaterial");
            PhysicsMaterial railsMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/PhysicsMaterials 1/Rails.physicMaterial");

            if (ballMat == null) UnityEngine.Debug.LogError("Ball.physicMaterial not found!");
            if (feltMat == null) UnityEngine.Debug.LogError("Felt.physicMaterial not found!");
            if (railsMat == null) UnityEngine.Debug.LogError("Rails.physicMaterial not found!");

            // Cueball physics material
            if (ballMat != null)
            {
                SphereCollider cueballCollider = cueball.GetComponent<SphereCollider>();
                if (cueballCollider != null)
                {
                    cueballCollider.material = ballMat;
                    UnityEngine.Debug.Log("✓ Assigned Ball.physicMaterial to cueball");
                    fixedCount++;
                }

                // All numbered balls
                for (int i = 1; i <= 15; i++)
                {
                    string ballName = $"ball_{i:D2}";
                    Transform ball = poolTable.transform.Find(ballName);
                    if (ball != null)
                    {
                        SphereCollider ballCollider = ball.GetComponent<SphereCollider>();
                        if (ballCollider != null)
                        {
                            ballCollider.material = ballMat;
                            fixedCount++;
                        }
                    }
                }
                UnityEngine.Debug.Log($"✓ Assigned Ball.physicMaterial to all 16 balls");
            }

            // Felt physics material
            if (felt != null && feltMat != null)
            {
                BoxCollider feltCollider = felt.GetComponent<BoxCollider>();
                if (feltCollider != null)
                {
                    feltCollider.material = feltMat;
                    UnityEngine.Debug.Log("✓ Assigned Felt.physicMaterial to felt");
                    fixedCount++;
                }
            }

            // Rails physics materials
            if (railsMat != null)
            {
                for (int i = 1; i <= 6; i++)
                {
                    string railName = $"rail_{i:D2}";
                    Transform rail = poolTable.transform.Find(railName);
                    if (rail != null)
                    {
                        BoxCollider railCollider = rail.GetComponent<BoxCollider>();
                        if (railCollider != null)
                        {
                            railCollider.material = railsMat;
                            fixedCount++;
                        }
                    }
                }
                UnityEngine.Debug.Log($"✓ Assigned Rails.physicMaterial to all 6 rails");
            }

            // === 2. Wire Component References ===
            UnityEngine.Debug.Log("\n--- Wiring Component References ---");

            // CueAim references
            CueAim cueAim = cuestick.GetComponent<CueAim>();
            if (cueAim != null)
            {
                // Set cueBall reference
                if (cueAim.cueBall == null)
                {
                    // Use reflection to set private field
                    var cueBallField = typeof(CueAim).GetField("cueBall",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (cueBallField != null)
                    {
                        cueBallField.SetValue(cueAim, cueball);
                        UnityEngine.Debug.Log("✓ Connected CueAim.cueBall → cueball");
                        fixedCount++;
                    }
                }

                // Set aimLineRenderer reference
                LineRenderer lineRenderer = cueball.GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    var lineRendererField = typeof(CueAim).GetField("aimLineRenderer",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (lineRendererField != null)
                    {
                        lineRendererField.SetValue(cueAim, lineRenderer);
                        UnityEngine.Debug.Log("✓ Connected CueAim.aimLineRenderer → cueball LineRenderer");
                        fixedCount++;
                    }
                }

                EditorUtility.SetDirty(cueAim);
            }

            // CueStrike references
            CueStrike cueStrike = cuestick.GetComponent<CueStrike>();
            if (cueStrike != null)
            {
                var cueBallField = typeof(CueStrike).GetField("cueBall",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (cueBallField != null)
                {
                    cueBallField.SetValue(cueStrike, cueball.gameObject);
                    UnityEngine.Debug.Log("✓ Connected CueStrike.cueBall → cueball");
                    fixedCount++;
                }
                EditorUtility.SetDirty(cueStrike);
            }

            // TurnManager references
            TurnManager turnManager = gameManager.GetComponent<TurnManager>();
            if (turnManager != null)
            {
                var cueAimField = typeof(TurnManager).GetField("cueAim",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (cueAimField != null && cueAim != null)
                {
                    cueAimField.SetValue(turnManager, cueAim);
                    UnityEngine.Debug.Log("✓ Connected TurnManager.cueAim → cuestick CueAim");
                    fixedCount++;
                }

                ShotPower shotPower = cuestick.GetComponent<ShotPower>();
                var shotPowerField = typeof(TurnManager).GetField("shotPower",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (shotPowerField != null && shotPower != null)
                {
                    shotPowerField.SetValue(turnManager, shotPower);
                    UnityEngine.Debug.Log("✓ Connected TurnManager.shotPower → cuestick ShotPower");
                    fixedCount++;
                }

                EditorUtility.SetDirty(turnManager);
            }

            // === 3. Verify Ball Positions ===
            UnityEngine.Debug.Log("\n--- Verifying Ball Positions ---");
            float feltTop = 0.771f; // From Blender model
            float ballRadius = 0.028575f;
            float targetY = feltTop + ballRadius;

            // Check cueball position
            if (Mathf.Abs(cueball.position.y - targetY) > 0.01f)
            {
                UnityEngine.Debug.LogWarning($"Cueball Y position is {cueball.position.y}, should be ~{targetY}");
            }

            UnityEngine.Debug.Log($"\n<color=green>=== Setup Complete! Fixed {fixedCount} issues ===</color>");
            UnityEngine.Debug.Log("\nNext steps:");
            UnityEngine.Debug.Log("1. Enter Play Mode to test");
            UnityEngine.Debug.Log("2. Use mouse to aim, hold Space to charge shot, release to shoot");
            UnityEngine.Debug.Log("3. Check Console for any runtime errors");
        }
#endif
    }
}
