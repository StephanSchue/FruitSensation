using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAG.Game
{
    [System.Serializable]
    public enum WinCondition
    {
        Points,
        Endless
    }

    [System.Serializable]
    public struct MatchWinCondition
    {
        public WinCondition condition;
        public int value;

        public MatchWinCondition(MatchWinCondition otherCondition)
        {
            this.condition = otherCondition.condition;
            this.value = otherCondition.value;
        }
    }

    [CreateAssetMenu(fileName = "MatchConditionsProfile_", menuName = "Configs/MatchConditionsProfile", order = 1)]
    public class MatchConditionsProfile : ScriptableObject
    {
        public int moves = 30;
        public MatchWinCondition winCondtion;

        public bool ValidateWinCondition(int score, int remainingMoves)
        {
            switch(winCondtion.condition)
            {
                case WinCondition.Points:
                    return remainingMoves == 0 && score >= winCondtion.value;
                case WinCondition.Endless:
                    return false;
            }

            return false;
        }
        
        public bool ValidateLooseCondition(int remainingMoves)
        {
            switch(winCondtion.condition)
            {
                case WinCondition.Points:
                    return (remainingMoves == 0);
            }

            return false;
        }
    }
}
