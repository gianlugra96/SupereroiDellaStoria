using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSpeaker", menuName = "Data/New Speaker")]
[System.Serializable]
public class Speaker : ScriptableObject
{
    public string speakerName;
    public Color textColor;

    public Sprite speakerIconNameSprite;
    public Sprite speakerDialogueSprite;
    public List<Sprite> sprites;
    public SpriteActionController prefab;
}
