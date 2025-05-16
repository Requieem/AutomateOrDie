using System.Collections;
using Code.Scripts.Common.MyGame.Extensions;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ImageFade : MonoBehaviour
{
    [SerializeField] private float m_fadeTime = 0.25f;
    [SerializeField] private Image m_image;

    private void Start()
    {
        FadeOut();
    }

    public void FadeIn()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(true));
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(false));
    }

    private IEnumerator Fade(bool fadeIn)
    {
        var color = m_image.color;
        var targetAlpha = fadeIn ? 1f : 0f;
        var startAlpha = color.a;
        var elapsedTime = 0f;
        while (elapsedTime < m_fadeTime)
        {
            elapsedTime += Time.deltaTime;
            var alpha = Mathf.Lerp(startAlpha, targetAlpha, (elapsedTime / m_fadeTime).EaseInOutQuad());
            color.a = alpha;
            m_image.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        m_image.color = color;
    }
}
