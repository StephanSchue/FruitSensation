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

        private GameObject preloadContainer;
        private List<AsyncOperationHandle<IList<Object>>> asyncLoadOperations = new List<AsyncOperationHandle<IList<Object>>>();
        private UnityAction preloadCallback;

        private void Awake()
        {
            InitializePreload();
        }

        #region Preload

        private void InitializePreload()
        {
            preloadContainer = new GameObject("PreloadAssets");
        }

        public void LoadPreloadAssets(UnityAction callback)
        {
            Debug.Log("LoadPreloadAssets: Start");
            AsyncOperationHandle<IList<Object>> preloadOperations = LoadAssets(preloader);
            asyncLoadOperations.Add(preloadOperations);

            preloadCallback = callback;
            preloadOperations.Completed += LoadPreloadAssetsComplete;
        }

        private void LoadPreloadAssetsComplete(AsyncOperationHandle<IList<Object>> handle)
        {
            Debug.Log("LoadPreloadAssets: Complete " + handle.Status.ToString());

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

            if(preloadCallback != null)
                preloadCallback.Invoke();
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