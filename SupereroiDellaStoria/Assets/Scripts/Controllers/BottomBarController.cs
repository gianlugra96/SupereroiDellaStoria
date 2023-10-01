using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BottomBarController : MonoBehaviour
{
    public TextMeshProUGUI barText;
    public TextMeshProUGUI speakerNameText;
    public Image speakerIconNameImage;
    public Image speakerImage;
    public AudioSource voicePlayer;

    private int sentenceIndex = -1;
    private StoryScene currentScene;
    private State state = State.COMPLETED;
    private Animator animator;
    private bool isHidden = false;

    public Dictionary<Speaker, SpriteController> sprites;
    public GameObject spritesPrefab;

    private Coroutine typingCoroutine;
    private float speedFactor = 1f;

    private enum State
    {
        PLAYING, SPEEDED_UP, COMPLETED
    }

    private StoryScene CurrentScene => currentScene;
    private StoryScene.Sentence CurrentSentence => CurrentScene.sentences[sentenceIndex];
    private Speaker CurrentSentenceSpeaker => CurrentSentence.speaker;
    private List<StoryScene.Sentence.Action> CurrentSentenceActions => CurrentSentence.actions;

    private void Start()
    {
        sprites = new Dictionary<Speaker, SpriteController>();
        animator = GetComponent<Animator>();
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
        if(CurrentSentenceSpeaker.speakerIconNameSprite != null)
        {
            speakerIconNameImage.gameObject.SetActive(true);
            speakerIconNameImage.sprite = CurrentSentenceSpeaker.speakerIconNameSprite;
        }
        else
        {
            speakerIconNameImage.gameObject.SetActive(false);
        }

        speakerNameText.text = CurrentSentenceSpeaker.speakerName;
        speakerNameText.color = CurrentSentenceSpeaker.textColor;
    }

    public void ClearDialogueText()
    {
        speakerIconNameImage.gameObject.SetActive(false);
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

    //Play all actions speaker
    private void ActSpeakers(bool isAnimated = true)
    {
        for (int i = 0; i < CurrentSentenceActions.Count; i++)
        {
            ActSpeaker(CurrentSentenceActions[i], isAnimated);
        }
    }

    private void ActSpeaker(StoryScene.Sentence.Action action, bool isAnimated = true)
    {
        SpriteController spriteController;
        if (!sprites.ContainsKey(action.speaker))
        {
            spriteController = Instantiate(action.speaker.prefab.gameObject, spritesPrefab.transform).GetComponent<SpriteController>();
            sprites.Add(action.speaker, spriteController);
        }
        else
        {
            spriteController = sprites[action.speaker];
        }

        switch (action.actionType)
        {
            case StoryScene.Sentence.Action.Type.APPEAR:
                spriteController.Setup(action.speaker.sprites[action.spriteIndex]);
                spriteController.Show(action.coords, isAnimated);
                return;
            case StoryScene.Sentence.Action.Type.MOVE:
                spriteController.Move(action.coords, action.moveSpeed, isAnimated);
                break;
            case StoryScene.Sentence.Action.Type.DISAPPEAR:
                spriteController.Hide(isAnimated);
                break;
        }
        spriteController.SwitchSprite(action.speaker.sprites[action.spriteIndex], isAnimated);
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

    public void HideSprites()
    {
        while (spritesPrefab.transform.childCount > 0)
        {
            DestroyImmediate(spritesPrefab.transform.GetChild(0).gameObject);
        }
        sprites.Clear();
    }

    #endregion

}
