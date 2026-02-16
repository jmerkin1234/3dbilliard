using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Billiards.Debug
{
    /// <summary>
    /// Auto-assigns materials to PoolTableFixed model objects based on naming conventions.
    /// Run from Unity menu: Tools > Assign Pool Table Materials
    /// </summary>
    public class AssignPoolTableMaterials
    {
#if UNITY_EDITOR
        [MenuItem("Tools/Assign Pool Table Materials")]
        public static void AssignMaterials()
        {
            // Find the PoolTableFixed root object
            GameObject poolTable = GameObject.Find("PoolTableFixed");
            if (poolTable == null)
            {
                UnityEngine.Debug.LogError("PoolTableFixed object not found in scene!");
                return;
            }

            int assignedCount = 0;

            // Assign materials to all children
            foreach (Transform child in poolTable.transform)
            {
                string objName = child.name.ToLower();
                Material mat = null;

                // Match object names to materials
                if (objName.Contains("cueball"))
                {
                    mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/CueBall_Mat.mat");
                }
                else if (objName.StartsWith("ball_"))
                {
                    // Try numbered ball materials first, fall back to generic Ball_Mat
                    string ballNum = objName.Replace("ball_", "");
                    mat = AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/Ball_{ballNum}_Mat.mat");
                    if (mat == null)
                        mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Ball_Mat.mat");
                }
                else if (objName.Contains("felt"))
                {
                    mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Felt_Mat.mat");
                }
                else if (objName.Contains("rail"))
                {
                    mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Rail_Mat.mat");
                }
                else if (objName.Contains("pocket"))
                {
                    mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Pocket_Mat.mat");
                }
                else if (objName.Contains("cuestick") || objName.Contains("cue"))
                {
                    mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Cue_Mat.mat");
                }
                else if (objName.Contains("wood") || objName.Contains("frame"))
                {
                    mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Wood_Mat 1.mat");
                }
                else if (objName.Contains("dot"))
                {
                    mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/InlineDot_Mat.mat");
                }

                // Apply material if found
                if (mat != null)
                {
                    Renderer renderer = child.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.sharedMaterial = mat;
                        assignedCount++;
                        UnityEngine.Debug.Log($"Assigned {mat.name} to {child.name}");
                    }
                }
            }

            UnityEngine.Debug.Log($"<color=green>Material assignment complete! {assignedCount} materials assigned.</color>");
            EditorUtility.SetDirty(poolTable);
        }
#endif
    }
}
