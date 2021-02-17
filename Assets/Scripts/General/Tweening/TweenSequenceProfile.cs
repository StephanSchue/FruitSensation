using UnityEngine;

namespace MAG.General.Tweening
{
    [CreateAssetMenu(fileName = "TweenSequenceProfile_", menuName = "Configs/TweenSequenceProfile", order = 1)]
    public class TweenSequenceProfile : ScriptableObject
    {
        public TweenSequenceElement[] elements;
    }
}