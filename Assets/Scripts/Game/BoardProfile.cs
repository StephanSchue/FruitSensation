using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAG.Game
{
    [CreateAssetMenu(fileName = "BoardProfile_", menuName = "Configs/BoardProfile", order = 1)]
    public class BoardProfile : ScriptableObject
    {
        public Vector2Int size = new Vector2Int(5,5);
        public TilePackProfile tilePack;
        public MatchConditionsProfile matchConditions;
    }
}