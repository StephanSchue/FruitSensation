using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MAG.Game
{
    [CustomEditor(typeof(BoardManager))]
    public class BoardManagerEditor : Editor
    {
        private BoardManager boardManager;

        private void OnEnable()
        {
            boardManager = target as BoardManager;
        }
    }
}