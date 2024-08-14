using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DeckLoader : MonoBehaviour
{
    private DeckData targetDeck;
    public UnityAction OnDeckLoaded;

    public void LoadDeck(DeckData deckToLoad)
    {
        targetDeck = deckToLoad;
        Addressables.LoadAssetsAsync<CardData>(targetDeck.labelsToInclude[0].labelString, null).Completed += OnResourcesRetrieved;
    }

    private void OnResourcesRetrieved(AsyncOperationHandle<IList<CardData>> obj)
    {
       if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            // DeckManager에 카드 데이터를 넘겨줍니다.
            targetDeck.CardsRetrieved((List<CardData>)obj.Result);

            if (OnDeckLoaded != null)
            {
                OnDeckLoaded();
            }

            // Addressables에서 로드한 리소스를 해제합니다.
            Addressables.Release(obj);

            // 이 컴포넌트를 게임 오브젝트에서 제거합니다.
            Destroy(this);
        }
        else
        {
            Debug.LogError("Failed to load deck resources");
        }
    }
}
