using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ElectrumDebugBehavior : MonoBehaviour
{
    public Character testCharacter;
    public void MakeTestCharacterChooseAction(Character character)
    {
        character.ChooseAction();
    }
}

[CustomEditor(typeof(ElectrumDebugBehavior))]
public class ElectrumDebugEditor : Editor
{
    SerializedProperty testCharacter;

    private void OnEnable()
    {
        testCharacter = serializedObject.FindProperty("testCharacter");
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        if (!Application.isPlaying) return;
        ElectrumDebugBehavior script = (ElectrumDebugBehavior)target;
        if (GUILayout.Button("Debug choose action"))
        {
            script.MakeTestCharacterChooseAction(script.testCharacter);
        }
    }
}
