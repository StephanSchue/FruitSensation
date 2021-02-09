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

        private void Reset()
        {
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }

        public void UpdateText(string text)
        {
            textMeshProUGUI.text = text;
            textMeshProUGUI.transform.localScale = Vector3.one;
            PlayTextUpdateAnimation();
        }

        private void PlayTextUpdateAnimation()
        {
            textMeshProUGUI.DOFade(1f, 0.25f);
            textMeshProUGUI.transform.DOShakeScale(0.5f, 1f);
            textMeshProUGUI.transform.DOShakeRotation(0.5f);
            textMeshProUGUI.DOFade(0f, 0.25f).SetDelay(1f);
        }
    }
}