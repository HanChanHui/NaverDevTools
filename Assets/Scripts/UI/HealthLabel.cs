using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HornSpirit {
    public class HealthLabel : MonoBehaviour {
        [SerializeField] Slider healthSlider;

        public void Show() {
            gameObject.SetActive(true);
        }

        public void Hide() {
            gameObject.SetActive(false);
        }

        public void Init() {
            healthSlider.value = 1f;
        }

        public void InitAndShow() {
            Init();
            Show();
        }

        public void UpdateHealth(float health, float maxHealth) {
            healthSlider.value = (float)health / maxHealth;
        }
    }
}
