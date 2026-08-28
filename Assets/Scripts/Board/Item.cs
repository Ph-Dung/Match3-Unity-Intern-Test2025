using System;
using UnityEngine;
using DG.Tweening;

[Serializable]
public class Item
{
    public Cell Cell { get; private set; }
    public Cell OriginalCell { get; private set; }

    public Transform View { get; private set; }


    public virtual void SetView()
    {
        string prefabname = GetPrefabName();

        if (!string.IsNullOrEmpty(prefabname))
        {
            GameObject prefab = Resources.Load<GameObject>(prefabname);
            if (prefab)
            {
                View = GameObject.Instantiate(prefab).transform;

                if (View.GetComponent<Collider2D>() == null)
                {
                    var col = View.gameObject.AddComponent<CircleCollider2D>();
                    col.radius = 0.45f;
                    col.enabled = false;
                }
            }
        }
    }

    protected virtual string GetPrefabName() { return string.Empty; }

    public virtual void SetCell(Cell cell)
    {
        Cell = cell;
        if (cell != null) 
        {
            OriginalCell = cell;
        }
    }

    public void EnableCollider(bool enable)
    {
        if (View == null) return;
        var col = View.GetComponent<Collider2D>();
        if (col != null) col.enabled = enable;
    }

    internal void AnimationMoveToPosition()
    {
        if (View == null) return;
        View.DOMove(Cell.transform.position, 0.2f);
    }

    internal void AnimationMoveToTray(Vector3 targetPosition, System.Action onComplete = null)
    {
        if (View == null) return;
        View.DOKill();
        View.DOMove(targetPosition, 0.3f).SetEase(Ease.OutBack)
            .OnComplete(() => {
                View.DOPunchScale(Vector3.one * 0.15f, 0.15f);
                onComplete?.Invoke();
            });
    }

    public void SetViewPosition(Vector3 pos)
    {
        if (View)
        {
            View.position = pos;
        }
    }

    public void SetViewRoot(Transform root)
    {
        if (View)
        {
            View.SetParent(root);
        }
    }

    public void SetSortingLayerHigher()
    {
        if (View == null) return;

        SpriteRenderer sp = View.GetComponent<SpriteRenderer>();
        if (sp)
        {
            sp.sortingOrder = 1;
        }
    }


    public void SetSortingLayerLower()
    {
        if (View == null) return;

        SpriteRenderer sp = View.GetComponent<SpriteRenderer>();
        if (sp)
        {
            sp.sortingOrder = 0;
        }

    }

    internal void ShowAppearAnimation()
    {
        if (View == null) return;

        Vector3 scale = View.localScale;
        View.localScale = Vector3.one * 0.1f;
        View.DOScale(scale, 0.1f);
    }

    internal virtual bool IsSameType(Item other)
    {
        return false;
    }

    internal virtual void ExplodeView()
    {
        if (View)
        {
            View.DOScale(0f, 0.2f).SetEase(Ease.InBack).OnComplete(
                () =>
                {
                    GameObject.Destroy(View.gameObject);
                    View = null;
                }
                );
        }
    }



    internal void AnimateForHint()
    {
        if (View)
        {
            View.DOPunchScale(View.localScale * 0.1f, 0.1f).SetLoops(-1);
        }
    }

    internal void StopAnimateForHint()
    {
        if (View)
        {
            View.DOKill();
        }
    }

    internal void Clear()
    {
        Cell = null;

        if (View)
        {
            GameObject.Destroy(View.gameObject);
            View = null;
        }
    }
}
