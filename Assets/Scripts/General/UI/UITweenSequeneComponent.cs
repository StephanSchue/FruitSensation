using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace MAG.General.Tweening
{
    public class UITweenSequeneComponent : MonoBehaviour
    {
        [Header("Tween Targets")]
        public Graphic targetGraphic;
        public Transform targetTransform;

        [Header("Tween Sequence")]
        public TweenSequenceProfile animationProfile;

        protected virtual void Reset()
        {
            targetGraphic = GetComponent<Graphic>();
            targetTransform = GetComponent<Transform>();
        }

        #region Tween Method

        protected void PlayTweenSequence(TweenCallback onCompleteCallback)
        {
            if(animationProfile == null)
                return;

            int length = animationProfile.elements.Length;
            int lastIndex = animationProfile.elements.Length - 1;

            for(int i = 0; i < length; i++)
            {
                var animationElement = animationProfile.elements[i];

                // --- Define Callback ---
                TweenCallback _onCompleteCallback;

                if(i == lastIndex)
                    _onCompleteCallback = onCompleteCallback;
                else
                    _onCompleteCallback = null;

                // --- Execute Animation Node ---
                switch(animationElement.animationType)
                {
                    case AnimationType.Fade:
                        SetupFadeAnimation(targetGraphic, animationElement, onCompleteCallback);
                        break;
                    case AnimationType.Scale:
                        SetupScaleAnimation(targetTransform, animationElement, onCompleteCallback);
                        break;
                    case AnimationType.ShakeScale:
                        SetupShakeScaleAnimation(targetTransform, animationElement, onCompleteCallback);
                        break;
                    case AnimationType.ShakeRotation:
                        SetupShakeRotation(targetTransform, animationElement, onCompleteCallback);
                        break;
                }
            }
        }

        #endregion

        #region Tween Setup

        protected static void SetupFadeAnimation(Graphic target, TweenSequenceElement tweenAnimationElement, TweenCallback callback = null)
        {
            target.DOFade(tweenAnimationElement.GetParameterValue(0, 1f),
                tweenAnimationElement.duration).
                SetDelay(tweenAnimationElement.delay).
                SetEase(tweenAnimationElement.easeType).OnComplete(callback);
        }

        protected static void SetupScaleAnimation(Transform target, TweenSequenceElement tweenAnimationElement, TweenCallback callback = null)
        {
            target.DOScale(tweenAnimationElement.GetParameterValue(0, 1f),
                tweenAnimationElement.duration).SetDelay(tweenAnimationElement.delay).
                SetEase(tweenAnimationElement.easeType).OnComplete(callback);
        }

        protected static void SetupShakeScaleAnimation(Transform target, TweenSequenceElement tweenAnimationElement, TweenCallback callback = null)
        {
            target.DOShakeScale(tweenAnimationElement.duration,
                tweenAnimationElement.GetParameterValue(0, 1f), (int)tweenAnimationElement.GetParameterValue(1, 10f),
                tweenAnimationElement.GetParameterValue(2, 90f)).
                SetDelay(tweenAnimationElement.delay).
                SetEase(tweenAnimationElement.easeType).
                OnComplete(callback);
        }

        protected static void SetupShakeRotation(Transform target, TweenSequenceElement tweenAnimationElement, TweenCallback callback = null)
        {
            target.DOShakeRotation(tweenAnimationElement.duration,
                tweenAnimationElement.GetParameterValue(0, 1f), (int)tweenAnimationElement.GetParameterValue(1, 10f),
                tweenAnimationElement.GetParameterValue(2, 90f)).
                SetDelay(tweenAnimationElement.delay).
                SetEase(tweenAnimationElement.easeType).
                OnComplete(callback);
        }

        #endregion
    }
}