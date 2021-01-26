using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using TMPro;
using static MAG.General.EventDefinitions;

namespace MAG.General
{
    public class UIManager : MonoBehaviour
    {
        #region Definitions

        [System.Serializable]
        public struct UITextEvent
        {
            public string label;
            public StringEvent onChange;
        }

        [System.Serializable]
        public struct UIPanel
        {
            public string name;
            public CanvasGroup canvasGroup;
            public UIPanelButton[] buttons;
            public UITextEvent[] textOuput;

            public int buttonCount => buttons.Length;

            #region Panel Controls

            public void Show()
            {
                canvasGroup.interactable = canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;
            }

            public void Show(float duration, float delay = 0f)
            {
                canvasGroup.interactable = canvasGroup.blocksRaycasts = true;
                canvasGroup.DOFade(1f, duration).SetDelay(delay);
            }

            public void Hide()
            {
                canvasGroup.interactable = canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0f;
            }

            public void Hide(float duration, float delay = 0f)
            {
                canvasGroup.interactable = canvasGroup.blocksRaycasts = false;
                canvasGroup.DOFade(0f, duration).SetDelay(delay);
            }

            #endregion

            #region Button Controls

            public void RegisterButtonAction(UIButtonRegistationAction buttonRegistation)
            {
                if(FindButton(buttonRegistation.buttonName, out UIPanelButton button))
                    button.RegisterOnClickListener(buttonRegistation.callback);
            }

            private bool FindButton(string buttonName, out UIPanelButton button)
            {
                for(int i = 0; i < buttons.Length; i++)
                {
                    if(buttonName == buttons[i].name)
                    {
                        button = buttons[i];
                        return true;
                    }
                }

                button = new UIPanelButton();
                return false;
            }

            #endregion

            #region Text Output

            public void RaiseTextOutput(string label, string output)
            {
                if(FindTextOutput(label, out UITextEvent textOuput))
                    textOuput.onChange.Invoke(output);
            }

            private bool FindTextOutput(string label, out UITextEvent textEvent)
            {
                for(int i = 0; i < textOuput.Length; i++)
                {
                    if(label == textOuput[i].label)
                    {
                        textEvent = textOuput[i];
                        return true;
                    }
                }

                textEvent = new UITextEvent();
                return false;
            }

            #endregion
        }

        [System.Serializable]
        public struct UIPanelButton
        {
            public string name;
            public Button button;

            public void RegisterOnClickListener(UnityAction callback)
            {
                button.onClick.AddListener(callback);
            }
        }

        public struct UIPanelButtonsRegistation
        {
            public string panelName;
            public UIButtonRegistationAction[] buttonRegistationActions;

            public UIPanelButtonsRegistation(string panelName, UIButtonRegistationAction[] buttonRegistationActions)
            {
                this.panelName = panelName;
                this.buttonRegistationActions = buttonRegistationActions;
            }
        }

        public struct UIButtonRegistationAction
        {
            public string buttonName;
            public UnityAction callback;

            public UIButtonRegistationAction(string name, UnityAction callback)
            {
                this.buttonName = name;
                this.callback = callback;
            }
        }

        // --- Constants ---
        public const float CANVAS_FADEIN_DURATION = 1f;
        public const float CANVAS_FADEOUT_DURATION = 1f;
        
        public const float PANEL_FADEIN_DURATION = 0.25f;
        public const float PANEL_FADEOUT_DURATION = 0.25f;

        #endregion

        #region Settings/Variables

        [Header("References")]
        public CanvasGroup hudcanvasGroup;

        [Header("Panels")]
        public UIPanel[] uiPanels;

        // --- Variables ---
        private string currentPanel;

        #endregion

        private void Reset()
        {
            hudcanvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            Hide();
            HideAll();
        }

        #region General Canvas

        public void Show()
        {
            hudcanvasGroup.interactable = hudcanvasGroup.blocksRaycasts = true;
            hudcanvasGroup.alpha = 1f;
        }
        
        public void Show(float duration)
        {
            hudcanvasGroup.interactable = hudcanvasGroup.blocksRaycasts = true;
            hudcanvasGroup.DOFade(duration, 1f);
        }

        public void Hide()
        {
            hudcanvasGroup.interactable = hudcanvasGroup.blocksRaycasts = false;
            hudcanvasGroup.alpha = 0;
        }
        
        public void Hide(float duration)
        {
            hudcanvasGroup.interactable = hudcanvasGroup.blocksRaycasts = false;
            hudcanvasGroup.DOFade(duration, 1f);
        }

        public void HideAll()
        {
            for(int i = 0; i < uiPanels.Length; i++)
                uiPanels[i].Hide();
        }

        #endregion

        #region UI Panel Controls

        // --- Show ---
        private void ShowUIPanel(string panelLabel)
        {
            UIPanel panel = new UIPanel();

            if(FindUIPanel(panelLabel, ref panel))
                panel.Show();
        }
        
        private void ShowUIPanel(string panelLabel, float duration, float delay = 0f)
        {
            UIPanel panel = new UIPanel();

            if(FindUIPanel(panelLabel, ref panel))
                panel.Show(duration, delay);
        }

        // --- Hide ---
        private void HideUIPanel(string panelLabel)
        {
            UIPanel panel = new UIPanel();

            if(FindUIPanel(panelLabel, ref panel))
                panel.Hide();
        }
        
        private void HideUIPanel(string panelLabel, float duration, float delay = 0f)
        {
            UIPanel panel = new UIPanel();

            if(FindUIPanel(panelLabel, ref panel))
                panel.Hide(duration, delay);
        }

        // --- Utils ---
        private bool FindUIPanel(string panelLabel, ref UIPanel uiPanel)
        {
            for(int i = 0; i < uiPanels.Length; i++)
            {
                if(uiPanels[i].name == panelLabel)
                {
                    uiPanel = uiPanels[i];
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Buttons

        public void RegisterButtonActionsOnPanel(UIPanelButtonsRegistation buttonsRegistation)
        {
            UIPanel uiPanel = new UIPanel();

            if(FindUIPanel(buttonsRegistation.panelName, ref uiPanel))
            {
                for(int i = 0; i < buttonsRegistation.buttonRegistationActions.Length; i++)
                    uiPanel.RegisterButtonAction(buttonsRegistation.buttonRegistationActions[i]);
            }
        }

        #endregion

        #region MenuState

        public void ChangeUIPanel(string newState)
        {
            // --- Exit Old State ---
            string oldState = currentPanel;
            HideUIPanel(oldState, PANEL_FADEOUT_DURATION);

            // --- Enter New State ---
            currentPanel = newState;
            ShowUIPanel(newState, PANEL_FADEIN_DURATION);
        }

        #endregion

        #region Text Output

        public void RaiseTextOutput(string label, string value)
        {
            UIPanel uiPanel = new UIPanel();

            if(FindUIPanel(currentPanel, ref uiPanel))
                uiPanel.RaiseTextOutput(label, value);
        }

        #endregion
    }

}