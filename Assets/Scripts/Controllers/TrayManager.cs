using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class TrayManager : MonoBehaviour
{
    private List<Item> m_trayItems = new List<Item>();
    private GameManager m_gameManager;
    private Board m_board;
    private int m_capacity;
    
    private Vector3[] m_slotPositions;
    private List<GameObject> m_slotBackgrounds = new List<GameObject>();

    public void Setup(GameManager gameManager, Board board, GameSettings settings)
    {
        m_gameManager = gameManager;
        m_board = board;
        m_capacity = settings.TrayCapacity;

        m_slotPositions = new Vector3[m_capacity];
        float spacing = 1.2f;
        float startX = -(m_capacity - 1) * spacing * 0.5f;
        float bottomY = -4.5f;

        GameObject slotPrefab = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);

        for (int i = 0; i < m_capacity; i++)
        {
            m_slotPositions[i] = new Vector3(startX + i * spacing, bottomY, 0);

            if (slotPrefab != null)
            {
                GameObject slot = Instantiate(slotPrefab, m_slotPositions[i], Quaternion.identity, this.transform);
                // Disable the Cell component so it doesn't interfere with click detection
                Cell cellComp = slot.GetComponent<Cell>();
                if (cellComp != null) cellComp.enabled = false;
                // Disable the BoxCollider so tray slot backgrounds don't intercept board raycasts
                Collider2D col = slot.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
                m_slotBackgrounds.Add(slot);
            }
        }
    }

    public void AddItem(Item item)
    {
        if (m_trayItems.Count >= m_capacity)
        {
            return;
        }

        m_trayItems.Add(item);
        item.SetSortingLayerHigher();
        item.EnableCollider(true);
        
        int targetSlot = m_trayItems.Count - 1;
        item.AnimationMoveToTray(m_slotPositions[targetSlot], () => {
            CheckForMatch();
        });
        
        for (int i = 0; i < m_trayItems.Count - 1; i++)
        {
            m_trayItems[i].View.DOMove(m_slotPositions[i], 0.2f);
        }
    }

    public void ReturnItem(Item item)
    {
        if (!m_trayItems.Contains(item)) return;
        
        m_trayItems.Remove(item);
        item.SetSortingLayerLower();
        item.EnableCollider(false);

        if (item.OriginalCell != null)
        {
            item.OriginalCell.Assign(item);
            item.View.DOKill();
            item.View.DOMove(item.OriginalCell.transform.position, 0.3f).SetEase(Ease.OutBack);
        }
        
        for (int i = 0; i < m_trayItems.Count; i++)
        {
            m_trayItems[i].View.DOMove(m_slotPositions[i], 0.2f);
        }
    }

    public bool HandleTrayItemClick(Transform viewTransform)
    {
        if (m_gameManager.CurrentMode != GameManager.eLevelMode.TIME_ATTACK)
            return false;

        foreach (var item in m_trayItems)
        {
            if (item.View == viewTransform)
            {
                ReturnItem(item);
                return true;
            }
        }
        return false;
    }

    private void UpdateTrayPositions(Action onComplete = null)
    {
        for (int i = 0; i < m_trayItems.Count; i++)
        {
            Item item = m_trayItems[i];
            bool isLast = (i == m_trayItems.Count - 1);
            
            if (isLast && onComplete != null)
            {
                item.View.DOMove(m_slotPositions[i], 0.3f).OnComplete(() => onComplete());
            }
            else
            {
                item.View.DOMove(m_slotPositions[i], 0.3f);
            }
        }
        
        if (m_trayItems.Count == 0 && onComplete != null)
        {
            onComplete();
        }
    }

    private void CheckForMatch()
    {
        var groups = m_trayItems
            .Where(i => i is NormalItem)
            .Cast<NormalItem>()
            .GroupBy(i => i.ItemType)
            .FirstOrDefault(g => g.Count() >= 3);

        if (groups != null)
        {
            var matchedItems = groups.Take(3).Cast<Item>().ToList();

            foreach (var item in matchedItems)
            {
                m_trayItems.Remove(item);
                item.ExplodeView();
            }

            UpdateTrayPositions(() => {
                CheckWinLoseCondition();
            });
        }
        else
        {
            CheckWinLoseCondition();
        }
    }

    private void CheckWinLoseCondition()
    {
        if (m_board.IsEmpty() && m_trayItems.Count == 0)
        {
            m_gameManager.GameOver(true);
        }
        else if (m_trayItems.Count >= m_capacity && m_gameManager.CurrentMode != GameManager.eLevelMode.TIME_ATTACK)
        {
            m_gameManager.GameOver(false);
        }
    }

    public void Clear()
    {
        foreach (var item in m_trayItems)
        {
            if (item != null)
            {
                item.Clear();
            }
        }
        m_trayItems.Clear();

        foreach (var slot in m_slotBackgrounds)
        {
            if (slot != null) Destroy(slot);
        }
        m_slotBackgrounds.Clear();
    }
}
