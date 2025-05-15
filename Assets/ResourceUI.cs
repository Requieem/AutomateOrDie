using UnityEngine;
using UnityEngine.UI;

public class ResourceUI : MonoBehaviour
{
    [SerializeField] private FillBar m_fillBar;
    [SerializeField] private Image m_resourceIcon;

    public void SetFill(float value)
    {
        m_fillBar.SetFill(value);
    }

    public void SetIcon(Sprite sprite)
    {
        m_resourceIcon.sprite = sprite;
    }
}
