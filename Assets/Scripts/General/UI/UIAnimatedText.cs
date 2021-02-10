using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace MAG.General.UI
{
    public class UIAnimatedText : MonoBehaviour
    {
        public TextMeshProUGUI textMeshProUGUI;

        private Queue<string> queue = new Queue<string>();
        private bool fading = false;

        private void Reset()
        {
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }

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
            PlayTextUpdateAnimation();
        }

        private void PlayTextUpdateAnimation()
        {
            textMeshProUGUI.DOFade(1f, 0.25f);
            textMeshProUGUI.transform.DOShakeScale(0.5f, 1f);
            textMeshProUGUI.transform.DOShakeRotation(0.5f, 10, 5, 0);
            textMeshProUGUI.DOFade(0f, 0.25f).SetDelay(1f);
            textMeshProUGUI.transform.DOScale(1f, 0.25f).SetDelay(0.25f).OnComplete(OnTextUpdateAnimationDone);
        }

        private void OnTextUpdateAnimationDone()
        {
            fading = false;

            if(queue.Count > 1)
                SetText(queue.Dequeue());
        }
    }
}