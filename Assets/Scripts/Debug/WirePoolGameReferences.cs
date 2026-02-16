using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Billiards.Debug
{
    /// <summary>
    /// Wires component references for PoolGame scene
    /// Run from Unity menu: Tools > Wire Pool Game References
    /// </summary>
    public class WirePoolGameReferences
    {
#if UNITY_EDITOR
        [MenuItem("Tools/Wire Pool Game References")]
        public static void WireReferences()
        {
            // Find objects
            GameObject cueball = GameObject.Find("cueball");
            GameObject cuestick = GameObject.Find("cuestick");
            GameObject gameManager = GameObject.Find("GameManager");

            if (cueball == null || cuestick == null || gameManager == null)
            {
                UnityEngine.Debug.LogError("Could not find required GameObjects!");
                return;
            }

            // Get components
            var cueAim = cuestick.GetComponent<Billiards.Cue.CueAim>();
            var cueStrike = cuestick.GetComponent<Billiards.Cue.CueStrike>();
            var shotPower = cuestick.GetComponent<Billiards.Cue.ShotPower>();
            var turnManager = gameManager.GetComponent<Billiards.GameState.TurnManager>();
            var lineRenderer = cueball.GetComponent<LineRenderer>();

            if (cueAim == null || cueStrike == null || shotPower == null || turnManager == null || lineRenderer == null)
            {
                UnityEngine.Debug.LogError("Could not find required components!");
                return;
            }

            // Wire CueAim references
            SerializedObject cueAimSO = new SerializedObject(cueAim);
            cueAimSO.FindProperty("cueBall").objectReferenceValue = cueball.transform;
            cueAimSO.FindProperty("aimLineRenderer").objectReferenceValue = lineRenderer;
            cueAimSO.ApplyModifiedProperties();

            // Wire CueStrike reference
            SerializedObject cueStrikeSO = new SerializedObject(cueStrike);
            cueStrikeSO.FindProperty("cueBall").objectReferenceValue = cueball;
            cueStrikeSO.ApplyModifiedProperties();

            // Wire TurnManager references
            SerializedObject turnManagerSO = new SerializedObject(turnManager);
            turnManagerSO.FindProperty("cueAim").objectReferenceValue = cueAim;
            turnManagerSO.FindProperty("shotPower").objectReferenceValue = shotPower;
            turnManagerSO.ApplyModifiedProperties();

            UnityEngine.Debug.Log("<color=green>All references wired successfully!</color>");
            EditorUtility.SetDirty(cuestick);
            EditorUtility.SetDirty(gameManager);
        }
#endif
    }
}
