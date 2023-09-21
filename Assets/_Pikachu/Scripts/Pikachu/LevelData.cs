﻿using UnityEngine;

namespace Pokemon
{
    [CreateAssetMenu(fileName = "LevelData_", menuName = "Data/Level Data", order = 0)]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private Vector2Int boardSize;
        [SerializeField] private int numberOfPokemon;

        public Vector2Int BoardSize => boardSize;
        public int NumberOfPokemon => numberOfPokemon;
    }
}