using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HornSpirit {
    public class DeckLoader : MonoBehaviour {
        private DeckData targetDeck;
        public UnityAction OnDeckLoaded;

        public void LoadDeck(DeckData deckToLoad) {
            targetDeck = deckToLoad;

            // Resources에서 카드 데이터를 로드
            targetDeck.LoadCards();

            // 로드 완료 후 이벤트 호출
            OnDeckLoaded?.Invoke();

            // 이 컴포넌트를 게임 오브젝트에서 제거합니다.
            Destroy(this);
        }
    }
}
