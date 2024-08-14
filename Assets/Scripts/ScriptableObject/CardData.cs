using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Unity Royale/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Card graphics")]
    public Sprite cardImage;
    public Sprite cardPropertyImage;

    [Header("List of Placeables")]
    public PlaceableTowerData towerData;
    public Vector3 relativeOffsets;
}
