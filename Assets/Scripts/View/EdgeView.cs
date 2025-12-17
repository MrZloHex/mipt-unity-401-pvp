using Unity.Netcode;
using UnityEngine;

public class EdgeView : NetworkBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    
    public readonly NetworkVariable<Vector3> StartPoint = new NetworkVariable<Vector3>();
    public readonly NetworkVariable<Vector3> EndPoint = new NetworkVariable<Vector3>();

    private void Awake()
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        UpdateLine();
        StartPoint.OnValueChanged += (p, c) => UpdateLine();
        EndPoint.OnValueChanged += (p, c) => UpdateLine();
    }

    private void UpdateLine()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, StartPoint.Value);
            lineRenderer.SetPosition(1, EndPoint.Value);
        }
    }
    
    public void SetPoints(Vector3 start, Vector3 end)
    {
        if (IsServer)
        {
            StartPoint.Value = start;
            EndPoint.Value = end;
        }
    }
}
