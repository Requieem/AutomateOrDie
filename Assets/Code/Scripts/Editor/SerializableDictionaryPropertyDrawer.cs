using Code.Scripts.Common;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace Code.Scripts.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public class SerializableDictionaryPropertyDrawer : PropertyDrawer
    {
        private int m_selectedIndex = -1;
        private bool m_showEntries = false;
        public VisualTreeAsset TreeAsset;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement
            {
                name = "Root",
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginBottom = 4,
                    paddingTop = 4,
                    paddingBottom = 4,
                    paddingLeft = 4,
                    paddingRight = 4,
                }
            };

            var headerContainer = new VisualElement
            {
                name = "HeaderContainer",
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                    marginBottom = 4,
                }
            };

            var headerLabel = new Label(property.displayName)
            {
                style =
                {
                    fontSize = 14,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 4,
                }
            };

            var headerToggle = new Toggle("Show Entries")
            {
                value = m_showEntries,
                style =
                {
                    marginBottom = 4,
                    marginLeft = 4,
                    marginRight = 4,
                    unityTextAlign = TextAnchor.MiddleRight,
                }
            };

            headerContainer.Add(headerLabel);
            headerContainer.Add(headerToggle);
            root.Add(headerContainer);

            var container = new VisualElement
            {
                name = "Container",
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginBottom = 4,
                    borderBottomWidth = 1,
                    borderBottomColor = new Color(0.8f, 0.8f, 0.8f),
                    borderTopColor = new Color(0.8f, 0.8f, 0.8f),
                    borderTopWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderLeftColor = new Color(0.8f, 0.8f, 0.8f),
                    borderRightColor = new Color(0.8f, 0.8f, 0.8f),
                    borderBottomLeftRadius = 5,
                    borderBottomRightRadius = 5,
                    borderTopLeftRadius = 5,
                    borderTopRightRadius = 5,
                    paddingTop = 2,
                    paddingBottom = 5,
                    paddingLeft = 2,
                    paddingRight = 2,
                }
            };
            container.style.marginBottom = 4;
            root.Add(container);

            var keysProp = property.FindPropertyRelative("m_keys");
            var valuesProp = property.FindPropertyRelative("m_values");

            var listContainer = new VisualElement
            {
                name = "ListContainer",
                style = { flexDirection = FlexDirection.Column }
            };

            container.Add(listContainer);

            void RebuildList()
            {
                listContainer.Clear();

                int count = Mathf.Min(keysProp.arraySize, valuesProp.arraySize);
                for (int i = 0; i < count; i++)
                {
                    var entryRoot = TreeAsset.CloneTree();
                    int currentIndex = i;

                    var keyProp = keysProp.GetArrayElementAtIndex(i);
                    var valueProp = valuesProp.GetArrayElementAtIndex(i);

                    var label = entryRoot.Q<Label>("Label");
                    label.text = $"Entry {i}";

                    var keyField = entryRoot.Q<PropertyField>("KeyProperty");
                    keyField.BindProperty(keyProp);
                    keyField.label = $"Key {i}";

                    var valueField = entryRoot.Q<PropertyField>("ValueProperty");
                    valueField.BindProperty(valueProp);
                    valueField.label = $"Value {i}";

                    var removeButton = entryRoot.Q<Button>("RemoveButton");
                    removeButton.clicked += () =>
                    {
                        m_selectedIndex = currentIndex;
                        if (m_selectedIndex < 0 || m_selectedIndex >= keysProp.arraySize)
                            return;

                        keysProp.DeleteArrayElementAtIndex(m_selectedIndex);
                        valuesProp.DeleteArrayElementAtIndex(m_selectedIndex);
                        m_selectedIndex = -1;

                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();
                        RebuildList();
                    };

                    listContainer.Add(entryRoot);
                }
            }

            RebuildList();

            var buttonRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                    marginTop = 4,
                }
            };

            var addButton = new Button(() =>
            {
                property.serializedObject.Update();

                object newKey = GenerateDefaultKey();
                if (newKey == null)
                {
                    Debug.LogWarning("[SerializableDictionary] Unable to generate default key.");
                    return;
                }

                if (IsDuplicateKey(keysProp, newKey))
                {
                    EditorUtility.DisplayDialog("Duplicate Key", "That key already exists in the dictionary.", "OK");
                    return;
                }

                keysProp.arraySize++;
                valuesProp.arraySize++;

                var newKeyProp = keysProp.GetArrayElementAtIndex(keysProp.arraySize - 1);
                var newValueProp = valuesProp.GetArrayElementAtIndex(valuesProp.arraySize - 1);

                SetSerializedValue(newKeyProp, newKey);

                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                RebuildList();
            })
            {
                text = "Add Entry"
            };

            buttonRow.Add(addButton);
            container.Add(buttonRow);

            headerToggle.RegisterValueChangedCallback(evt =>
            {
                m_showEntries = evt.newValue;
                container.style.display = m_showEntries ? DisplayStyle.Flex : DisplayStyle.None;
            });

            container.style.display = m_showEntries ? DisplayStyle.Flex : DisplayStyle.None;

            return root;
        }

        private object GenerateDefaultKey()
        {
            var keyType = fieldInfo.FieldType.GetGenericArguments()[0];

            if (keyType == typeof(string))
                return "NewKey";
            if (keyType.IsValueType)
                return Activator.CreateInstance(keyType);

            return null;
        }

        private bool IsDuplicateKey(SerializedProperty keysProp, object newKey)
        {
            for (int i = 0; i < keysProp.arraySize; i++)
            {
                var keyProp = keysProp.GetArrayElementAtIndex(i);
                if (SerializedValueEquals(keyProp, newKey))
                    return true;
            }
            return false;
        }

        private void SetSerializedValue(SerializedProperty prop, object value)
        {
            if (value == null)
            {
                prop.objectReferenceValue = null;
                return;
            }

            switch (prop.propertyType)
            {
                case SerializedPropertyType.String:
                    prop.stringValue = (string)value;
                    break;
                case SerializedPropertyType.Integer:
                    prop.intValue = Convert.ToInt32(value);
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = (bool)value;
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = Convert.ToSingle(value);
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = (UnityEngine.Object)value;
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = (Vector2)value;
                    break;
                default:
                    Debug.LogWarning($"Unsupported type: {prop.propertyType}");
                    break;
            }
        }

        private bool SerializedValueEquals(SerializedProperty prop, object value)
        {
            if (value == null)
                return prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue == null;

            switch (prop.propertyType)
            {
                case SerializedPropertyType.String:
                    return prop.stringValue == (string)value;
                case SerializedPropertyType.Integer:
                    return prop.intValue == Convert.ToInt32(value);
                case SerializedPropertyType.Boolean:
                    return prop.boolValue == (bool)value;
                case SerializedPropertyType.Float:
                    return Mathf.Approximately(prop.floatValue, Convert.ToSingle(value));
                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue == (UnityEngine.Object)value;
                default:
                    return false;
            }
        }
    }
}