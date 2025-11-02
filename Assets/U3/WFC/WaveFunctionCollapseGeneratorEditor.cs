using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaveFunctionCollapseGenerator))]
public class WaveFunctionCollapseGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WaveFunctionCollapseGenerator gen = (WaveFunctionCollapseGenerator)target;
        IntGrid grid = gen.inputExample;

        if (grid == null) return;

        // Asegurar que la lista tenga el tamaño correcto
        if (grid.values == null || grid.values.Count != grid.width * grid.height)
        {
            grid.Resize(grid.width, grid.height);
        }


        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Matriz de ejemplo (inputExample)", EditorStyles.boldLabel);

        int newWidth = EditorGUILayout.IntField("Ancho", grid.width);
        int newHeight = EditorGUILayout.IntField("Alto", grid.height);

        if (newWidth != grid.width || newHeight != grid.height)
        {
            Undo.RecordObject(gen, "Resize Input Grid");
            grid.Resize(Mathf.Max(1, newWidth), Mathf.Max(1, newHeight));
            EditorUtility.SetDirty(gen);
        }

        EditorGUILayout.Space(5);
        for (int y = 0; y < grid.height; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < grid.width; x++)
            {
                int current = grid[x, y];
                int newVal = EditorGUILayout.IntField(current, GUILayout.Width(35));
                if (newVal != current)
                {
                    Undo.RecordObject(gen, "Edit Grid Value");
                    grid[x, y] = newVal;
                    EditorUtility.SetDirty(gen);
                }
            }
            EditorGUILayout.EndHorizontal();
        }


        EditorGUILayout.Space(10);
        if (GUILayout.Button("Generar mapa"))
        {
            gen.Generate();
        }
    }
}
