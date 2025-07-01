using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Scripts.Runtime.UI
{
    public class ResourceUI : MonoBehaviour
    {
        [SerializeField] private FillBar m_fillBar;
        [SerializeField] private Image m_resourceIcon;
        [SerializeField] private Image m_background;
        [SerializeField] private TextMeshProUGUI m_infoText;
        [SerializeField] private bool m_showInfo;

        public bool ShowInfo => m_showInfo;

        public void OnEnable()
        {
            SetText(0);
        }

        public void SetFill(float value)
        {
            m_fillBar.SetFill(value);
        }

        public void SetBackgroundColor(Color color)
        {
            m_background.color = color;
        }

        public void SetBackground(Sprite sprite)
        {
            m_background.sprite = sprite;
        }

        public void SetIcon(Sprite sprite)
        {
            m_resourceIcon.sprite = sprite;
        }

        public void SetText(string msg)
        {
            m_infoText.text = !m_showInfo ? "" : msg;
        }

        public void SetText(float value)
        {
            SetText(value.ToString("N0"));
        }
    }
}
