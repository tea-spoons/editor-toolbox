
namespace TeaSpoons.EditorToolbox.Editor
{
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using System.Collections.Generic;

    /// <summary>
    /// An editor window that is basically a lightweight inspector for ScriptableObjects,
    /// but offers a search bar to filter its properties.
    /// </summary>
    internal class SOPropsWindow : EditorWindow
    {
        /// <summary>
        /// Some metadata of a <see cref="SerializedProperty"/>.
        /// The central data building block of the hierarchy represented in the window.
        /// </summary>
        private class PropertyInfo
        {
            public readonly PropertyInfo Parent;
            public readonly List<PropertyInfo> Children = new();

            public readonly string PropertyPath;
            public readonly string Name;
            public readonly int Depth;
            public readonly bool IsArray;
            public readonly bool IsGeneric;
            public bool IsRoot => string.IsNullOrEmpty(PropertyPath);

            public bool MatchesFilterDirectly { get; private set; }
            public bool HasMatchingChild { get; private set; }
            public bool HasMatchingDescendantOrSelf { get; private set; }
            public bool HasMatchingSibling => Parent != null && !Parent.IsRoot && Parent.IsGeneric && Parent.HasMatchingChild;

            public PropertyInfo(SerializedProperty property, PropertyInfo parent)
            {
                Parent = parent;
                PropertyPath = property.propertyPath;
                Name = property.displayName;
                Depth = property.depth;
                IsArray = property.isArray && property.propertyType != SerializedPropertyType.String;
                IsGeneric = property.propertyType == SerializedPropertyType.Generic && !IsArray;
            }

            /// <summary>
            /// Updates the state of the <see cref="PropertyInfo"/>
            /// regarding how well it matches the current search parameters.
            /// </summary>
            /// <param name="selectedObject">The <see cref="SerializedObject"/> to get the current values from.</param>
            public void UpdateMatchProperties(SerializedObject selectedObject, string filterString)
            {
                MatchesFilterDirectly = false;
                HasMatchingChild = false;
                HasMatchingDescendantOrSelf = false;

                if (!string.IsNullOrEmpty(PropertyPath) &&
                    !IsArray &&
                    !IsGeneric &&
                    (string.IsNullOrEmpty(filterString) ||
                    Name.ToLower().Contains(filterString) ||
                    ValueMatches(selectedObject, filterString)))
                {
                    MatchesFilterDirectly = true;
                    HasMatchingDescendantOrSelf = true;
                }

                HasMatchingChild = false;
                foreach (var child in Children)
                {
                    child.UpdateMatchProperties(selectedObject, filterString);
                    if (child.HasMatchingDescendantOrSelf)
                    {
                        HasMatchingDescendantOrSelf = true;
                    }
                    if (child.MatchesFilterDirectly)
                    {
                        HasMatchingChild = true;
                    }
                }
            }

            /// <summary>
            /// Draws the <see cref="SerializedProperty"/> represented by this object.
            /// </summary>
            public void Draw(SerializedObject selectedObject)
            {
                if (HasMatchingDescendantOrSelf || HasMatchingSibling)
                {
                    if (!string.IsNullOrEmpty(PropertyPath))
                    {
                        EditorGUI.indentLevel = Depth + 1;
                        if (MatchesFilterDirectly || HasMatchingSibling)
                        {
                            EditorGUILayout.PropertyField(selectedObject.FindProperty(PropertyPath), false);
                        }
                        else
                        {
                            EditorGUILayout.LabelField(Name);
                        }
                    }

                    foreach (var child in Children)
                    {
                        child.Draw(selectedObject);
                    }
                }
            }

            /// <summary>
            /// Returns <c>true</c> if the value of the <see cref="SerializedProperty"/> represented by this object
            /// matches the filter string.
            /// </summary>
            /// <param name="selectedObject">The <see cref="SerializedObject"/> to acquire the value from.</param>
            private bool ValueMatches(SerializedObject selectedObject, string filterString)
            {
                var property = selectedObject.FindProperty(PropertyPath);

                bool Matches(object obj)
                {
                    return obj.ToString().ToLower().Contains(filterString);
                }

                switch (property.propertyType)
                {
                    case SerializedPropertyType.Generic:
                        return false;
                    case SerializedPropertyType.Enum:
                        return Matches(property.enumDisplayNames[property.enumValueIndex]);
                    case SerializedPropertyType.Character:
                        return filterString.Length == 1 && (char)property.intValue == filterString[0];
                    case SerializedPropertyType.Vector2:
                        return Matches(property.vector2Value.x) ||
                            Matches(property.vector2Value.y);
                    case SerializedPropertyType.Vector3:
                        return Matches(property.vector3Value.x)
                            || Matches(property.vector3Value.y) ||
                            Matches(property.vector3Value.z);
                    case SerializedPropertyType.Vector4:
                        return Matches(property.vector4Value.x) || 
                           Matches(property.vector4Value.y) ||
                           Matches(property.vector4Value.z) ||
                           Matches(property.vector4Value.w);
                    case SerializedPropertyType.Vector2Int:
                        return Matches(property.vector2IntValue.x) ||
                            Matches(property.vector2IntValue.y);
                    case SerializedPropertyType.Vector3Int:
                        return Matches(property.vector3IntValue.x)
                            || Matches(property.vector3IntValue.y) ||
                            Matches(property.vector3IntValue.z);
                    case SerializedPropertyType.Boolean:
                        return (property.boolValue && filterString == "true") ||
                            (!property.boolValue && filterString == "false");
                    case SerializedPropertyType.ObjectReference:
                        var isNull = property.objectReferenceValue == null;
                        if (isNull)
                        {
                            return filterString == "null" || filterString == "none";
                        }
                        return Matches(property.objectReferenceValue.name);
                    default: return Matches(property.boxedValue);
                }
            }
        }

