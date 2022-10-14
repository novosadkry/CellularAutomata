using System;
using UnityEngine;

public class GridScript : MonoBehaviour
{
    public event Action OnGameUpdate;
    public event Action<Vector2Int, bool> OnInputUpdate;
    
    [field: Header("Dimensions")]
    [field: SerializeField] public Vector2Int Size { get; set; }
    [field: SerializeField] public Vector2Int Scale { get; set; }
    
    [Header("Settings")]
    public int editRadius;
    public float editZoom;
    public Vector2 editOffset;
    
    [Header("Timings")]
    public ClockScript inputClock;
    public ClockScript gameClock;

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;

        inputClock.OnTick += OnInputTick;
        gameClock.OnTick += OnGameTick;
    }

    private void Update()
    {
        if (!Input.GetKey(KeyCode.LeftControl))
        {
            editRadius += Mathf.RoundToInt(Input.mouseScrollDelta.y);
            editRadius = Mathf.Max(editRadius, 0);
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
            gameClock.paused = !gameClock.paused;
    }

    private void OnInputTick()
    {
        if (Input.GetKey(KeyCode.LeftControl))
            return;
        
        if (Input.GetMouseButton(0))
        {
            var cell = GetCellFromMousePos();
            OnInputUpdate?.Invoke(cell, true);
        }

        if (Input.GetMouseButton(1))
        {
            var cell = GetCellFromMousePos();
            OnInputUpdate?.Invoke(cell, false);
        }
    }
    
    private void OnGameTick()
    {
        OnGameUpdate?.Invoke();
    }

    private Vector2Int GetCellFromMousePos()
    {
        float xRatio = (float) _cam.pixelWidth / Scale.x / editZoom;
        float yRatio = (float) _cam.pixelHeight / Scale.y / editZoom;
        
        var cellScale = GetCellScale();

        var offsetX = editOffset.x * _cam.pixelWidth / editZoom;
        var offsetY = editOffset.y * _cam.pixelHeight / editZoom;
        
        int x = Mathf.FloorToInt((Input.mousePosition.x + offsetX) / (cellScale.x * xRatio));
        int y = Mathf.FloorToInt((Input.mousePosition.y + offsetY) / (cellScale.y * yRatio));

        return new Vector2Int(x, y);
    }

    public Vector2 GetCellScale()
    {
        float x = (float) Scale.x / Size.x;
        float y = (float) Scale.y / Size.y;

        return new Vector2(x, y);
    }
}
