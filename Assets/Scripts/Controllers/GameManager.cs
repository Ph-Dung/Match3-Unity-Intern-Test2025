using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode
    {
        TILE_MATCH,
        TIME_ATTACK
    }

    public eLevelMode CurrentMode { get; private set; }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,
        WIN,
        LOSE
    }

    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }


    private GameSettings m_gameSettings;


    private BoardController m_boardController;
    private AutoplayService m_autoplayService;

    private UIMainManager m_uiMenu;
    private LevelTime m_levelTime;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    void Update()
    {
    }


    internal void SetState(eStateGame state)
    {
        State = state;

        if(State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode mode)
    {
        CurrentMode = mode;
        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(this, m_gameSettings);

        if (mode == eLevelMode.TIME_ATTACK)
        {
            m_levelTime = this.gameObject.AddComponent<LevelTime>();
            m_levelTime.Setup(60f, m_uiMenu.GetLevelConditionView(), this);
        }

        State = eStateGame.GAME_STARTED;
    }

    public void StartAutoplay(bool isWinTarget)
    {
        if (m_autoplayService == null)
        {
            m_autoplayService = this.gameObject.AddComponent<AutoplayService>();
            m_autoplayService.Setup(this, m_boardController);
        }
        else
        {
            m_autoplayService.UpdateBoardController(m_boardController);
        }
        
        m_autoplayService.StartAutoplay(isWinTarget);
    }

    public void GameOver(bool isWin)
    {
        StartCoroutine(WaitBoardController(isWin));
    }

    internal void ClearLevel()
    {
        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }

        if (m_levelTime != null)
        {
            Destroy(m_levelTime);
            m_levelTime = null;
        }
    }

    private IEnumerator WaitBoardController(bool isWin)
    {
        while (m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        State = isWin ? eStateGame.WIN : eStateGame.LOSE;
    }
}
