using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace MAG.General
{
    public class Preloader : MonoBehaviour
    {
        public AssetReference gameScene;

        private AsyncOperationHandle<SceneInstance> gameSceneLoadAsyncOperation;
        private AsyncOperationHandle<SceneInstance> gameSceneUnloadAsyncOperation;

        public void Start()
        {
            LoadGameScene();
        }

        private void LoadGameScene()
        {
            gameSceneLoadAsyncOperation = Addressables.LoadSceneAsync(gameScene, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            gameSceneLoadAsyncOperation.Completed += LoadGameSceneComplete;
        }

        private void LoadGameSceneComplete(AsyncOperationHandle<SceneInstance> obj)
        {
            switch(obj.Status)
            {
                case AsyncOperationStatus.Succeeded:
                    Debug.LogFormat("LoadGameSceneComplete: successfully loaded!");
                    break;
                case AsyncOperationStatus.Failed:
                    Debug.LogErrorFormat("LoadGameSceneComplete: failed load!");
                    break;
            }
        }

        private void UnloadGameScene()
        {
            gameSceneUnloadAsyncOperation = Addressables.UnloadSceneAsync(gameSceneLoadAsyncOperation);
            gameSceneUnloadAsyncOperation.Completed += UnloadGameSceneComplete;
        }

        private void UnloadGameSceneComplete(AsyncOperationHandle<SceneInstance> obj)
        {
            switch(obj.Status)
            {
                case AsyncOperationStatus.Succeeded:
                    Debug.LogFormat("LoadGameSceneComplete: successfully loaded!");
                    break;
                case AsyncOperationStatus.Failed:
                    Debug.LogErrorFormat("LoadGameSceneComplete: failed load!");
                    break;
            }
        }
    }
}