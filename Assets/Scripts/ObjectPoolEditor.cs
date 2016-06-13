#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(ObjectPool))]
public class ObjectPoolEditor : Editor {

	// Variables
	private ReorderableList thePool;

	void OnEnable() {
		SerializedProperty objectPool = serializedObject.FindProperty("Entries");
		
		thePool = new ReorderableList(
			serializedObject,
			objectPool,
			true,
			true,
			true,
			true
			);
		
		thePool.drawHeaderCallback = (Rect rect) => {  
			EditorGUI.LabelField(rect, "Pooled Objects");
		};
		
		thePool.drawElementCallback =  
		(rect, index, active, isFocused) => {
			SerializedProperty element = thePool.serializedProperty.GetArrayElementAtIndex(index);
			thePool.elementHeight = EditorGUIUtility.singleLineHeight * 1.25f;

			rect.y += 2;
			
			// Prefab
			EditorGUI.LabelField(
				new Rect(rect.x, rect.y, 40, EditorGUIUtility.singleLineHeight), "Prefab:");
			EditorGUI.PropertyField(
				new Rect(rect.x + 45, rect.y, 130, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative("Prefab"), GUIContent.none);

			// Amount
			EditorGUI.LabelField(
				new Rect(rect.x + 185, rect.y, 50, EditorGUIUtility.singleLineHeight), "Count:");
			EditorGUI.PropertyField(
				new Rect(rect.x + 230, rect.y, 25, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative("Amount"), GUIContent.none);
			
		};
		
		thePool.onReorderCallback = (ReorderableList theList) => {
//			updated = true;
		};
		
		thePool.onRemoveCallback = (ReorderableList l) => {  
			if (EditorUtility.DisplayDialog("Warning!", 
			                                "Are you sure you want to delete this?", "Yes", "No")) {
				ReorderableList.defaultBehaviours.DoRemoveButton(l);
			}
		};
	}
		

	public override void OnInspectorGUI() {
		ObjectPool myTarget = (ObjectPool)target;
		
		// Regular inspector
		myTarget.GrowIfUnavailable = EditorGUILayout.Toggle("Grow If Unavailable:", myTarget.GrowIfUnavailable);
		myTarget.containerSuffix = EditorGUILayout.TextField("Object Suffix:", myTarget.containerSuffix);
		
		// Use this to draw the original inspector layout
		//		DrawDefaultInspector();
		
		// Still not sure what this does...
		serializedObject.Update();
		
		// This draws the same list as defined by our custom editor parameters
		thePool.DoLayoutList();
		
		// This allows us to change properties (like adding/removing elements)
		serializedObject.ApplyModifiedProperties();

	}
}
	#endif