        private const string windowName = "SO Props";

        [MenuItem(PackageCore.Editor.Menus.RootItem + "Editor Toolbox/" + windowName)]
        private static void Open()
        {
            var window = GetWindow<SOPropsWindow>();
            window.titleContent = new GUIContent(windowName, GetEditorIcon("d_Search Icon"));
        }

        private SearchField searchField;
        private string filterString = "";
        private SerializedObject selectedSerializedObject;
        private ScriptableObject selectedObject;
        private PropertyInfo rootProperty;
        private Vector2 scrollPosition;

        private void OnEnable()
        {
            InitializeSearchField();
            ScanSelectedScriptableObject();
            Selection.selectionChanged += ScanSelectedScriptableObject;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= ScanSelectedScriptableObject;
        }

        private void OnFocus()
        {
            ScanSelectedScriptableObject();
        }

        private void OnGUI()
        {
            RecreateSerializdeObjectIfNeeded();

            if (selectedSerializedObject != null)
            {
                GUILayout.Box(selectedObject.name);

                DisplaySearchBar();
                GUILayout.Space(5);

                DisplayProperties();
            }
            else
            {
                EditorGUILayout.HelpBox("This window allows you to inspect a ScriptableObject and filter its properties using a search bar.\n" +
                    "You can search for both field names and values.\n" +
                    "Special values are 'true', 'false', 'null' and 'none'.\n" +
                    "\n" +
                    "Select a ScriptableObject to start.", MessageType.Info);
            }
        }

        private void RecreateSerializdeObjectIfNeeded()
        {
            if (selectedObject != null)
            {
                if (selectedSerializedObject == null || selectedSerializedObject.targetObject != selectedObject)
                {
                    selectedSerializedObject = new SerializedObject(selectedObject);
                }
            }
        }

        private void DisplaySearchBar()
        {
            EditorGUI.BeginChangeCheck();
            filterString = searchField.OnGUI(filterString);
            if (EditorGUI.EndChangeCheck())
            {
                rootProperty?.UpdateMatchProperties(selectedSerializedObject, filterString.ToLower());
            }
        }

        private void DisplayProperties()
        {
            selectedSerializedObject.Update();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUIStyle.none);
            rootProperty?.Draw(selectedSerializedObject);
            GUILayout.EndScrollView();

            selectedSerializedObject.ApplyModifiedProperties();
        }

        private void InitializeSearchField()
        {
            if (searchField != null) return;

            searchField = new SearchField();
        }

        private void ScanSelectedScriptableObject()
        {
            selectedObject = Selection.activeObject as ScriptableObject;
            if (!AssetDatabase.GetAssetPath(selectedObject).StartsWith("Assets/"))
            {
                selectedObject = null;
            }
            selectedSerializedObject = selectedObject != null ? new SerializedObject(selectedObject) : null;
            rootProperty = null;

            if (selectedSerializedObject == null)
            {
                Repaint();
                return;
            }

            var serializedProperty = selectedSerializedObject.GetIterator();
            rootProperty = new PropertyInfo(serializedProperty, null);

            var properties = new Stack<PropertyInfo>();
            properties.Push(rootProperty);

            while (serializedProperty.NextVisible(serializedProperty.propertyType == SerializedPropertyType.Generic))
            {
                if (serializedProperty.name == "m_Script") continue;

                while (serializedProperty.depth <= properties.Peek().Depth)
                {
                    properties.Pop();
                }

                var property = new PropertyInfo(serializedProperty, properties.Peek());
                properties.Peek().Children.Add(property);
                properties.Push(property);

                if (property.IsArray)
                {
                    // Go into the array
                    serializedProperty.Next(true);
                    // Skip the size property
                    serializedProperty.Next(true);
                }
            }

            rootProperty.UpdateMatchProperties(selectedSerializedObject, filterString.ToLower());

            Repaint();
        }

        private static Texture GetEditorIcon(string name)
        {
            return EditorGUIUtility.IconContent(name).image;
        }
    }
}
