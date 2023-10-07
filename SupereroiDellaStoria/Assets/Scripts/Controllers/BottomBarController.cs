using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static StoryScene.Sentence;

public class BottomBarController : MonoBehaviour
{
    public TextMeshProUGUI barText;
    public TextMeshProUGUI speakerNameText;
    public Image speakerIconNameImage;
    public Image speakerDialogueImage;
    public AudioSource voicePlayer;

    private int sentenceIndex = -1;
    private StoryScene currentScene;
    private State state = State.COMPLETED;
    private Animator animator;
    private bool isHidden = false;

    public Dictionary<Speaker, SpriteActionController> spritesAction;
    public GameObject spritesPrefab;

    private CameraActionController cameraActionController;
    private Coroutine typingCoroutine;
    private float speedFactor = 1f;

    private enum State
    {
        PLAYING, SPEEDED_UP, COMPLETED
    }

    private StoryScene CurrentScene => currentScene;
    private StoryScene.Sentence CurrentSentence => CurrentScene.sentences[sentenceIndex];
    private Speaker CurrentSentenceSpeaker => CurrentSentence.speaker;
    private List<StoryScene.Sentence.SpriteAction> CurrentSentenceSpriteActions => CurrentSentence.spriteActions;
    private List<StoryScene.Sentence.CameraAction> CurrentSentenceCameraActions => CurrentSentence.cameraActions;

    private void Start()
    {
        spritesAction = new Dictionary<Speaker, SpriteActionController>();
        animator = GetComponent<Animator>();
        cameraActionController = Camera.main.GetComponent<CameraActionController>();
    }

    #region SCENE MANAGER

    public void PlayScene(StoryScene scene, int sentenceIndex = -1, bool isAnimated = true)
    {
        currentScene = scene;
        this.sentenceIndex = sentenceIndex;
        PlayNextSentence(isAnimated);
    }

    #endregion

    #region SENTENCES MANAGER
    public int GetSentenceIndex()
    {
        return sentenceIndex;
    }

    public void SetSentenceIndex(int sentenceIndex)
    {
        this.sentenceIndex = sentenceIndex;
    }

    private void PlaySentence(bool isAnimated = true)
    {
        speedFactor = 1f;
        typingCoroutine = StartCoroutine(TypeText(CurrentSentence.text));

        InitializeDialogueText();

        if (CurrentSentence.audio)
        {
            voicePlayer.clip = CurrentSentence.audio;
            voicePlayer.Play();
        }
        else
        {
            voicePlayer.Stop();
        }

        ActSpeakers(isAnimated);
    }

    public void PlayNextSentence(bool isAnimated = true)
    {
        sentenceIndex++;
        PlaySentence(isAnimated);
    }

    public void PlayPreviousSentence()
    {
        sentenceIndex--;
        StopTyping();
        HideSprites();
        PlaySentence(false);
    }

    public bool IsLastSentence()
    {
        return sentenceIndex + 1 == currentScene.sentences.Count;
    }

    public bool IsFirstSentence()
    {
        return sentenceIndex == 0;
    }

    public bool IsSentenceCompleted()
    {
        return state == State.COMPLETED || state == State.SPEEDED_UP;
    }

    public void InitializeDialogueText()
    {
        //Initialize speaker Icon Name
        if(CurrentSentenceSpeaker.speakerIconNameSprite != null)
        {
            speakerIconNameImage.gameObject.SetActive(true);
            speakerIconNameImage.sprite = CurrentSentenceSpeaker.speakerIconNameSprite;
        }
        else
        {
            speakerIconNameImage.gameObject.SetActive(false);
        }

        //Initialize speaker Dialogue Image
        if(CurrentSentenceSpeaker.speakerDialogueSprite != null)
        {
            speakerDialogueImage.gameObject.SetActive(true);
            speakerDialogueImage.sprite = CurrentSentenceSpeaker.speakerDialogueSprite;
        }
        else
        {
            speakerDialogueImage.gameObject.SetActive(false);
        }

        speakerNameText.text = CurrentSentenceSpeaker.speakerName;
        speakerNameText.color = CurrentSentenceSpeaker.textColor;
    }

