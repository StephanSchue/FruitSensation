using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace MAG.General
{
    public class ResourceManager : MonoBehaviour
    {
        public AssetLabelReference preloader;

        // --- Preload Variables ---
        private GameObject preloadContainer;
        private AsyncOperationHandle<GameObject> preloaderCanvas;
        private AsyncOperationHandle<IList<Object>> preloadAsyncLoadOperations = new AsyncOperationHandle<IList<Object>>();
        private UnityAction preloadCallback;
        
        // --- Operations ---
        private List<AsyncOperationHandle<IList<Object>>> asyncLoadOperations = new List<AsyncOperationHandle<IList<Object>>>();

        // --- Events ---
        public UnityEvent onPreloadStart { get; private set; }
        public UnityEvent onPreloadEnd { get; private set; }

        // --- Progress ---
        public float preloadProgress => preloadAsyncLoadOperations.PercentComplete;

        #region Init

        private void Awake()
        {
            InitializePreload();
        }

        #endregion

        #region Preload

        private void InitializePreload()
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
            Debug.Log("LoadPreloadAssets: Start");

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
            Debug.Log("LoadPreloadAssets: Complete " + handle.Status.ToString());

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

            // --- Events ---
            if(preloadCallback != null)
                preloadCallback.Invoke();

            if(onPreloadEnd != null)
                onPreloadEnd.Invoke();
        }

        #endregion

        #region Load Assets

        private AsyncOperationHandle<IList<Object>> LoadAssets(AssetLabelReference assetReference)
        {
            Debug.Log("LoadAssets: " + assetReference.labelString);
            return Addressables.LoadAssetsAsync<Object>(assetReference, LoadAssetComplete);
        }

        private void LoadAssetComplete(Object loadedObject)
        {
            Debug.Log("LoadObject: " + loadedObject);
        } 

        #endregion
    }

    public static class ResourceLabels
    {
        // --- Scenes ----
        public const string RS_Game = "RS_Game";
    }
}