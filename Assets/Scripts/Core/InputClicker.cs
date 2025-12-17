using UnityEngine;

public sealed class InputClicker : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private int clickPower = 1;

    [SerializeField]
    private NodeOwner localPlayer = NodeOwner.Player1;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        Vector3 world = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 p = new Vector2(world.x, world.y);

        RaycastHit2D hit = Physics2D.Raycast(p, Vector2.zero);
        if (hit.collider == null)
        {
            return;
        }

        NodeView node = hit.collider.GetComponent<NodeView>();
        if (node == null)
        {
            return;
        }

        // 1. Если это МОЙ узел — усиливаем
        if (node.Owner == localPlayer)
        {
            node.AddValue(clickPower);
            return;
        }

        // 2. Если узел ЧУЖОЙ или НЕЙТРАЛЬНЫЙ — проверяем атаку по соседям
        if (node.HasNeighborOwnedBy(localPlayer))
        {
            node.Attack(clickPower, localPlayer);
        }

    }
    
    public void SetLocalPlayer(NodeOwner player)
    {
        localPlayer = player;
    }
    
    public NodeOwner LocalPlayer => localPlayer;
}

