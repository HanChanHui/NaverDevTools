using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : Singleton<ResourceManager>
{
    [SerializeField] private List<PlaceableTowerData> prefabs;
    public List<PlaceableTowerData> Prefabs{get{return prefabs;}}
    public GameObject enemyPrefab;
    public GameObject enemyBossPrefab;

    [SerializeField] private Transform gridSystemVisualSingPrefab;
    [SerializeField] private Transform gridSystemVisualSingPrefab2;
    public Transform GridSystemVisualSingPrefab { get {return gridSystemVisualSingPrefab; } }
    public Transform GridSystemVisualSingPrefab2 { get {return gridSystemVisualSingPrefab2; } }

    private int selectedPrefabIndex = -1; // 선택된 프리팹 인덱스
    public int SelectedPrefabIndex {get { return selectedPrefabIndex; } set{ selectedPrefabIndex = value; }}



    public void SetSelectedPrefabIndex(int index)
    {
        if (index > prefabs.Count - 1)
        {
            Debug.LogError("Invalid index");
            return;
        }
        selectedPrefabIndex = index;
    }
}
