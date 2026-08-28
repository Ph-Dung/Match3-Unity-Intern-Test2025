using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelGameOver : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnClose;

    private UIMainManager m_mngr;

    private void Awake()
    {
        btnClose.onClick.AddListener(OnClickClose);
    }

    private void OnDestroy()
    {
        if (btnClose) btnClose.onClick.RemoveAllListeners();
    }

    private void OnClickClose()
    {
        m_mngr.ShowMainMenu();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        
        // Find and update title text
        Text[] texts = GetComponentsInChildren<Text>(true);
        Text titleText = null;
        foreach (var t in texts)
        {
            if (t.gameObject != btnClose.gameObject && t.transform.parent != btnClose.transform)
            {
                titleText = t;
                break;
            }
        }

        if (titleText != null)
        {
            if (m_mngr.GetGameManager().State == GameManager.eStateGame.WIN)
            {
                titleText.text = "YOU WIN!";
                titleText.color = Color.green;
            }
            else
            {
                titleText.text = "YOU LOSE!";
                titleText.color = Color.red;
            }
        }
    }

}
