using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace MAG.General.UI
{
    public class UIPreloadVisual : MonoBehaviour
    {
        public CanvasGroup canvasGroup;
        public TextMeshProUGUI progessTextObject;

        private ResourceManager resourceManager;
        private bool visible = false;

        private void Awake()
        {
            GameObject gameController = GameObject.FindGameObjectWithTag("GameController");

            if(gameController != null && gameController.TryGetComponent(out ResourceManager resourceManager))
            {
                this.resourceManager = resourceManager;
                this.resourceManager.onPreloadStart.AddListener(Show);
                this.resourceManager.onPreloadEnd.AddListener(Hide);
            }
        }

        private void Show()
        {
            canvasGroup.alpha = 1f;
            visible = true;
        }

        private void Update()
        {
            if(visible)
                progessTextObject.text = string.Format("{0}%", (int)(resourceManager.preloadProgress * 100f));
        }

        private void Hide()
        {
            canvasGroup.alpha = 0f;
            visible = false;
        }
    }
}