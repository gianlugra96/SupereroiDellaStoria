using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSpeaker", menuName = "Data/New Speaker")]
[System.Serializable]
public class Speaker : ScriptableObject
{
    public string speakerName;
    public Color textColor;

    public Sprite speakerIconNameSprite;
    public Sprite speakerSprite;
    public List<Sprite> sprites;
    public SpriteController prefab;
}
