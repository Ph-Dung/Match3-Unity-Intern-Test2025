using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoplayService : MonoBehaviour
{
    private BoardController m_boardController;
    private GameManager m_gameManager;
    private bool m_isRunning;
    private bool m_isWinTarget;

    private Queue<Cell> m_clickQueue = new Queue<Cell>();

    public void Setup(GameManager gameManager, BoardController boardController)
    {
        m_gameManager = gameManager;
        m_boardController = boardController;
        m_gameManager.StateChangedAction += OnGameStateChange;
    }

    public void UpdateBoardController(BoardController boardController)
    {
        m_boardController = boardController;
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        if (state == GameManager.eStateGame.WIN || state == GameManager.eStateGame.LOSE || state == GameManager.eStateGame.GAME_OVER)
        {
            StopAutoplay();
        }
    }

    public void StartAutoplay(bool isWinTarget)
    {
        if (m_isRunning) return;
        m_isRunning = true;
        m_isWinTarget = isWinTarget;
        m_clickQueue.Clear();
        StartCoroutine(AutoplayRoutine());
    }

    public void StopAutoplay()
    {
        m_isRunning = false;
        StopAllCoroutines();
    }

    private IEnumerator AutoplayRoutine()
    {
        while (m_isRunning)
        {
            yield return new WaitForSeconds(0.5f);

            if (m_boardController == null)
            {
                StopAutoplay();
                yield break;
            }

            Board board = m_boardController.GetBoard();
            if (board == null || board.IsEmpty())
            {
                continue;
            }

            Cell cellToClick = null;

            if (m_clickQueue.Count > 0)
            {
                cellToClick = m_clickQueue.Dequeue();
                if (cellToClick == null || cellToClick.IsEmpty)
                {
                    cellToClick = null;
                }
            }

            if (cellToClick == null)
            {
                if (m_isWinTarget)
                {
                    Cell randomCell = board.GetRandomNotEmptyCell();
                    if (randomCell != null && randomCell.Item is NormalItem normalItem)
                    {
                        var allMatching = board.GetAllCellsWithItemType(normalItem.ItemType);
                        foreach (var c in allMatching)
                        {
                            m_clickQueue.Enqueue(c);
                        }
                        if (m_clickQueue.Count > 0)
                        {
                            cellToClick = m_clickQueue.Dequeue();
                        }
                    }
                }
                else
                {
                    var uniqueTypes = new HashSet<NormalItem.eNormalType>();
                    int attempts = 0;
                    while (m_clickQueue.Count < 5 && attempts < 50)
                    {
                        attempts++;
                        Cell c = board.GetRandomNotEmptyCell();
                        if (c != null && c.Item is NormalItem item)
                        {
                            if (!uniqueTypes.Contains(item.ItemType))
                            {
                                uniqueTypes.Add(item.ItemType);
                                m_clickQueue.Enqueue(c);
                            }
                        }
                    }

                    if (m_clickQueue.Count > 0)
                    {
                        cellToClick = m_clickQueue.Dequeue();
                    }
                }
            }

            if (cellToClick != null)
            {
                m_boardController.HandleCellClick(cellToClick);
            }
        }
    }

    private void OnDestroy()
    {
        if (m_gameManager != null)
        {
            m_gameManager.StateChangedAction -= OnGameStateChange;
        }
    }
}
