using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAG.Game
{
    [CreateAssetMenu(fileName = "TilePack_", menuName = "Configs/TilePack", order = 1)]
    public class TilePackProfile : ScriptableObject
    {
        public BoardTile[] boardTiles;

        public BoardTile GetTile()
        {
            return boardTiles[Random.Range(0, boardTiles.Length)];
        }

        public BoardTile[] GetTiles()
        {
            return boardTiles;
        }
    }
}