    public void ClearDialogueText()
    {
        speakerIconNameImage.gameObject.SetActive(false);
        speakerDialogueImage.gameObject.SetActive(false);
        barText.text = "";
        speakerNameText.text = "";
    }

    public void SpeedUpTyping()
    {
        state = State.SPEEDED_UP;
        speedFactor = 0.25f;
    }

    public void StopTyping()
    {
        state = State.COMPLETED;
        StopCoroutine(typingCoroutine);
    }

    private IEnumerator TypeText(string text)
    {
        barText.text = "";
        state = State.PLAYING;
        int wordIndex = 0;

        while (state != State.COMPLETED)
        {
            barText.text += text[wordIndex];
            yield return new WaitForSeconds(speedFactor * 0.05f);
            if (++wordIndex == text.Length)
            {
                state = State.COMPLETED;
                break;
            }
        }
    }

    #endregion

    #region SPEAKERS/ACTIONS MANAGER

    //Play all Sprite/Camera actions
    private void ActSpeakers(bool isAnimated = true)
    {
        CurrentSentenceSpriteActions.ForEach(action => ActSpriteAction(action, isAnimated));
        CurrentSentenceCameraActions.ForEach(action => ActCameraAction(action, isAnimated));
    }

    private void ActSpriteAction(SpriteAction spriteAction, bool isAnimated = true)
    {
        SpriteActionController spriteActionController;
        if (!spritesAction.ContainsKey(spriteAction.speaker))
        {
            spriteActionController = Instantiate(spriteAction.speaker.prefab.gameObject, spritesPrefab.transform).GetComponent<SpriteActionController>();
            spritesAction.Add(spriteAction.speaker, spriteActionController);
        }
        else
        {
            spriteActionController = spritesAction[spriteAction.speaker];
        }

        //Sprite movement
        switch (spriteAction.actionType)
        {
            case SpriteAction.Type.APPEAR:
                spriteActionController.Setup(SpriteFromSpriteAction(spriteAction));
                spriteActionController.Show(spriteAction.coords, isAnimated);
                return;
            case SpriteAction.Type.MOVE:
                spriteActionController.Move(spriteAction.coords, spriteAction.moveSpeed, isAnimated);
                break;
            case SpriteAction.Type.DISAPPEAR:
                spriteActionController.Hide(isAnimated);
                break;
        }

        //Change sprite
        spriteActionController.SwitchSprite(SpriteFromSpriteAction(spriteAction), isAnimated);
    }

    private void ActCameraAction(CameraAction cameraAction, bool isAnimated = true)
    {
        cameraActionController.Move(-cameraAction.moveCoords, cameraAction.moveSpeed, isAnimated);
        cameraActionController.Zoom(cameraAction.zoom, cameraAction.zoomSpeed, isAnimated);
    }

    private Sprite SpriteFromSpriteAction(SpriteAction spriteAction)
    {
        return spriteAction.speaker.sprites[spriteAction.spriteIndex];
    }

    public void ResetCamera()
    {
        cameraActionController.ResetMove();
        cameraActionController.ResetZoom();
    }

    public void HideSprites()
    {
        while (spritesPrefab.transform.childCount > 0)
        {
            DestroyImmediate(spritesPrefab.transform.GetChild(0).gameObject);
        }
        spritesAction.Clear();
    }

    #endregion

    #region ANIMATOR MANAGER

    public void Hide()
    {
        if (!isHidden)
        {
            animator.SetTrigger("Hide");
            isHidden = true;
        }
    }

    public void Show()
    {
        animator.SetTrigger("Show");
        isHidden = false;
    }

    #endregion

}
