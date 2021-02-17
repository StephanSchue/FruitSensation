using DG.Tweening;
using UnityEngine;

namespace MAG.General.Tweening
{
    public enum AnimationType
    {
        Fade,
        Scale,
        ShakeScale,
        ShakeRotation,
    }

    [System.Serializable]
    public struct KeyValuePairFloat
    {
        public string label;
        public float value;
    }

    [System.Serializable]
    public struct KeyValuePairVector3
    {
        public string label;
        public Vector3 value;
    }

    [System.Serializable]
    public struct TweenSequenceElement
    {
        public string label;
        public AnimationType animationType;
        public Ease easeType;

        [Header("Parameters")]
        public KeyValuePairFloat[] parameters;

        [Header("Timing")]
        public float duration;
        public float delay;

        public float GetParameterValue(int index, float defaultValue)
        {
            if(index > -1 && index < parameters.Length)
                return parameters[index].value;
            else
                return defaultValue;
        }
    }

    [System.Serializable]
    public struct TweenBaseSettingsElement
    {
        public Ease easeType;
        
        [Header("Timing")]
        public float duration;
        public float delay;
    }

    [System.Serializable]
    public struct TweenColorSettingsElement
    {
        public Ease easeType;

        public Color color;

        [Header("Timing")]
        public float duration;
        public float delay;
    }
}