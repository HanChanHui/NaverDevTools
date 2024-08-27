using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class GridSystemVisualSingle : MonoBehaviour {
        [SerializeField] private SpriteRenderer sprite;

        public void Show(Material material) {
            sprite.enabled = true;
            sprite.material = material;
        }

        public void GridLayerChange(string name) {
            sprite.sortingLayerName = name;
        }

        public void Hide() {
            sprite.enabled = false;
        }
    }
}
