using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAG.General
{
    [System.Serializable]
    public struct AspectRatioElement
    {
        public string label;
        public float aspectRatio;
        public bool orthographic;
        public float orthographicSize;
        public Vector3 offset;
    }

    [CreateAssetMenu(fileName = "CameraSceneProfile_", menuName = "Configs/CameraSceneProfile", order = 1)]
    public class CameraSceneProfile : ScriptableObject
    {
        public AspectRatioElement[] aspects;

        private void Reset()
        {
            bool orthographicActive = true;
            float orthographicSize = 5f;

            aspects = new AspectRatioElement[]
            {
                new AspectRatioElement() { label="9:21", aspectRatio = 2.33f, orthographic = orthographicActive, orthographicSize = orthographicSize },
                new AspectRatioElement() { label="9:19", aspectRatio = 2.11f, orthographic = orthographicActive, orthographicSize = orthographicSize  },
                new AspectRatioElement() { label="10:19", aspectRatio = 1.9f, orthographic = orthographicActive, orthographicSize = orthographicSize  },
                new AspectRatioElement() { label="9:16", aspectRatio = 1.77f, orthographic = orthographicActive, orthographicSize = orthographicSize  },
                new AspectRatioElement() { label="10:16", aspectRatio = 1.6f, orthographic = orthographicActive, orthographicSize = orthographicSize  },
                new AspectRatioElement() { label="2:3", aspectRatio = 1.5f, orthographic = orthographicActive, orthographicSize = orthographicSize  },
                new AspectRatioElement() { label="3:4", aspectRatio = 1.33f, orthographic = orthographicActive, orthographicSize = orthographicSize  },
                new AspectRatioElement() { label="10:14", aspectRatio = 1.4f, orthographic = orthographicActive, orthographicSize = orthographicSize  }
            };
        }
    }
}