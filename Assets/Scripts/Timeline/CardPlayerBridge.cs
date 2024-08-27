using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace HornSpirit {
    public class CardPlayerBridge : MonoBehaviour, INotificationReceiver {
        public GameManager gameManager;

        public void OnNotify(Playable origin, INotification notification, object context) {
            CardMarker cm = notification as CardMarker;

            if (cm != null) {
                gameManager.UseCard(cm.card, cm.position, cm.towerGridPosition, cm.towerCost);
            }
        }
    }
}
