using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Code.Scripts.Runtime.UI
{
    public class FillBar : MonoBehaviour
    {
        [FormerlySerializedAs("m_fillbar")] [SerializeField] private Image m_fillBar;
        [FormerlySerializedAs("m_fillbarBackground")] [SerializeField] private Image m_fillBarBackground;
        [SerializeField] private Gradient m_gradient;
        [SerializeField] private float m_animationTime = 0.15f;

        public void SetFill(float value)
        {
            if (!gameObject.activeInHierarchy || Mathf.Approximately(m_fillBar.fillAmount, value)) return;
            StopAllCoroutines();
            StartCoroutine(FillbarAnimation(value));
        }

        private IEnumerator FillbarAnimation(float value)
        {
            var time = 0f;
            var startValue = m_fillBar.fillAmount;
            while (time < m_animationTime)
            {
                time += Time.deltaTime;
                var t = Mathf.Clamp01(time / m_animationTime);
                m_fillBar.fillAmount = Mathf.Lerp(startValue, value, t);
                m_fillBar.color = m_gradient.Evaluate(m_fillBar.fillAmount);
                m_fillBarBackground.color = m_gradient.Evaluate(m_fillBar.fillAmount);
                yield return null;
            }
        }
    }
}
