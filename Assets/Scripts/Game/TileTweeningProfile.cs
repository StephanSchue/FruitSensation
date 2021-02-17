using DG.Tweening;
using MAG.General.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAG.Game.Tweening
{
    [CreateAssetMenu(fileName = "TileTweenProfile_", menuName = "Configs/TileTweenProfile", order = 1)]
    public class TileTweeningProfile : ScriptableObject
    {
        public TweenBaseSettingsElement moveTweenSettings;
        public TweenColorSettingsElement selectTweenSettings;
        public TweenColorSettingsElement deselectTweenSettings;
    }
}