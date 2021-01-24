using MAG.General;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MAG.Game.Core
{
    public class GameManager : MonoBehaviour
    {
        #region Definitions

        public enum ApplicationState
        {
            Preload,
            MainMenu,
            Game
        }

        public enum GamePhase
        {
            Select,
            Shift,
            Match,
            Refill,
            Pause,
            GameOver
        }

        #endregion

        #region Settings/Variables

        // --- References ---
        public ResourceManager resourceManager;
        public AssetReference[] gameScenes;

        // --- Variables ---
        // References
        private UIManager uiManager;
        private BoardManager boardManager;
        private InputManager inputManager;

        // States
        private ApplicationState applicationState;
        private GamePhase gamePhase;
        private GamePhase lastGamePhase;

        private AssetReference currentGameScene;

        #endregion

        // Start is called before the first frame update
        private void Start()
        {
            StartPreload();
        }

        // Update is called once per frame
        private void Update()
        {
            var dt = Time.deltaTime;
            
            switch(applicationState)
            {
                case ApplicationState.Preload:
                    ProcessPreloader(dt);
                    break;
                case ApplicationState.MainMenu:
                    ProcessMenu(dt);
                    break;
                case ApplicationState.Game:
                    ProcessGame(dt);
                    break;
                default:
                    break;
            }
        }

        #region MenuState

        public void ChangeApplicationState(ApplicationState newState)
        {
            // --- Exit Old State ---
            ApplicationState oldState = applicationState;

            switch(oldState)
            {
                case ApplicationState.Preload:
                    break;
                case ApplicationState.MainMenu:
                    HideMenu();
                    break;
                case ApplicationState.Game:
                    EndGame();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(oldState), oldState, null);
            }

            // --- Enter New State ---
            applicationState = newState;

            switch(newState)
            {
                case ApplicationState.Preload:
                    EndPreload();
                    break;
                case ApplicationState.MainMenu:
                    ShowMenu();
                    break;
                case ApplicationState.Game:
                    if(oldState == ApplicationState.MainMenu)
                        InitializeGame();
                    else
                        ShowGame();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        #endregion

        #region GamePhase

        public void ChangeGamePhase(GamePhase newPhase)
        {
            Debug.Log("ChangeGamePhase: " + newPhase);
            // --- Exit Old State ---
            GamePhase oldPhase = gamePhase;

            switch(oldPhase)
            {
                case GamePhase.Select:
                    break;
                case GamePhase.Shift:
                    break;
                case GamePhase.Match:
                    break;
                case GamePhase.Refill:
                    break;
                case GamePhase.Pause:
                    CallUnpause();
                    break;
                case GamePhase.GameOver:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(oldPhase), oldPhase, null);
            }

            // --- Enter New State ---
            gamePhase = newPhase;

            switch(newPhase)
            {
                case GamePhase.Select:
                    ShowGame();
                    break;
                case GamePhase.Shift:
                    ShowGame();
                    break;
                case GamePhase.Match:
                    ShowGame();
                    break;
                case GamePhase.Refill:
                    ShowGame();
                    break;
                case GamePhase.Pause:
                    CallPause();
                    break;
                case GamePhase.GameOver:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newPhase), newPhase, null);
            }
        }

        #endregion

        #region Preload State

        private void StartPreload()
        {
            resourceManager.LoadPreloadAssets(PreloadDone);
        }

        private void ProcessPreloader(float dt)
        {

        }

        private void EndPreload()
        {

        }

        private void PreloadDone()
        {
            uiManager = FindObjectOfType<UIManager>();

            // --- Button Registration ----
            uiManager.RegisterButtonActionsOnPanel(new UIManager.UIPanelButtonsRegistation("MainMenu",
                new UIManager.UIButtonRegistationAction[]
                {
                    new UIManager.UIButtonRegistationAction("Start", OnButtonStartClick),
                    new UIManager.UIButtonRegistationAction("Highscore", OnButtonHighscoreClick),
                }));

            uiManager.RegisterButtonActionsOnPanel(new UIManager.UIPanelButtonsRegistation("Game",
                new UIManager.UIButtonRegistationAction[]
                {
                    new UIManager.UIButtonRegistationAction("Menu", OnButtonPauseMenuClick),
                }));

            uiManager.RegisterButtonActionsOnPanel(new UIManager.UIPanelButtonsRegistation("Pause",
                new UIManager.UIButtonRegistationAction[]
                {
                    new UIManager.UIButtonRegistationAction("Resume", OnButtonPauseMenuResumeClick),
                    new UIManager.UIButtonRegistationAction("Restart", OnButtonPauseMenuRestartClick),
                    new UIManager.UIButtonRegistationAction("Exit", OnButtonPauseMenuExitClick),
                }));

            // --- Show UI ---
            uiManager.Show(UIManager.CANVAS_FADEIN_DURATION);

            // --- Initialize BoardMananger & InputManager ---
            boardManager = GetComponent<BoardManager>();

            inputManager = GetComponent<InputManager>();
            inputManager.InitializeBoardInput(boardManager.boardOrigin);
            inputManager.OnMouseDown.AddListener(boardManager.ProccessInput);

            // --- Move to MainMenu ---
            ChangeApplicationState(ApplicationState.MainMenu);
        }

        #endregion

        #region Menu State

        // --- Panel Functions ---
        private void ShowMenu()
        {
            uiManager.ChangeUIPanel("MainMenu");
        }

        private void ProcessMenu(float dt)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
                QuitGame();
        }

        private void HideMenu()
        {

        }

        // --- Button Actions ---
        private void OnButtonStartClick()
        {
            ChangeApplicationState(ApplicationState.Game);
        }

        private void OnButtonHighscoreClick()
        {

        }

        #endregion

        #region Game State

        // --- Panel Functions ---
        private void InitializeGame()
        {
            #warning Change direct reference to index
            currentGameScene = gameScenes[0];
            resourceManager.LoadScene(currentGameScene, InitializeGameComplete);
        }

        private void InitializeGameComplete()
        {
            ShowGame();
        }

        private void ShowGame()
        {
            uiManager.ChangeUIPanel("Game");
        }

        private void ProcessGame(float dt)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(gamePhase == GamePhase.Pause)
                    OnButtonPauseMenuResumeClick();
                else if(gamePhase != GamePhase.Pause)
                    OnButtonPauseMenuClick();
            }  
        }

        private void EndGame()
        {
            resourceManager.UnloadScene(currentGameScene, EndGameComplete);
        }

        private void EndGameComplete()
        {
            
        }

        // --- Button Actions ---
        private void OnButtonPauseMenuClick()
        {
            if(gamePhase != GamePhase.Pause)
                lastGamePhase = gamePhase;

            ChangeGamePhase(GamePhase.Pause);
        }

        private void OnButtonPauseMenuResumeClick()
        {
            ChangeGamePhase(lastGamePhase);
        }

        private void OnButtonPauseMenuRestartClick()
        {

        }

        private void OnButtonPauseMenuExitClick()
        {
            ChangeApplicationState(ApplicationState.MainMenu);
        }

        #endregion

        #region Pause

        public void CallPause()
        {
            uiManager.ChangeUIPanel("Pause");
        }

        private void CallUnpause()
        {
            
        }

        #endregion

        #region Quit

        public void QuitGame()
        {
            // save any game data here
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        #endregion
    }
}

