using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace MAG.General
{
    public class ResourceManager : MonoBehaviour
    {
        public AssetLabelReference preloader;
        public Camera preloadCamera;

        // --- Preload Variables ---
        private GameObject preloadContainer;
        private AsyncOperationHandle<GameObject> preloaderCanvas;
        private AsyncOperationHandle<IList<Object>> preloadAsyncLoadOperations = new AsyncOperationHandle<IList<Object>>();
        private UnityAction preloadCallback;

        // --- Operations ---
        private List<AsyncOperationHandle<IList<Object>>> asyncLoadOperations = new List<AsyncOperationHandle<IList<Object>>>();

        private AsyncOperationHandle<SceneInstance> sceneLoadAsyncOperation;
        private AsyncOperationHandle<SceneInstance> sceneUnloadAsyncOperation;

        // --- Events ---
        public UnityEvent onPreloadStart { get; private set; }
        public UnityEvent onPreloadEnd { get; private set; }

        // --- Progress ---
        public float preloadProgress => preloadAsyncLoadOperations.PercentComplete;

        #region Preload

        public void InitializePreload()
        {
            preloadContainer = new GameObject("PreloadAssets");
            onPreloadStart = new UnityEvent();
            onPreloadEnd = new UnityEvent();

            preloaderCanvas = Addressables.InstantiateAsync("UI_PreloadCanvas");
        }

        private void DeinitializePreload()
        {
            Addressables.ReleaseInstance(preloaderCanvas);
        }

        public void LoadPreloadAssets(UnityAction callback)
        {
            //Debug.Log("LoadPreloadAssets: Start");

            if(onPreloadStart != null)
                onPreloadStart.Invoke();

            AsyncOperationHandle<IList<Object>> preloadOperations = LoadAssets(preloader);
            preloadAsyncLoadOperations = preloadOperations;
            asyncLoadOperations.Add(preloadOperations);

            preloadCallback = callback;
            preloadOperations.Completed += LoadPreloadAssetsComplete;
        }

        private void LoadPreloadAssetsComplete(AsyncOperationHandle<IList<Object>> handle)
        {
            //Debug.Log("LoadPreloadAssets: Complete " + handle.Status.ToString());

            // --- Instantiate Objects ---
            if(handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach(var item in handle.Result)
                {
                    if(item is GameObject gameObjectItem)
                    {
                        GameObject instance = Instantiate(gameObjectItem, preloadContainer.transform);
                        instance.name = item.name;
                    }
                }
            }

            // --- Remove Operations ---
            asyncLoadOperations.Remove(preloadAsyncLoadOperations);

            // --- Remove Preloader Canvas ---
            DeinitializePreload();

            if(preloadCamera != null)
                preloadCamera.enabled = false;

            // --- Events ---
            preloadCallback?.Invoke();
            onPreloadEnd?.Invoke();
        }

        #endregion

        #region Load Assets

        private AsyncOperationHandle<IList<Object>> LoadAssets(AssetLabelReference assetReference)
        {
            //Debug.Log("LoadAssets: " + assetReference.labelString);
            return Addressables.LoadAssetsAsync<Object>(assetReference, LoadAssetComplete);
        }

        private void LoadAssetComplete(Object loadedObject)
        {
            //Debug.Log("LoadObject: " + loadedObject);
        }

        #endregion

        #region Load Scene

        public void LoadScene(AssetReference sceneReference, UnityAction callback)
        {
            sceneLoadAsyncOperation = Addressables.LoadSceneAsync(sceneReference, LoadSceneMode.Additive);
            sceneLoadAsyncOperation.Completed += (obj) => LoadSceneComplete(obj, callback);
        }

        private void LoadSceneComplete(AsyncOperationHandle<SceneInstance> obj, UnityAction callback)
        {
            switch(obj.Status)
            {
                case AsyncOperationStatus.Succeeded:
                    //Debug.LogFormat("'{0}' - LoadSceneComplete: successfully loaded!", obj.DebugName);
                    break;
                case AsyncOperationStatus.Failed:
                    //Debug.LogErrorFormat("'{0}' - LoadSceneComplete: failed load!", obj.DebugName);
                    break;
            }

            callback?.Invoke();
        }

        public void UnloadScene(AssetReference sceneReference, UnityAction callback)
        {
            sceneUnloadAsyncOperation = Addressables.UnloadSceneAsync(sceneLoadAsyncOperation);
            sceneUnloadAsyncOperation.Completed += (obj) => UnloadSceneComplete(obj, callback);
        }

        private void UnloadSceneComplete(AsyncOperationHandle<SceneInstance> obj, UnityAction callback)
        {
            switch(obj.Status)
            {
                case AsyncOperationStatus.Succeeded:
                    //Debug.LogFormat("UnloadSceneComplete: successfully loaded!");
                    break;
                case AsyncOperationStatus.Failed:
                    //Debug.LogErrorFormat("UnloadSceneComplete: failed load!");
                    break;
            }

            callback?.Invoke();
        }

        #endregion
    }

    public static class ResourceLabels
    {
        // --- Scenes ----
        public const string RS_Game = "RS_Game";
    }
}