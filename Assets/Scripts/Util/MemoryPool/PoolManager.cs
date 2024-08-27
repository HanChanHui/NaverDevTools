using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour {
    public static PoolManager Instance = null;

    [System.Serializable]
    public class PoolGroup {
        public MemItem[] items;
        public Dictionary<string, MemoryPool> pool;
    }

    [System.Serializable]
    public struct MemItem {
        public string type;
        public GameObject obj;
        public int count;
    };

    public PoolManagerGroup itemGroup;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            // DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void Init() {
        InitGroup();
    }

    private void InitGroup() {
        foreach (PoolGroup group in itemGroup.Values) {
            group.pool = new Dictionary<string, MemoryPool>();

            foreach (MemItem item in group.items) {
                group.pool.Add(item.type, new MemoryPool(item.obj, item.count, transform));
            }
        }
    }

    GameObject NewItem(string group, string name) {
        if (itemGroup.ContainsKey(group) && itemGroup[group].pool.ContainsKey(name)) {
            return itemGroup[group].pool[name].NewItem();
        } else {
            return null;
        }
    }

    public T NewItem<T>(string group, string name) {
        GameObject obj = NewItem(group, name);

        if (obj) {
            T objType = obj.GetComponent<T>();

            if (objType != null) {
                return objType;
            } else {
                return default(T);
            }
        } else {
            return default(T);
        }
    }

    public MemoryPool GetMemoryPool(string group, string name) {
        if (itemGroup.ContainsKey(group) && itemGroup[group].pool.ContainsKey(name)) {
            return itemGroup[group].pool[name];
        } else {
            return null;
        }
    }

    public void RemoveItem(string group, string name, GameObject gameObject) {
        if (itemGroup.ContainsKey(group) && itemGroup[group].pool.ContainsKey(name)) {
            itemGroup[group].pool[name].RemoveItem(gameObject);
        }
    }

    public int GetCount(string group, string name) {
        if (itemGroup.ContainsKey(group) && itemGroup[group].pool.ContainsKey(name)) {
            return itemGroup[group].pool[name].Count;
        } else {
            return -1;
        }
    }
}
