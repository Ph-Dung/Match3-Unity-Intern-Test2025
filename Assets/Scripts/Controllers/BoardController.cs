using System;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    private Board m_board;
    private TrayManager m_trayManager;
    private GameManager m_gameManager;
    private Camera m_cam;
    private GameSettings m_gameSettings;
    private bool m_gameOver;

    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;
        m_gameSettings = gameSettings;

        m_gameManager.StateChangedAction += OnGameStateChange;
        m_cam = Camera.main;

        m_board = new Board(this.transform, gameSettings);
        
        m_trayManager = new GameObject("TrayManager").AddComponent<TrayManager>();
        m_trayManager.transform.SetParent(this.transform);
        m_trayManager.Setup(m_gameManager, m_board, m_gameSettings);

        Fill();
    }

    private void Fill()
    {
        m_board.Fill();
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
            case GameManager.eStateGame.WIN:
            case GameManager.eStateGame.LOSE:
                m_gameOver = true;
                break;
        }
    }

    private void Update()
    {
        if (m_gameOver || IsBusy) return;
        if (m_cam == null) { m_cam = Camera.main; return; }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 screenPos = Input.mousePosition;
            screenPos.z = -m_cam.transform.position.z;
            Vector2 worldPos = m_cam.ScreenToWorldPoint(screenPos);

            var hits = Physics2D.RaycastAll(worldPos, Vector2.zero);

            foreach (var hit in hits)
            {
                if (hit.collider != null && m_trayManager.HandleTrayItemClick(hit.collider.transform))
                    return;
            }

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                Cell clickedCell = hit.collider.GetComponent<Cell>();
                if (clickedCell != null && !clickedCell.IsEmpty)
                {
                    HandleCellClick(clickedCell);
                    return;
                }
            }
        }
    }

    public void HandleCellClick(Cell clickedCell)
    {
        if (clickedCell == null || clickedCell.IsEmpty) return;

        Item item = clickedCell.Item;
        item.EnableCollider(false);
        clickedCell.Free();

        OnMoveEvent();

        m_trayManager.AddItem(item);
    }

    internal void Clear()
    {
        if (m_board != null)
        {
            m_board.Clear();
        }
        
        if (m_trayManager != null)
        {
            m_trayManager.Clear();
            Destroy(m_trayManager.gameObject);
        }
    }
    
    public Board GetBoard()
    {
        return m_board;
    }
}
