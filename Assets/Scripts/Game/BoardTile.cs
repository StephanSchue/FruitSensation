using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using DG.Tweening;

namespace MAG.Game
{
    public class BoardTile : MonoBehaviour
    {
		public int id;
        public Vector2 size;
		public SpriteRenderer overlayRenderer;

        private Color baseColor;
		private float duration = 0.25f;

		private bool selected = false;
		private bool moving = false;

		private UnityAction moveCallback;
		public bool IsMoving => moving;

		public Vector3 Position => transform.position;

        private void Awake()
        {
            baseColor = overlayRenderer.color;
        }

        #region Select/Deselect

        public void Select()
        {
            selected = true;
            overlayRenderer.color = new Color(1f, 1f, 0f, 0.5f);
        }

        public void Deselect()
        {
            selected = false;
            overlayRenderer.color = baseColor;
        }
        
        #endregion

        #region Set Position

        public void SetPosition(Vector3 position, UnityAction callback = null)
        {
            moveCallback = callback;
            transform.DOMove(position, duration).OnComplete(SetPositionDone);
        }

        private void SetPositionDone()
        {
            if(moveCallback != null)
            {
                moveCallback.Invoke();
                moveCallback = null;
            }
        } 

        #endregion
    }
}