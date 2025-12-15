using UnityEditor;
using UnityEngine;
using Core.Enums;

namespace Core.Enums.Editor
{
    [CustomEditor(typeof(StringEnumLibrary))]
    public class StringEnumLibraryEditor : UnityEditor.Editor
    {
        private const string OutputPath = "Assets/Core/Data/StringEnums.generated.cs";
        private const string Namespace = "Core.Enums";

        private SerializedProperty groupsProp;
        private bool[] groupFoldouts;

        private void OnEnable()
        {
            groupsProp = serializedObject.FindProperty("groups");
            groupFoldouts = null;
        }

        public override void OnInspectorGUI()
        {
            var library = (StringEnumLibrary)target;

            serializedObject.Update();

            DrawGroupsSection();

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(8);

            DrawStatusPanel(library);
        }

        private void DrawGroupsSection()
        {
            if (groupsProp == null)
            {
                EditorGUILayout.HelpBox("groups property not found", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("Enum Groups", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Each group is Name:[Entries], e.g. Name = 'Ammo', Entries = ['Small','Medium','Large'].", MessageType.None);

            const int removeButtonWidth = 22;

            // Ensure foldout array size
            int size = groupsProp.arraySize;
            if (groupFoldouts == null || groupFoldouts.Length != size)
            {
                var newFoldouts = new bool[size];
                for (int i = 0; i < size; i++)
                    newFoldouts[i] = groupFoldouts != null && i < groupFoldouts.Length ? groupFoldouts[i] : true;
                groupFoldouts = newFoldouts;
            }

            for (int i = 0; i < groupsProp.arraySize; i++)
            {
                var groupProp  = groupsProp.GetArrayElementAtIndex(i);
                var keyProp    = groupProp.FindPropertyRelative("key");
                var valuesProp = groupProp.FindPropertyRelative("values");

                EditorGUILayout.BeginVertical("box");

                // Group header: [ foldout ][ name ][ - ]
                EditorGUILayout.BeginHorizontal();
                groupFoldouts[i] = EditorGUILayout.Foldout(groupFoldouts[i], GUIContent.none, true, EditorStyles.foldout);
                keyProp.stringValue = EditorGUILayout.TextField("Enum Name", keyProp.stringValue ?? string.Empty);
                if (GUILayout.Button("-", GUILayout.Width(removeButtonWidth)))
                {
                    groupsProp.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                if (groupFoldouts[i])
                {
                    // Entries list: flat rows, label them nicely.
                    if (valuesProp != null)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField("Entries");

                        for (int j = 0; j < valuesProp.arraySize; j++)
                        {
                            var valueProp = valuesProp.GetArrayElementAtIndex(j);

                            EditorGUILayout.BeginHorizontal();
                            valueProp.stringValue = EditorGUILayout.TextField(valueProp.stringValue ?? string.Empty);
                            if (GUILayout.Button("-", GUILayout.Width(removeButtonWidth)))
                            {
                                valuesProp.DeleteArrayElementAtIndex(j);
                                EditorGUILayout.EndHorizontal();
                                break;
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        if (GUILayout.Button("+ Add Entry", GUILayout.Width(110)))
                        {
                            int newValIndex = valuesProp.arraySize;
                            valuesProp.InsertArrayElementAtIndex(newValIndex);
                            valuesProp.GetArrayElementAtIndex(newValIndex).stringValue = string.Empty;
                        }
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
            }

            if (GUILayout.Button("+ Add Group", GUILayout.Height(22)))
            {
                int newIndex = groupsProp.arraySize;
                groupsProp.InsertArrayElementAtIndex(newIndex);
                var newGroup = groupsProp.GetArrayElementAtIndex(newIndex);
                newGroup.FindPropertyRelative("key").stringValue = string.Empty;
                var valuesProp = newGroup.FindPropertyRelative("values");
                if (valuesProp != null)
                {
                    valuesProp.ClearArray();
                }
            }
        }

        private void DrawStatusPanel(StringEnumLibrary library)
        {
            // Dirty indicator panel
            string currentSnapshot = StringEnumCodeGenerator.BuildSnapshot(library);
            bool isDirty = currentSnapshot != library.lastGeneratedSnapshot;

            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor = isDirty ? new Color(0.5f, 0.1f, 0.1f) : new Color(0.1f, 0.3f, 0.1f);

            EditorGUILayout.BeginVertical("box");
            GUI.backgroundColor = prevColor; // reset for inner controls

            string statusLabel = isDirty ? "Enum changes not saved" : "Enums are up to date";
            EditorGUILayout.LabelField(statusLabel, EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                isDirty
                    ? "Changes to keys are not yet reflected in generated code.\nClick 'Save / Generate' to update StringEnums.generated.cs and trigger a compile."
                    : "The generated StringEnums.generated.cs matches the current data.\nYou can still force regeneration if needed.",
                isDirty ? MessageType.Warning : MessageType.Info);

            if (GUILayout.Button("Save / Generate String Enum Constants", GUILayout.Height(26)))
            {
                StringEnumCodeGenerator.GenerateAndStamp(library, OutputPath, Namespace);
            }

            EditorGUILayout.EndVertical();
        }
    }
}
