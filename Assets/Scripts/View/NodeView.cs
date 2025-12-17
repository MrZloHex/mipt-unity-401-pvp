using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;

public sealed class NodeView : NetworkBehaviour
{
    [SerializeField]
    private TMP_Text valueText;

    private readonly NetworkVariable<int> netValue = new NetworkVariable<int>(1);
    private readonly NetworkVariable<NodeOwner> netOwner = new NetworkVariable<NodeOwner>(NodeOwner.Neutral);
    private readonly NetworkVariable<int> netId = new NetworkVariable<int>(0);

    [SerializeField] private SpriteRenderer spriteRenderer;

    public int Id => netId.Value;
    public NodeOwner Owner => netOwner.Value;

    private NetworkList<NetworkBehaviourReference> neighbors;

    private void Awake()
    {
        neighbors = new NetworkList<NetworkBehaviourReference>();

        if (valueText == null)
        {
            valueText = GetComponentInChildren<TMP_Text>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public override void OnNetworkSpawn()
    {
        netValue.OnValueChanged += (prev, current) => UpdateView();
        netOwner.OnValueChanged += (prev, current) => ApplyOwnerColor();

        UpdateView();
        ApplyOwnerColor();
    }

    private void UpdateView()
    {
        if (valueText != null)
        {
            valueText.text = netValue.Value.ToString();
        }
    }

    public void DebugClick()
    {
        if (IsServer)
        {
            netValue.Value += 1;
            Debug.Log($"Node {Id} clicked. New value: {netValue.Value}");
        }
    }

    public void AddValue(int delta)
    {
        if (IsServer)
        {
            netValue.Value += delta;
        }
        else
        {
            RequestAddValueRpc(delta);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestAddValueRpc(int delta)
    {
        netValue.Value += delta;
    }

    public int Value => netValue.Value;

    public void SubValue(int delta)
    {
        if (IsServer)
        {
            int newValue = netValue.Value - delta;
            if (newValue < 0)
            {
                newValue = 0;
            }
            netValue.Value = newValue;
        }
        else
        {
            RequestSubValueRpc(delta);
        }
    }

    public void SetValue(int v)
    {
        netValue.Value = v;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestSubValueRpc(int delta)
    {
        SubValue(delta);
    }

    public void Init(int id)
    {
        if (IsServer)
        {
            netId.Value = id;
        }
    }

    public void SetOwner(NodeOwner owner)
    {
        if (IsServer)
        {
            netOwner.Value = owner;
        }
        else
        {
            RequestSetOwnerRpc(owner);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestSetOwnerRpc(NodeOwner owner)
    {
        SetOwner(owner);
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

    public bool HasNeighbor(NodeView other)
    {
        foreach (var neighborRef in neighbors)
        {
            if (neighborRef.TryGet(out NodeView neighborNode))
            {
                if (neighborNode == other) return true;
            }
        }
        return false;
    }

    public void AddNeighbor(NodeView other)
    {
        if (!IsServer) return;

        if (other == null || other == this)
        {
            return;
        }

        foreach (var neighborRef in neighbors)
        {
            if (neighborRef.TryGet(out NodeView neighborNode))
            {
                if (neighborNode == other) return;
            }
        }

        neighbors.Add(new NetworkBehaviourReference(other));
    }

    public bool HasNeighborOwnedBy(NodeOwner owner)
    {
        foreach (var neighborRef in neighbors)
        {
            if (neighborRef.TryGet(out NodeView neighborNode))
            {
                if (neighborNode.Owner == owner)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void Attack(int damage, NodeOwner attacker)
    {
        if (IsServer)
        {
            int newValue = netValue.Value - damage;
            
            if (newValue == 0)
            {
                netValue.Value = 0;
                netOwner.Value = NodeOwner.Neutral;
            }
            else if (newValue < 0)
            {
                netValue.Value = Mathf.Abs(newValue);
                netOwner.Value = attacker;
            }
            else
            {
                netValue.Value = newValue;
            }
        }
        else
        {
            RequestAttackRpc(damage, attacker);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestAttackRpc(int damage, NodeOwner attacker)
    {
        Attack(damage, attacker);
    }
}
