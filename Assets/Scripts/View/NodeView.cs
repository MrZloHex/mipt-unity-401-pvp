using UnityEngine;
using TMPro;
using System.Collections.Generic;

public sealed class NodeView : MonoBehaviour
{
    [SerializeField]
    private TMP_Text valueText;

    [SerializeField]
    private int value = 1;

    [SerializeField] private SpriteRenderer spriteRenderer;

    public int Id { get; private set; }
    public NodeOwner Owner { get; private set; } = NodeOwner.Neutral;

    private void Awake()
    {
        if (valueText == null)
        {
            valueText = GetComponentInChildren<TMP_Text>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        ApplyOwnerColor();

        UpdateView();
    }

    private void UpdateView()
    {
        if (valueText != null)
        {
            valueText.text = value.ToString();
        }
    }

    public void DebugClick()
    {
        value += 1;
        UpdateView();
        Debug.Log($"Node clicked. New value = {value}");
    }

    public void AddValue(int delta)
    {
        value += delta;
        UpdateView();
    }

    public int Value => value;

    public void SubValue(int delta)
    {
        value -= delta;
        if (value < 0)
        {
            value = 0;
        }
        UpdateView();
    }

    public void Init(int id)
    {
        Id = id;
    }

    public void SetOwner(NodeOwner owner)
    {
        Owner = owner;
        ApplyOwnerColor();
    }

    public bool CanBeClickedBy(NodeOwner player)
    {
        return Owner == player;
    }

    private void ApplyOwnerColor()
    {
        if (spriteRenderer == null) return;

        switch (Owner)
        {
            case NodeOwner.Player1: spriteRenderer.color = new Color(0.35f, 0.75f, 1.0f); break; // голубой
            case NodeOwner.Player2: spriteRenderer.color = new Color(1.0f, 0.45f, 0.45f); break; // красный
            default:                spriteRenderer.color = Color.white; break;                   // нейтрал
        }
    }

    private readonly List<NodeView> neighbors = new List<NodeView>();

    public bool HasNeighbor(NodeView other)
    {
        return neighbors.Contains(other);
    }

    public void AddNeighbor(NodeView other)
    {
        if (other == null || other == this)
        {
            return;
        }

        if (!neighbors.Contains(other))
        {
            neighbors.Add(other);
        }
    }

    public bool HasNeighborOwnedBy(NodeOwner owner)
    {
        for (int i = 0; i < neighbors.Count; i++)
        {
            if (neighbors[i] != null && neighbors[i].Owner == owner)
            {
                return true;
            }
        }
        return false;
    }



}
