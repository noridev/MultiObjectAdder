using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class MultiObjectAdder : EditorWindow
{
    private const string Title = "NoriDev - Multi Object Adder";
    private string version = "";

    // D&D할 객체들과 그에 따른 경로 리스트
    private List<GameObject> targetObjects = new List<GameObject>();
    private List<Transform> targetPaths = new List<Transform>();  // 각 객체에 대한 경로를 저장할 리스트

    // 추가할 객체
    private GameObject objectToAdd;

    [MenuItem("Tools/_NORIDEV_/Multi Object Adder")]
    public static void ShowWindow()
    {
        MultiObjectAdder window = GetWindow<MultiObjectAdder>(Title);
        window.minSize = new Vector2(400, 300); // 최소 크기 설정
        window.LoadVersion(); // 버전 로드
    }

    private void LoadVersion()
    {
        string packageJsonPath = Path.Combine(Application.dataPath, "../Packages/moe.noridev.multi-object-adder/package.json");

        if (File.Exists(packageJsonPath))
        {
            string json = File.ReadAllText(packageJsonPath);
            var packageInfo = JsonUtility.FromJson<PackageInfo>(json);
            version = packageInfo.version;
        }
        else
        {
            Debug.LogWarning($"package.json not found at {packageJsonPath}");
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(Title, new GUIStyle(EditorStyles.boldLabel) { fontSize = 18 });
        GUILayout.FlexibleSpace();
        GUILayout.Label($"v{version}", new GUIStyle(EditorStyles.label) { fontSize = 13 });
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();

        // D&D하여 추가할 객체 리스트 표시
        GUILayout.Label("Drag and drop objects from the Hierarchy", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Drag and drop multiple objects here.", MessageType.Info, true);
        EditorGUILayout.Space();

        if (targetObjects.Count > 0)
        {
            if (GUILayout.Button("Clear All"))
            {
                targetObjects.Clear(); // 모든 객체 슬롯 제거
                targetPaths.Clear();   // 모든 경로 슬롯 제거
            }

            EditorGUILayout.Space();
        }

        // 추가된 객체 목록과 경로 선택 필드 및 제거 버튼
        EditorGUILayout.BeginVertical();
        for (int i = 0; i < targetObjects.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // D&D한 객체 표시
            targetObjects[i] = (GameObject)EditorGUILayout.ObjectField(targetObjects[i], typeof(GameObject), true, GUILayout.Width(position.width / 2 - 50));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Path", GUILayout.Width(40));
            targetPaths[i] = (Transform)EditorGUILayout.ObjectField(targetPaths[i], typeof(Transform), true);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Remove", GUILayout.Width(80)))
            {
                targetObjects.RemoveAt(i);
                targetPaths.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // D&D 필드
        GUIStyle centeredBoxStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter
        };

        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(new Rect(dropArea.x + 3, dropArea.y, position.width - 7, 50), "Drop Objects Here", centeredBoxStyle);

        if (dropArea.Contains(evt.mousePosition))
        {
            if (evt.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                evt.Use();
            }
            else if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (var draggedObject in DragAndDrop.objectReferences)
                {
                    if (draggedObject is GameObject)
                    {
                        targetObjects.Add((GameObject)draggedObject);
                        targetPaths.Add(null);  // 새로 추가된 객체에 대한 경로 리스트 초기화
                    }
                }
                evt.Use();
            }
        }

        EditorGUILayout.Space();

        // 추가할 객체 필드
        GUILayout.Label("Object to Add to All", EditorStyles.boldLabel);
        objectToAdd = (GameObject)EditorGUILayout.ObjectField("Object to Add", objectToAdd, typeof(GameObject), true);

        EditorGUILayout.Space();

        if (GUILayout.Button("Add Object to All"))
        {
            if (objectToAdd == null)
            {
                EditorUtility.DisplayDialog("Error", "You must assign an object to add.", "OK");
            }
            else
            {
                AddObjectToAll();
            }
        }
    }

    private void AddObjectToAll()
    {
        for (int i = 0; i < targetObjects.Count; i++)
        {
            GameObject target = targetObjects[i];
            Transform path = targetPaths[i];  // 각 객체에 대해 지정된 경로 가져오기

            if (target != null && objectToAdd != null)
            {
                // 지정된 경로에 객체 추가
                GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(objectToAdd);

                if (path != null)
                {
                    newObject.transform.SetParent(path);
                }
                else
                {
                    newObject.transform.SetParent(target.transform);
                }

                newObject.transform.localPosition = Vector3.zero;
                newObject.transform.localRotation = Quaternion.identity;
                newObject.transform.localScale = Vector3.one;

                Undo.RegisterCreatedObjectUndo(newObject, "Add Object to All");
            }
        }

        // Hierarchy 업데이트
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // package.json에 대한 클래스 정의
    [System.Serializable]
    private class PackageInfo
    {
        public string version;
    }
}
