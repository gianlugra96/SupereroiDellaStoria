using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStoryScene", menuName ="Data/New Story Scene")]
[System.Serializable]
public class StoryScene : GameScene
{
    public List<Sentence> sentences;
    public Sprite background;
    public GameScene nextScene;

    [System.Serializable]
    public struct Sentence
    {
        public string text;
        public AudioClip audio;
        public Speaker speaker;
        public List<SpriteAction> spriteActions;
        public List<CameraAction> cameraActions;

        public AudioClip music;
        public AudioClip sound;

        [System.Serializable]
        public struct SpriteAction
        {
            public Speaker speaker;
            public int spriteIndex;
            public Type actionType;
            public Vector2 coords;
            public float moveSpeed;

            [System.Serializable]
            public enum Type
            {
                NONE, APPEAR, MOVE, DISAPPEAR
            }
        }

        [System.Serializable]
        public struct CameraAction
        {
            [Header("Zoom")]
            public bool zoom;
            public Zoom zoomPercentage;
            public float zoomSpeed;

            [Header("Move")]
            public bool move;
            public Vector2 moveCoords;
            public float moveSpeed;

            [Header("Shake")]
            public bool shake;
            public float shakeMagnitude;
            public float shakeDuration;

            [System.Serializable]
            public enum Zoom
            {
                x1, x2, x4, x8, x16
            }
        }
    }
}

public class GameScene : ScriptableObject { }
