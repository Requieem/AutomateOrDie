using System.Collections;
using Code.Scripts.Runtime.GameResources;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Image m_background;
    [SerializeField] private GameObject m_content;
    [SerializeField] private InputActionReference m_pauseAction;

    private void OnEnable()
    {
        m_pauseAction.action.performed += OnPause;
    }

    private void OnDisable()
    {
        m_pauseAction.action.performed -= OnPause;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        Toggle(true);
        Time.timeScale = 0;
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        if (GameState.Instance.Lost) return;
        if(!context.performed) return;
        Toggle(!m_content.activeSelf);
        Time.timeScale = m_content.activeSelf ? 0 : 1;
    }

    public void Toggle(bool show)
    {
        m_background.color = show ? Color.white : Color.clear;
        m_content.SetActive(show);
    }
}
