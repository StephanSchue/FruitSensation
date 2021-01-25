using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MAG.General
{
    public class EventDefinitions : MonoBehaviour
    {
        [System.Serializable] public class StringEvent : UnityEvent<string> { }
        [System.Serializable] public class IntEvent : UnityEvent<int> { }
    }
}
