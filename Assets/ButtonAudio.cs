using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private AudioClip m_clickSound;
    [SerializeField] private AudioClip m_hoverSound;
    [SerializeField] private AudioClip m_unhoverSound;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Camera.main != null) AudioSource.PlayClipAtPoint(m_hoverSound, Camera.main.transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Camera.main != null) AudioSource.PlayClipAtPoint(m_unhoverSound, Camera.main.transform.position);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Camera.main != null) AudioSource.PlayClipAtPoint(m_clickSound, Camera.main.transform.position);
    }
}
