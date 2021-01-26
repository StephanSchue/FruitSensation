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
            View,
            Select,
            Shift,
            Match,
            Refill,
            Pause,
            Restart,
            GameOver
        }

        public enum MatchResult
        {
            None,
            Win,
            Loose
        }

        #endregion

        #region Settings/Variables

        // --- References ---
        public bool ingameRepresentation = false;
        public ResourceManager resourceManager;
        public AssetReference[] gameScenes;

        // --- Variables ---
        // References
        private UIManager uiManager;
        private BoardManager boardManager;
        private InputManager inputManager;
        private CameraManager cameraManager;
        private SceneSettings sceneSettings;

        // States
        private ApplicationState applicationState;
        private GamePhase gamePhase;
        private GamePhase lastGamePhase;

        private AssetReference currentGameScene;

        // Gameplay
        private int selectedLevelIndex = -1;
        private MatchConditionsProfile matchConditionsProfile;
        private MatchWinCondition winCondition;
        private MatchResult matchResult;

        private int totalMoves = 0;
        private int _remainingMoves = 0;
        private int _score = 0;

        public int RemainingMoves
        {
            get { return _remainingMoves; }

            private set
            {
                _remainingMoves = value;
                OutputMoves();
            }
        }

        public int Score
        {
            get { return _score; }

            private set
            {
                _score = value;
                OutputScore();
            }
        }

        #endregion

        private void Awake()
        {
            // --- Clean Methods for GameScene Setup ----

            // GameController
            GameObject[] gameControllerInstances = GameObject.FindGameObjectsWithTag("GameController");
            
            if(gameControllerInstances.Length > 1)
                Destroy(gameObject);
            else
                resourceManager.InitializePreload();

            // Cameras
            GameObject[] mainCameraInstances = GameObject.FindGameObjectsWithTag("MainCamera");

            if(mainCameraInstances.Length > 1)
                Destroy(mainCameraInstances[1]);

            GameObject gameCameraInstance = GameObject.FindGameObjectWithTag("GameCamera");

            if(gameCameraInstance != null)
                Destroy(gameCameraInstance);
        }

        private void Start()
        {
            StartPreload();
        }

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
                        InitializeGameComplete();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        #endregion

        #region GamePhase

        public void ChangeGamePhase(GamePhase newPhase)
        {
            // --- Exit Old State ---
            GamePhase oldPhase = gamePhase;

            switch(oldPhase)
            {
                case GamePhase.View:
                    break;
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
                case GamePhase.Restart:
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
                case GamePhase.View:
                    break;
                case GamePhase.Select:
                    break;
                case GamePhase.Shift:
                    CallMatchNextStep();
                    break;
                case GamePhase.Match:
                    break;
                case GamePhase.Refill:
                    CallMatchNextStep();
                    break;
                case GamePhase.Pause:
                    CallPause();
                    break;
                case GamePhase.Restart:
                    CallRestart();
                    break;
                case GamePhase.GameOver:
                    //StartGameOver();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newPhase), newPhase, null);
            }
        }

        private void CallMatchNextStep()
        {
            if(CheckMatchConditions(out MatchResult matchResult))
            {
                this.matchResult = matchResult;
                ChangeGamePhase(GamePhase.GameOver);
            }
            else
            {
                ShowGame();
            }
        }

        private bool CheckMatchConditions(out MatchResult matchResult)
        {
            int winValue = winCondition.condition == WinCondition.Points ? Score : RemainingMoves;

            if(matchConditionsProfile.ValidateWinCondition(winValue))
            {
                // Check Win Condition
                matchResult = MatchResult.Win;
                return true;
            }
            else if(matchConditionsProfile.ValidateLooseCondition(RemainingMoves))
            {
                // Check loose Condition
                matchResult = MatchResult.Loose;
                return true;
            }

            matchResult = MatchResult.None;
            return false;
        }

        private void OnSelectTile()
        {
            
        }

        private void OnDeselectTile()
        {
            
        }

        private void OnSwapTile()
        {
            //Debug.Log("OnSwapTile");
            --RemainingMoves;
        }

        private void OnMatchTile(int tileCount)
        {
            Score += 100 * tileCount;

            if(CheckMatchConditions(out MatchResult matchResult))
            {
                this.matchResult = matchResult;
                ChangeGamePhase(GamePhase.GameOver);
            }
        }
        
        private void OnRefill()
        {
            //Debug.Log("OnRefill");
        }

        private void StartGameOver()
        {
            uiManager.ChangeUIPanel("GameOver");
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
                    new UIManager.UIButtonRegistationAction("Level01", OnButtonLevel01Click),
                    new UIManager.UIButtonRegistationAction("Level02", OnButtonLevel02Click),
                    new UIManager.UIButtonRegistationAction("Level03", OnButtonLevel03Click),
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
            inputManager.cameraReference = Camera.main;
            inputManager.OnMouseDown.AddListener(boardManager.ProccessInput);

            cameraManager = Camera.main.GetComponent<CameraManager>();

            // --- Move to MainMenu ---
            if(ingameRepresentation)
                ChangeApplicationState(ApplicationState.Game);
            else
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
        private void OnButtonLevel01Click()
        {
            selectedLevelIndex = 0;
            ChangeApplicationState(ApplicationState.Game);
        }

        private void OnButtonLevel02Click()
        {
            selectedLevelIndex = 1;
            ChangeApplicationState(ApplicationState.Game);
        }

        private void OnButtonLevel03Click()
        {
            selectedLevelIndex = 2;
            ChangeApplicationState(ApplicationState.Game);
        }

        #endregion

        #region Game State

        // --- Panel Functions ---
        private void InitializeGame()
        {
            currentGameScene = gameScenes[selectedLevelIndex];
            resourceManager.LoadScene(currentGameScene, InitializeGameComplete);
        }

        private void InitializeGameComplete()
        {
            // --- Get SceneSettings ---
            GameObject sceneSettingsObject = GameObject.FindGameObjectWithTag("SceneSettings");

            if(sceneSettingsObject != null && sceneSettingsObject.TryGetComponent(out SceneSettings sceneSettings))
            {
                this.sceneSettings = sceneSettings;

                boardManager.InitializeBoard(sceneSettings);
                inputManager.InitializeBoardInput(boardManager.boardOrigin);
                cameraManager.Initialize(sceneSettings.cameraSceneProfile);

                matchConditionsProfile = sceneSettings.boardProfile.matchConditions;
                winCondition = new MatchWinCondition(this.matchConditionsProfile.winCondtion);

                boardManager.OnSelectTile.AddListener(OnSelectTile);
                boardManager.OnDeselectTile.AddListener(OnDeselectTile);
                boardManager.OnSwapTile.AddListener(OnSwapTile);
                boardManager.OnMatch.AddListener(OnMatchTile);
                boardManager.OnRefill.AddListener(OnRefill);

                StartGame();
            }
        }

        private void StartGame()
        {
            ShowGame();

            Score = 0;
            RemainingMoves = totalMoves = matchConditionsProfile.moves;

            boardManager.CreateBoard();

            inputManager.SetInputActive(true);
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
                    OnButtonPauseMenuExitClick();
                else if(gamePhase != GamePhase.Pause)
                    OnButtonPauseMenuClick();
            }  
        }

        private void EndGame()
        {
            Score = RemainingMoves = 0;
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
            ChangeGamePhase(GamePhase.Restart);
        }

        private void OnButtonPauseMenuExitClick()
        {
            if(!ingameRepresentation)
                ChangeApplicationState(ApplicationState.MainMenu);
        }

        #endregion

        #region Pause

        public void CallPause()
        {
            uiManager.ChangeUIPanel("Pause");
            inputManager.SetInputActive(false);
        }

        private void CallUnpause()
        {
            inputManager.SetInputActive(true);
            ShowGame();
        }

        #endregion

        #region Restart

        private void CallRestart()
        {
            Score = 0;
            RemainingMoves = totalMoves;

            boardManager.RecreateBoard();
            ChangeGamePhase(GamePhase.View);
        }

        #endregion

        #region UI Events

        private void OutputScore()
        {
            if(winCondition.condition == WinCondition.Points)
                uiManager.RaiseTextOutput("Score", string.Format("{0}/{1}", _score, winCondition.value));
            else
                uiManager.RaiseTextOutput("Score", _score.ToString());
        }

        private void OutputMoves()
        {
            if(winCondition.condition == WinCondition.Endless)
                uiManager.RaiseTextOutput("Moves", Mathf.Abs(_remainingMoves).ToString());
            else
                uiManager.RaiseTextOutput("Moves", string.Format("{0}/{1}", _remainingMoves, totalMoves));
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

