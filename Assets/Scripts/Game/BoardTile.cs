using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using DG.Tweening;
using MAG.Game.Tweening;

namespace MAG.Game
{
    public class BoardTile : MonoBehaviour
    {
        #region Settings/Variables

        [Header("Settings")]
        public int id;
        public Vector2 size;

        [Header("Components")]
        public SpriteRenderer overlayRenderer;

        [Header("Tweening")]
        public TileTweeningProfile tweeningProfile;

        // --- Variables ---
        private Color baseColor;

        private bool selected = false;
        private bool moving = false;

        // --- Events ---
        private UnityAction moveCallback;

        // --- Properties ---
        public bool IsMoving => moving;

        public Vector3 Position => transform.position; 

        #endregion


        private void Awake()
        {
            baseColor = overlayRenderer.color;
        }

        #region Select/Deselect

        public void Select()
        {
            selected = true;

            float duration = tweeningProfile.selectTweenSettings.duration;
            float delay = tweeningProfile.selectTweenSettings.delay;
            var easeType = tweeningProfile.selectTweenSettings.easeType;
            var color = tweeningProfile.selectTweenSettings.color;

            overlayRenderer.DOColor(color, duration).
                SetDelay(delay).SetEase(easeType);
        }

        public void Deselect()
        {
            selected = false;

            float duration = tweeningProfile.deselectTweenSettings.duration;
            float delay = tweeningProfile.deselectTweenSettings.delay;
            var easeType = tweeningProfile.deselectTweenSettings.easeType;
            var color = tweeningProfile.deselectTweenSettings.color;

            overlayRenderer.DOColor(color, duration).
                SetDelay(delay).SetEase(easeType);
        }
        
        #endregion

        #region Set Position

        public void SetPosition(Vector3 position, UnityAction callback = null)
        {
            moveCallback = callback;

            float duration = tweeningProfile.moveTweenSettings.duration;
            float delay = tweeningProfile.moveTweenSettings.delay;
            var easeType = tweeningProfile.moveTweenSettings.easeType;

            transform.DOMove(position, duration).
                SetDelay(delay).SetEase(easeType).
                OnComplete(SetPositionDone);
        }

        private void SetPositionDone()
        {
            moveCallback?.Invoke();
            moveCallback = null;
        } 

        #endregion
    }
}