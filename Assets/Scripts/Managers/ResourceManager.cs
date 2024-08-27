using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class ResourceManager : Singleton<ResourceManager> {


        [SerializeField] private Transform gridSystemVisualSingPrefab;
        [SerializeField] private Transform gridSystemVisualSingPrefab2;
        public Transform GridSystemVisualSingPrefab { get { return gridSystemVisualSingPrefab; } }
        public Transform GridSystemVisualSingPrefab2 { get { return gridSystemVisualSingPrefab2; } }

        [SerializeField] private PatternData patternData;
        public PatternData GetPatternData { get { return patternData; } }




    }
}
