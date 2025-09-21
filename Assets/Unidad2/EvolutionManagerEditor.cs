#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EvolutionManager))]
public class EvolutionManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EvolutionManager script = (EvolutionManager)target;

        if (GUILayout.Button("Run Evolution"))
        {
            script.StartCoroutine(script.RunESCoroutine());
        }
    }
}
#endif
