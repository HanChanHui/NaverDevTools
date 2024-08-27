using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class EnemyAttackEffect : MonoBehaviour {


        void Start() {
            Destroy(gameObject, 0.3f);
        }

    }
}
