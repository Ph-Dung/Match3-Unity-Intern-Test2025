using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    private int boardSizeX;
    private int boardSizeY;
    private Cell[,] m_cells;
    private Transform m_root;
    private GameSettings m_settings;

    public Board(Transform transform, GameSettings gameSettings)
    {
        m_root = transform;
        m_settings = gameSettings;
        this.boardSizeX = gameSettings.BoardSizeX;
        this.boardSizeY = gameSettings.BoardSizeY;

        m_cells = new Cell[boardSizeX, boardSizeY];

        CreateBoard();
    }

    private void CreateBoard()
    {
        Vector3 origin = new Vector3(-boardSizeX * 0.5f + 0.5f, -boardSizeY * 0.5f + 0.5f, 0f);
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                GameObject go = GameObject.Instantiate(prefabBG);
                go.transform.position = origin + new Vector3(x, y, 0f);
                go.transform.SetParent(m_root);

                Cell cell = go.GetComponent<Cell>();
                cell.Setup(x, y);

                m_cells[x, y] = cell;
            }
        }

        //set neighbours
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (y + 1 < boardSizeY) m_cells[x, y].NeighbourUp = m_cells[x, y + 1];
                if (x + 1 < boardSizeX) m_cells[x, y].NeighbourRight = m_cells[x + 1, y];
                if (y > 0) m_cells[x, y].NeighbourBottom = m_cells[x, y - 1];
                if (x > 0) m_cells[x, y].NeighbourLeft = m_cells[x - 1, y];
            }
        }
    }

    internal void Fill()
    {
        int totalCells = boardSizeX * boardSizeY;
        int groupCount = totalCells / 3;

        List<NormalItem.eNormalType> generatedTypes = new List<NormalItem.eNormalType>();

        // We assume 7 types max as per eNormalType
        Array enumValues = Enum.GetValues(typeof(NormalItem.eNormalType));

        for (int i = 0; i < groupCount; i++)
        {
            NormalItem.eNormalType typeToAdd;
            if (i < enumValues.Length)
            {
                typeToAdd = (NormalItem.eNormalType)enumValues.GetValue(i);
            }
            else
            {
                typeToAdd = (NormalItem.eNormalType)enumValues.GetValue(UnityEngine.Random.Range(0, enumValues.Length));
            }
            
            generatedTypes.Add(typeToAdd);
            generatedTypes.Add(typeToAdd);
            generatedTypes.Add(typeToAdd);
        }

        // Shuffle generated types
        for (int i = 0; i < generatedTypes.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(0, generatedTypes.Count);
            var temp = generatedTypes[i];
            generatedTypes[i] = generatedTypes[rnd];
            generatedTypes[rnd] = temp;
        }

        int typeIndex = 0;

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (typeIndex >= generatedTypes.Count) break;

                Cell cell = m_cells[x, y];
                NormalItem item = new NormalItem();

                item.SetType(generatedTypes[typeIndex]);
                item.SetView();
                item.SetViewRoot(m_root);

                cell.Assign(item);
                cell.ApplyItemPosition(false);

                typeIndex++;
            }
        }
    }

    public bool IsEmpty()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (m_cells[x, y] != null && !m_cells[x, y].IsEmpty)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void Clear()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                if (cell != null)
                {
                    cell.Clear();
                    GameObject.Destroy(cell.gameObject);
                }
                m_cells[x, y] = null;
            }
        }
    }

    public Cell GetRandomNotEmptyCell()
    {
        List<Cell> notEmptyCells = new List<Cell>();
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (!m_cells[x, y].IsEmpty)
                {
                    notEmptyCells.Add(m_cells[x, y]);
                }
            }
        }
        
        if (notEmptyCells.Count == 0) return null;
        return notEmptyCells[UnityEngine.Random.Range(0, notEmptyCells.Count)];
    }

    public List<Cell> GetAllCellsWithItemType(NormalItem.eNormalType type)
    {
        List<Cell> matchingCells = new List<Cell>();
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (!m_cells[x, y].IsEmpty && m_cells[x, y].Item is NormalItem normalItem)
                {
                    if (normalItem.ItemType == type)
                    {
                        matchingCells.Add(m_cells[x, y]);
                    }
                }
            }
        }
        return matchingCells;
    }

    /// <summary>
    /// Finds the Cell that owns an item whose view Transform matches the given transform.
    /// Used to map a raycast hit on an item sprite back to its board cell.
    /// </summary>
    public Cell FindCellByItemView(Transform viewTransform)
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                if (!cell.IsEmpty && cell.Item.View == viewTransform)
                {
                    return cell;
                }
            }
        }
        return null;
    }
}
