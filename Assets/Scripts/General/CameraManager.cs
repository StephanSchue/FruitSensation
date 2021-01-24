using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAG.General
{
    public class CameraManager : MonoBehaviour
    {
        public Transform origin;
        public Camera camera;

        public CameraSceneProfile cameraSceneProfile;

        private Vector3 originPosition;

        private void Reset()
        {
            origin = transform;
            camera = Camera.main;
        }

        // Start is called before the first frame update
        private void Awake()
        {
            if(origin == null)
                return;

            originPosition = origin.position;
            AdjustToAspectRatio();
        }

        public void AdjustToAspectRatio()
        {

            if(origin == null || camera == null || cameraSceneProfile == null)
            {
                Debug.LogErrorFormat("'{0}': Is missing component reference. Function interrupted.", name);
                return;
            }
            bool isPortrait = Screen.width < Screen.height;
            float ratio = isPortrait ? Screen.height / (float)Screen.width : Screen.width / Screen.height;
            AspectRatioElement[] aspects = cameraSceneProfile.aspects;

            for(int i = 0; i < aspects.Length; i++)
            {
                if(ratio >= aspects[i].aspectRatio)
                {
                    UpdateTransform(aspects[i].offset);
                    UpdateOrthographicSize(aspects[i].orthographicSize);
                    break;
                }
            }
        }
        
        public void ResetCameraAspect()
        {
            camera.ResetAspect();
        }

        #region Property Methods

        private void UpdateTransform(Vector3 offset)
        {
            origin.transform.position = originPosition + offset;
        }

        private void UpdateOrthographicSize(float orthographicSize)
        {
            camera.orthographicSize = orthographicSize;
        }

        #endregion
    }

}