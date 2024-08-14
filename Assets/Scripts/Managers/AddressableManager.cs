using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableManager : MonoBehaviour
{
    public static AddressableManager Instance;

    [SerializeField] private AssetReferenceGameObject[] towerObjs;
    private List<GameObject> gameObjects = new List<GameObject>();
    private Dictionary<int, GameObject> loadedPrefabs = new Dictionary<int, GameObject>();
    private Dictionary<int, AsyncOperationHandle<GameObject>> prefabHandles = new Dictionary<int, AsyncOperationHandle<GameObject>>();

    private int selectedPrefabIndex = -1; // 선택된 프리팹 인덱스
    public int SelectedPrefabIndex {get { return selectedPrefabIndex; } set{ selectedPrefabIndex = value; }}

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(InitAddressable());
    }

    IEnumerator InitAddressable()
    {
        var init = Addressables.InitializeAsync();
        yield return init;
    }

    public void SetSelectedPrefabIndex(int index)
    {
        if (index >= towerObjs.Length)
        {
            Debug.LogError("Invalid index");
            return;
        }
        selectedPrefabIndex = index;
        LoadPrefab();
    }

   public void LoadPrefab()
    {
        if (selectedPrefabIndex < 0 || selectedPrefabIndex >= towerObjs.Length)
        {
            Debug.LogError("Invalid index for loading prefab");
            return;
        }

        // 이미 로드된 프리팹이 있으면 해제 후 다시 로드
        if (prefabHandles.ContainsKey(selectedPrefabIndex))
        {
            Addressables.Release(prefabHandles[selectedPrefabIndex]);
            prefabHandles.Remove(selectedPrefabIndex);
        }

        // 프리팹 로드 및 캐싱
        var handle = towerObjs[selectedPrefabIndex].LoadAssetAsync<GameObject>();
        handle.Completed += (completedHandle) =>
        {
            if (completedHandle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject prefab = completedHandle.Result;
                loadedPrefabs[selectedPrefabIndex] = prefab; // 로드된 프리팹 캐싱
                prefabHandles[selectedPrefabIndex] = completedHandle; // 핸들 캐싱
            }
            else
            {
                Debug.LogError("Failed to load prefab.");
            }
        };
    }

    public void InstantiatePrefab(Vector3 position, Quaternion rotation)
    {
        if (!loadedPrefabs.ContainsKey(selectedPrefabIndex))
        {
            Debug.LogError("Prefab not loaded.");
            return;
        }

        GameObject prefab = loadedPrefabs[selectedPrefabIndex];
        GameObject instance = Instantiate(prefab, position, rotation);
        gameObjects.Add(instance);
        instance.GetComponent<SpriteRenderer>().sortingOrder = -(int)position.y;

        // 인스턴스화 후 캐시에서 제거
        loadedPrefabs.Remove(selectedPrefabIndex);
        Addressables.Release(prefabHandles[selectedPrefabIndex]);
        prefabHandles.Remove(selectedPrefabIndex);

        selectedPrefabIndex = -1;
    }

    public GameObject GetLoadedPrefab()
    {
        if (selectedPrefabIndex < 0 || selectedPrefabIndex >= towerObjs.Length)
        {
            Debug.LogError("Invalid index for getting prefab");
            return null;
        }

        loadedPrefabs.TryGetValue(selectedPrefabIndex, out var prefab);
        return prefab;
    }

    private void OnDisable()
    {
        ReleaseAllInstances();
    }

    private void OnApplicationQuit()
    {
        ReleaseAllInstances();
    }

    private void ReleaseAllInstances()
    {
        foreach (var obj in gameObjects)
        {
            Debug.Log(obj);
            Addressables.ReleaseInstance(obj);
        }
        gameObjects.Clear();
        ReleaseAllCachedPrefabs();
    }

    public void ReleaseCachedPrefab(int index)
    {
        if (loadedPrefabs.ContainsKey(index))
        {
            Addressables.Release(loadedPrefabs[index]);
            loadedPrefabs.Remove(index);
        }
    }

    // 모든 캐시된 프리팹 해제
    public void ReleaseAllCachedPrefabs()
    {
        foreach (var kvp in loadedPrefabs)
        {
            Addressables.Release(kvp.Value);
        }
        loadedPrefabs.Clear();
    }
   
}
