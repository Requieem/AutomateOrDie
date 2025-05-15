using UnityEditor;
using UnityEngine;

namespace Code.Scripts.Editor
{
    [CustomEditor(typeof(FillBar))]
    public class FillBarEditor : UnityEditor.Editor
    {
        private float m_fillAmount = 0.5f;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            m_fillAmount = EditorGUILayout.Slider("Fill Amount", m_fillAmount, 0f, 1f);

            if (!GUILayout.Button("Fill")) return;

            var fillBar = (FillBar)target;
            fillBar.SetFill(m_fillAmount);
        }
    }
}