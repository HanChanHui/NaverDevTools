using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    [CreateAssetMenu(fileName = "NewDeck", menuName = "Unity Royale/Deck Data")]
    public class DeckData : ScriptableObject {
        public string[] cardNames;  // Resources에서 로드할 카드 이름 목록
        private CardData[] cards;
        private int currentCard = 0;

        // 실제 카드 데이터를 배열에 로드
        public void LoadCards() {
            int totalCards = cardNames.Length;
            cards = new CardData[totalCards];
            for (int c = 0; c < totalCards; c++) {
                cards[c] = Resources.Load<CardData>($"GameData/Cards/{cardNames[c]}");
            }
        }

        // Fisher-Yates 섞기 알고리즘
        public void ShuffleCards() {
            System.Random random = new System.Random();
            for (int i = cards.Length - 1; i > 0; i--) {
                int j = random.Next(0, i + 1);
                // Swap cards[i] with the element at random index
                CardData temp = cards[i];
                cards[i] = cards[j];
                cards[j] = temp;
            }
        }

        // 덱에서 다음 카드를 반환.
        public CardData GetNextCardFromDeck() {
            currentCard++;
            if (currentCard >= cards.Length)
                currentCard = 0;

            return cards[currentCard];
        }
    }
}
