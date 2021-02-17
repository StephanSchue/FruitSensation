using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MAG.General.Tweening;

namespace MAG.General.UI
{
    public sealed class UIAnimatedText : UITweenSequeneComponent
    {
        #region Components/Settings/Variables

        // --- Components/Settings ---
        [Header("Text Target")]
        public TextMeshProUGUI textMeshProUGUI;
        public bool cleanTextOnStartup = true;

        // --- Variables ---
        private Queue<string> queue = new Queue<string>();
        private bool fading = false;

        #endregion

        protected override void Reset()
        {
            base.Reset();
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }

        private void Awake()
        {
            if(cleanTextOnStartup)
                textMeshProUGUI.text = string.Empty;
        }

        #region Text Update

        public void UpdateText(string text)
        {
            if(fading)
                queue.Enqueue(text);
            else
                SetText(text);
        }

        private void SetText(string text)
        {
            fading = true;
            textMeshProUGUI.text = text;
            textMeshProUGUI.transform.rotation = Quaternion.identity;
            textMeshProUGUI.transform.localScale = Vector3.one;

            PlayTweenSequence(OnTextUpdateAnimationDone);
        }

        private void OnTextUpdateAnimationDone()
        {
            fading = false;

            if(queue.Count > 1)
                SetText(queue.Dequeue());
        }  

        #endregion
    }
}