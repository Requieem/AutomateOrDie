using Code.Scripts.Runtime;
using UnityEngine;

namespace Code.Scripts.Editor
{
    using UnityEngine.UIElements;
    using UnityEditor;

    [CustomEditor(typeof(ToggleableUI))]
    public class ToggleableUIEditor : Editor
    {
        private int m_selectedState;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var toggleableUI = (ToggleableUI)target;

            if (GUILayout.Button("Toggle"))
            {
                toggleableUI.Move();
            }

            if (GUILayout.Button("Move Forward"))
            {
                toggleableUI.Move(true);
            }

            if (GUILayout.Button("Move Backward"))
            {
                toggleableUI.Move(false);
            }

            m_selectedState = EditorGUILayout.IntSlider("State", m_selectedState, 0, toggleableUI.Offsets.Length);
            if (GUILayout.Button("Move To State"))
            {
                toggleableUI.Move(m_selectedState);
            }
        }
    }
}