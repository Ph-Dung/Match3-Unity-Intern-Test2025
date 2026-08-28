using UnityEngine;
using UnityEngine.UI;

public class UIPanelMain : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnAutoplay;
    [SerializeField] private Button btnAutoLose;
    [SerializeField] private Button btnTimeAttack;

    private UIMainManager m_mngr;

    private void Awake()
    {
        if (btnPlay)       btnPlay.onClick.AddListener(OnClickPlay);
        if (btnAutoplay)   btnAutoplay.onClick.AddListener(OnClickAutoplay);
        if (btnAutoLose)   btnAutoLose.onClick.AddListener(OnClickAutoLose);
        if (btnTimeAttack) btnTimeAttack.onClick.AddListener(OnClickTimeAttack);
    }

    private void OnDestroy()
    {
        if (btnPlay)       btnPlay.onClick.RemoveAllListeners();
        if (btnAutoplay)   btnAutoplay.onClick.RemoveAllListeners();
        if (btnAutoLose)   btnAutoLose.onClick.RemoveAllListeners();
        if (btnTimeAttack) btnTimeAttack.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    private void OnClickPlay()
    {
        m_mngr.LoadLevelMoves();
    }

    private void OnClickAutoplay()
    {
        m_mngr.LoadLevelAutoplay(true);
    }

    private void OnClickAutoLose()
    {
        m_mngr.LoadLevelAutoplay(false);
    }

    private void OnClickTimeAttack()
    {
        m_mngr.LoadLevelTimeAttack();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
