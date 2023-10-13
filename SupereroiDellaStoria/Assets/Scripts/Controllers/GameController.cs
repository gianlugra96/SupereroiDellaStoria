using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("Controller")]
    public BottomBarController bottomBarController;
    public SpriteSwitcherController backgroundController;
    public ChooseController chooseController;
    public AudioController audioController;

    [Header("Scene")]
    public GameScene currentScene;
    public string menuScene;
    public DataHolder data;

    [Header("Camera")]
    public Camera VFXCamera;

    private State state = State.IDLE;

    private List<StoryScene> history = new List<StoryScene>();

    private enum State
    {
        IDLE, ANIMATE, CHOOSE
    }

    void Start()
    {
        if (SaveManager.IsGameSaved())
        {
            LoadLastSave();
        }
        if (currentScene is StoryScene)
        {
            StoryScene storyScene = currentScene as StoryScene;
            history.Add(storyScene);
            bottomBarController.PlayScene(storyScene, bottomBarController.GetSentenceIndex());
            backgroundController.SetImage(storyScene.background);
            PlayAudio(storyScene.sentences[bottomBarController.GetSentenceIndex()]);
            PlayVFX(storyScene);
        }
    }
    void Update()
    {
        if (state == State.IDLE) {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                //PLAY NEXT SENTENCE
                if (bottomBarController.IsSentenceCompleted())
                {
                    bottomBarController.StopTyping();
                    if (bottomBarController.IsLastSentence())
                    {
                        PlayScene((currentScene as StoryScene).nextScene);
                    }
                    else
                    {
                        bottomBarController.PlayNextSentence();
                        PlayAudio((currentScene as StoryScene).sentences[bottomBarController.GetSentenceIndex()]);
                    }
                }
                else
                {
                    bottomBarController.SpeedUpTyping();
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                //PLAY PREVIOUS SENTENCE
                if (bottomBarController.IsFirstSentence())
                {
                    if(history.Count > 1)
                    {
                        bottomBarController.StopTyping();
                        bottomBarController.HideSprites();
                        history.RemoveAt(history.Count - 1);
                        StoryScene scene = history[history.Count - 1];
                        history.RemoveAt(history.Count - 1);
                        PlayScene(scene, scene.sentences.Count - 2, false);
                    }
                }
                else
                {
                    bottomBarController.PlayPreviousSentence();
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SaveData();
                SceneManager.LoadScene(menuScene);
            }
        }
    }

    public void PlayScene(GameScene scene, int sentenceIndex = -1, bool isAnimated = true)
    {
        StartCoroutine(SwitchScene(scene, sentenceIndex, isAnimated));
    }

    #region SAVE MANAGER

    private void SaveData()
    {
        List<int> historyIndicies = new List<int>();
        history.ForEach(scene =>
        {
            historyIndicies.Add(this.data.scenes.IndexOf(scene));
        });
        SaveData data = new SaveData
        {
            sentence = bottomBarController.GetSentenceIndex(),
            prevScenes = historyIndicies
        };
        SaveManager.SaveGame(data);
    }

    private void LoadLastSave()
    {
        SaveData data = SaveManager.LoadGame();
        data.prevScenes.ForEach(scene =>
        {
            history.Add(this.data.scenes[scene] as StoryScene);
        });
        currentScene = history[history.Count - 1];
        history.RemoveAt(history.Count - 1);
        bottomBarController.SetSentenceIndex(data.sentence - 1);
    }


    #endregion

    #region BACKGROUND CONTROLLER

    private IEnumerator SwitchScene(GameScene scene, int sentenceIndex = -1, bool isAnimated = true)
    {
        state = State.ANIMATE;
        currentScene = scene;
        if (isAnimated)
        {
            bottomBarController.Hide();
            yield return new WaitForSeconds(1f);
        }
        if (scene is StoryScene)
        {
            StoryScene storyScene = scene as StoryScene;
            history.Add(storyScene);
            PlayAudio(storyScene.sentences[sentenceIndex + 1]);
            PlayVFX(storyScene);
            if (isAnimated)
            {
                backgroundController.SwitchImage(storyScene.background);
                yield return new WaitForSeconds(1f);
                bottomBarController.ClearDialogueText();
                bottomBarController.Show();
                yield return new WaitForSeconds(1f);
            }
            else
            {
                backgroundController.SetImage(storyScene.background);
                bottomBarController.ClearDialogueText();
            }
            bottomBarController.PlayScene(storyScene, sentenceIndex, isAnimated);
            state = State.IDLE;
        }
        else if (scene is ChooseScene)
        {
            state = State.CHOOSE;
            chooseController.SetupChoose(scene as ChooseScene);
        }
    }

    private void PlayVFX(StoryScene scene)
    {
        for (int i = 0; i < VFXCamera.transform.childCount; i++)
        {
            Destroy(VFXCamera.transform.GetChild(i).gameObject);
        }

        if (scene.VFXEffect != null)
        {
            var vfx = Instantiate(scene.VFXEffect, VFXCamera.transform);
            vfx.SetActive(true);
        }

    }


    #endregion

    #region AUDIO CONTROLLER

    private void PlayAudio(StoryScene.Sentence sentence)
    {
        audioController.PlayAudio(sentence.music, sentence.sound);
    }

    #endregion
}
