using UnityEngine;

namespace Code.Scripts.Common
{
    namespace MyGame.Extensions
{
    public enum EaseType
    {
        Linear,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InBack,
        OutBack,
        InOutBack,
        InBounce,
        OutBounce,
        InOutBounce
    }

    public static class Easing
    {
        public static float Ease(this float t, EaseType easeType)
        {
            switch (easeType)
            {
                case EaseType.InQuad:
                    return t.EaseInQuad();
                case EaseType.OutQuad:
                    return t.EaseOutQuad();
                case EaseType.InOutQuad:
                    return t.EaseInOutQuad();
                case EaseType.InCubic:
                    return t.EaseInCubic();
                case EaseType.OutCubic:
                    return t.EaseOutCubic();
                case EaseType.InOutCubic:
                    return t.EaseInOutCubic();
                case EaseType.InBack:
                    return t.EaseInBack();
                case EaseType.OutBack:
                    return t.EaseOutBack();
                case EaseType.InOutBack:
                    return t.EaseInOutBack();
                case EaseType.InBounce:
                    return t.EaseInBounce();
                case EaseType.OutBounce:
                    return t.EaseOutBounce();
                case EaseType.InOutBounce:
                    return t.EaseInOutBounce();
                case EaseType.Linear:
                default:
                    return t.EaseLinear();
            }
        }

        private static float Clamp01(this float t) => Mathf.Clamp01(t);

        // Linear
        public static float EaseLinear(this float t) => t.Clamp01();

        // Quadratic
        public static float EaseInQuad(this float t)
        {
            t = t.Clamp01();
            return t * t;
        }

        public static float EaseOutQuad(this float t)
        {
            t = t.Clamp01();
            return 1f - (1f - t) * (1f - t);
        }

        public static float EaseInOutQuad(this float t)
        {
            t = t.Clamp01();
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        // Cubic
        public static float EaseInCubic(this float t)
        {
            t = t.Clamp01();
            return t * t * t;
        }

        public static float EaseOutCubic(this float t)
        {
            t = t.Clamp01();
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        public static float EaseInOutCubic(this float t)
        {
            t = t.Clamp01();
            return t < 0.5f
                ? 4f * t * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        // Back
        public static float EaseInBack(this float t)
        {
            t = t.Clamp01();
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return c3 * t * t * t - c1 * t * t;
        }

        public static float EaseOutBack(this float t)
        {
            t = t.Clamp01();
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        public static float EaseInOutBack(this float t)
        {
            t = t.Clamp01();
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;

            return t < 0.5f
                ? (Mathf.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2)) / 2f
                : (Mathf.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (2f * t - 2f) + c2) + 2f) / 2f;
        }

        // Bounce Helpers
        private static float EaseOutBounceHelper(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            return t switch
            {
                < 1f / d1 => n1 * t * t,
                < 2f / d1 => n1 * (t -= 1.5f / d1) * t + 0.75f,
                < 2.5f / d1 => n1 * (t -= 2.25f / d1) * t + 0.9375f,
                _ => n1 * (t -= 2.625f / d1) * t + 0.984375f
            };
        }

        private static float EaseInBounceHelper(float t)
        {
            return 1f - EaseOutBounce(1f - t);
        }

        // Bounce
        public static float EaseInBounce(this float t)
        {
            t = t.Clamp01();
            return EaseInBounceHelper(t);
        }

        public static float EaseOutBounce(this float t)
        {
            t = t.Clamp01();
            return EaseOutBounceHelper(t);
        }

        public static float EaseInOutBounce(this float t)
        {
            t = t.Clamp01();
            return t < 0.5f
                ? (1f - EaseOutBounce(1f - 2f * t)) * 0.5f
                : (1f + EaseOutBounce(2f * t - 1f)) * 0.5f;
        }
    }
}
}